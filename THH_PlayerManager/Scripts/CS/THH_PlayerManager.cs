
using Amazon.S3.Model;
using System.Net;
using UdonSharp;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms;
using VRC.SDKBase;
using VRC.Udon;

public class THH_PlayerManager : UdonSharpBehaviour
{
    //public Debug_THH_PlayerManager TEMP_DEBUG;

    [HideInInspector]
    public VRCPlayerApi Master;

    [HideInInspector]
    public int playerCount;
    // Variable Size = playerCount, no null gaps
    [HideInInspector]
    public VRCPlayerApi[] cleanPlayerList;

    // Constant Size = handlerCount, null gaps
    private VRCPlayerApi[] playerList;

    private VRCPlayerApi[] handlerOwnerList;

    [HideInInspector]
    public THH_PlayerObjectHandler assignedHandler;
    [HideInInspector]
    public THH_PlayerObjectHandler masterHandler;
    [HideInInspector]
    public THH_PlayerObjectHandler[] handlers;
    [HideInInspector]
    public int handlerCount;

    private bool isMaster;
    private bool processingQueue;

    private VRCPlayerApi[] playerQueue;
    private int queueCapacity = 40;
    private int queueSize;
    private int queueStart;

    [UdonSynced, HideInInspector]
    public int ownershipTargetID = -1;
    private int last_ownershipTargetID;
    private THH_PlayerObjectHandler ownershipTargetHandler;

    [HideInInspector]
    public VRCPlayerApi customEventPlayerTarget;
    [HideInInspector]
    public UdonBehaviour customEventUdonTarget;
    [HideInInspector]
    public string customEventName;

    public void SendCustomNetworkEventToPlayer()
    {
        UpdateHandlerOwnerList();
        for (int i = 0; i < handlerCount; i++)
        {
            if (handlerOwnerList[i] == customEventPlayerTarget)
            {                
                THH_PlayerObjectHandler handler = handlers[i];
                VRCPlayerApi handlerOwner = Networking.GetOwner(handler.gameObject);
                if (handlerOwner != customEventPlayerTarget)
                {
                    Debug.LogError($"<color=green>[THH_PlayerManager]</color> Intended to send custom event {customEventUdonTarget.name}:{customEventName} to {customEventPlayerTarget.displayName}, however owner of handler {handler.name} is {handlerOwner.displayName}; Aborting event call");
                    return;
                }
                handler.customEventName = customEventName;
                handler.customEventUdonTarget = customEventUdonTarget;
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Sending custom event {customEventUdonTarget.name}:{customEventName} to {customEventPlayerTarget.displayName}");
                handler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "SendCustomNetworkEventToPlayer");
                return;
            }
        }
        Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Could not find matching player {customEventPlayerTarget.displayName} in handlerOwnerList, maybe its not initialised yet?");
    }

    public void Start()
    {
        Master = Networking.GetOwner(gameObject);
        handlers = transform.GetComponentsInChildren<THH_PlayerObjectHandler>();
        handlerCount = handlers.Length;
        playerList = new VRCPlayerApi[handlerCount];
        handlerOwnerList = new VRCPlayerApi[handlerCount];

        Debug.Log($"<color=green>[THH_PlayerManager]</color> Amount of available handlers: {handlerCount}");
        playerQueue = new VRCPlayerApi[queueCapacity];

        // If youre the master automatically assign the first handler
        if (Networking.IsMaster)
        {
            assignedHandler = handlers[0];
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Assigning handler {assignedHandler.name} to master");
            masterHandler = assignedHandler;
            isMaster = true;
        }
        else
        {
            GetMasterHandler();
        }
        //TEMP_DEBUG.gameObject.SetActive(true);
    }

    public void Update()
    {
        if (ownershipTargetID != last_ownershipTargetID && ownershipTargetID == Networking.LocalPlayer.playerId)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Received and confirmed ownership target ID");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "OwnershipTargetReady");
        }
        last_ownershipTargetID = ownershipTargetID;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (playerCount == handlers.Length)
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color>: Tracked player limit reached, player '{player.displayName}' cannot be processed.");
            return;
        }

        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i] == null)
            {
                playerList[i] = player;
                playerCount++;
                CleanPlayerList();
                break;
            }
        }
        // Player that joined, has joined after you
        if (player.playerId > Networking.LocalPlayer.playerId)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Adding {player.displayName} to the queue");
            AddToQueue(player);
            ProcessQueue();
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        // Are you the new master?
        if (Networking.IsMaster && !isMaster)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Last master has left, I am the new master.");
            isMaster = true;
            assignedHandler.ownedByNonMaster = false;
            BroadcastMasterHandler();
        }

        // Remove the player that has left from the player list
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i] == player)
            {
                playerList[i] = null;
                break;
            }
        }

        playerCount--;
        CleanPlayerList();
    }

    void GetMasterHandler()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Retrieving master handler");
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "BroadcastMasterHandler");
    }

    // Event should only be called for the master!
    public void BroadcastMasterHandler()
    {
        if (!Networking.IsMaster)
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Unauthorized event call: BroadcastMasterHandler; You are not master!");
            return;
        }

        assignedHandler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "SetMasterHandler");
    }

    public void OwnershipTargetReady()
    {
        ownershipTargetHandler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "TakeOwnership");
    }

    public void ProcessQueue()
    {
        if (processingQueue) 
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Queue is currently being processed already.");
            return; 
        }
        processingQueue = true;
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Processing queue...");
        if (masterHandler == null)
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Master handler has not yet been assigned, aborting queue proccessing.");
            processingQueue = false;
            return;
        }

        VRCPlayerApi player = GetPlayerFromQueue();

        if (player == null)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> No player in queue or player was null");
            processingQueue = false;
            return;
        }

        for (int i = 0; i < handlers.Length; i++)
        {
            THH_PlayerObjectHandler handler = handlers[i];
            if (Networking.GetOwner(handler.gameObject).isMaster && handler != masterHandler && !handler.blocked)
            {
                handler.blocked = true;
                ownershipTargetHandler = handler;
                handler.ownedByNonMaster = true;
                if (Networking.IsMaster)
                {
                    Debug.Log($"<color=green>[THH_PlayerManager]</color> Assigning handler {handler.name} to {player.displayName}");
                    // Does not work because bugs: Networking.SetOwner(player, handler.gameObject);
                    ownershipTargetID = player.playerId;
                }
                processingQueue = false;
                RemoveFromQueue();
                return;
            }
            else
            {
                handler.blocked = true;
            }
        }
        Debug.LogError($"<color=green>[THH_PlayerManager]</color> Could not find an unassigned handler, {player.displayName} cannot be processed");
        processingQueue = false;
        RemoveFromQueue();
    }

    public void UpdateHandlerOwnerList()
    {
        if (masterHandler == null)
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Master handler has not yet been assigned, aborting UpdateHandlerOwnerList");
            return;
        }
        for (int i = 0; i < handlerCount; i++)
        {
            THH_PlayerObjectHandler handler = handlers[i];
            VRCPlayerApi owner = Networking.GetOwner(handler.gameObject);
            if (owner.isMaster)
            {
                if (handler == masterHandler)
                {
                    handlerOwnerList[i] = owner;
                }
            }
            else
            {
                handlerOwnerList[i] = owner;
            }
        }
        logOwnerHandlerList();
    }

    void CleanPlayerList()
    {
        cleanPlayerList = new VRCPlayerApi[playerCount];
        int c = 0;
        foreach (VRCPlayerApi player in playerList)
        {
            if (player == null) { continue; }
            cleanPlayerList[c] = player;
            c++;
        }
    }

    public void AddToQueue(VRCPlayerApi player)
    {
        playerQueue[(queueStart + queueSize) % queueCapacity] = player;
        if (queueSize < queueCapacity)
        {
            queueSize++;
        }
        else
        {
            queueStart = (queueStart + 1) % queueCapacity;
        }
    }

    public void RemoveFromQueue()
    {
        while (queueSize > 0 && playerQueue[queueStart] == null)
        {
            queueStart = (queueStart + 1) % queueCapacity;
            queueSize--;
        }
        if (queueSize > 0)
        {
            playerQueue[queueStart] = null;
            queueStart = (queueStart + 1) % queueCapacity;
            queueSize--;
        }
    }

    public VRCPlayerApi GetPlayerFromQueue()
    {
        while (queueSize > 0 && playerQueue[queueStart] == null)
        {
            queueStart = (queueStart + 1) % queueCapacity;
            queueSize--;
        }
        if (queueSize > 0)
        {
            return playerQueue[queueStart];
        }
        return null;
    }

    public void logQueue()
    {
        for (int i = 0; i < queueSize; i++)
        {
            Debug.Log(playerQueue[(queueStart + i) % queueCapacity].displayName);
        }
    }

    public void logOwnerHandlerList()
    {
        Debug.Log($"LOGGING OWNER HANDLER LIST:");
        for (int i = 0; i < handlerCount; i++)
        {
            if (handlerOwnerList[i] == null)
            {
                Debug.Log($"{i}: NULL");
            }
            else
            {
                Debug.Log($"{i}: {handlerOwnerList[i].displayName}");
            }
        }
    }
}
