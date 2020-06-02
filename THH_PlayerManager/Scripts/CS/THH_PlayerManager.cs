
using UdonSharp;
using UnityEngine;
using UnityEngine.Networking;
using VRC.SDKBase;
using VRC.Udon;

public class THH_PlayerManager : UdonSharpBehaviour
{
    [HideInInspector]
    public THH_PlayerObjectHandler[] handlers;
    [HideInInspector]
    public int handlerCount;

    private VRCPlayerApi[] LateJoiners;
    private int LateJoinersCount;

    private VRCPlayerApi ward;
    private VRCPlayerApi guardian;

    [HideInInspector]
    public THH_PlayerObjectHandler masterHandler;
    [HideInInspector]
    public THH_PlayerObjectHandler assignedHandler;
    private bool handlerAssigned;
    private bool assignmentDelayed;

    void Start()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> THH_PlayerManager v2.2 initialized");
        handlers = GetComponentsInChildren<THH_PlayerObjectHandler>();
        foreach (THH_PlayerObjectHandler handler in handlers)
        {
            handler.Initialize();
        }
        handlerCount = handlers.Length;

        LateJoiners = new VRCPlayerApi[handlerCount];

        if (Networking.IsMaster)
        {
            assignedHandler = handlers[0];
            masterHandler = assignedHandler;
            handlerAssigned = true;
            CheckAllHandlerStatus();
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Assigned handler {assignedHandler.name} as master");
        }
        else
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Requesting master handler...");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "RequestMasterHandler");
        }
    }

    // This event should only be called for the master!
    public void RequestMasterHandler()
    {
        if (!Networking.IsMaster)
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Unauthorized event call: 'RequestMasterHandler'; You are not master!");
            return;
        }

        assignedHandler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "BroadcastMasterHandler");
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // If youre the last player (LateJoinersCount == 0) and you have a handlerAssigned then inform the player that joined that they should check for handler assignment availability
        if (player != Networking.LocalPlayer && handlerAssigned && LateJoinersCount == 0)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> A new player has joined, after finishing assignment; Broadcasting 'CheckHandlerAssignmentAvailability'");
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckHandlerAssignmentAvailability");
        }
        
        // If the player that has joined, joined after you process them as a late-joiner
        if (player.playerId > Networking.LocalPlayer.playerId)
        {
            bool suscessfullyProcessedLateJoiner = ProcessLateJoin(player);
            if (!suscessfullyProcessedLateJoiner)
            {
                Debug.LogError($"<color=green>[THH_PlayerManager]</color> LateJoiner {player.displayName} could not be processed. You may need more Handlers!");
            }
            if (LateJoinersCount == 1)
            {
                ward = player;
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Your ward is now {ward.displayName}");
            }
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        // If the player that has left, joined after you, decrement LateJoinersCount
        if (player.playerId > Networking.LocalPlayer.playerId)
        {
            LateJoinersCount--;
            // If the player was also your ward, find a new ward
            if (player == ward)
            {
                ward = FindWard(player);
                if (ward == null)
                {
                    Debug.Log($"<color=green>[THH_PlayerManager]</color> Your last ward has left, but no new ward was found");
                }
                else
                {
                    Debug.Log($"<color=green>[THH_PlayerManager]</color> Your last ward has left. Your new ward is now {ward.displayName}");
                    // If the new ward does not have a handler yet, inform them that they should check for handler assignment availability
                    if (!PlayerHasHandler(ward))
                    {
                        Debug.Log($"<color=green>[THH_PlayerManager]</color> Your ward did not have a handler assigned yet, broadcasting 'CheckHandlerAssignmentAvailability'");
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckHandlerAssignmentAvailability");
                    }
                }
            }
        }
        //If the player that left had joined before you, then make sure it was not your guardian who left;
        else if (player.playerId < Networking.LocalPlayer.playerId)
        {
            // Only matters if youre not the master, otherwise you will not have a guardian
            if (guardian == null && !Networking.IsMaster)
            {
                guardian = FindGuardian(null);
                if (guardian == null)
                {
                    Debug.LogError($"<color=green>[THH_PlayerManager]</color> Could not find a guardian, something went wrong...");
                    return;
                }
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Your last guardian has left, your new guardian is {guardian.displayName}");
            }
            // If you are the master, then that must mean that the previous master has left, since only they can have a lower playerID than you (being the master)
            else if (Networking.IsMaster)
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> The last master has left, you are the new master");
                assignedHandler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "BroadcastMasterHandler");
            }
        }
    }

    // This event is called for everyone in the instance
    public void CheckHandlerAssignmentAvailability()
    {
        // Could check if youre not master, but should be redundant, since master should always have a handler assigned
        if (!handlerAssigned)
        {
            // If you dont have a guardian yet, try to find one
            if (guardian == null)
            {
                guardian = FindGuardian(null);
                if (guardian == null)
                {
                    Debug.LogError($"<color=green>[THH_PlayerManager]</color> Could not find a guardian, something went wrong...");
                    return;
                }
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Your guardian is now {guardian.displayName}");
            }
            // Only proceed if the guardian actually has a handler assigned yet, otherwise wait for further instructions
            if (PlayerHasHandler(guardian))
            {
                // Cant check for masters handler, if masterHandler is not set yet, wait for it
                if (masterHandler == null)
                {
                    Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Master handler has not yet been set, delaying handler assignment untill it has been set");
                    assignmentDelayed = true;
                }
                else
                {
                    FindUnassignedHandler();
                }
            }
            else
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Your guardian does not have a handler assigned to them yet, waiting for further instructions");
            }
        }
    }

    // Callback
    public void OnMasterHandlerSet()
    {
        if (assignmentDelayed && !handlerAssigned)
        {
            FindUnassignedHandler();
        }
        CheckAllHandlerStatus();
    }

    void CheckAllHandlerStatus()
    {
        foreach (THH_PlayerObjectHandler handler in handlers)
        {
            handler.CheckStatus();
        }
    }

    bool ProcessLateJoin(VRCPlayerApi player)
    {
        for (int i = 0; i < LateJoiners.Length; i++)
        {
            if (LateJoiners[i] == null)
            {
                LateJoiners[i] = player;
                LateJoinersCount++;
                return true;
            }
        }
        return false;
    }

    VRCPlayerApi FindGuardian(VRCPlayerApi playerWhoLeft)
    {
        // Trying every ID decrementally until a player is found (Janky if a lot of players have joined and left the instance)
        int i = Networking.LocalPlayer.playerId - 1;
        while (i > 0)
        {
            VRCPlayerApi player = VRCPlayerApi.GetPlayerById(i);
            if ( player != null)
            {
                return player;
            }
            i--;
        }
        return null;
    }

    VRCPlayerApi FindWard(VRCPlayerApi playerWhoLeft)
    {
        // Can only find a ward if someone joined after you
        if (LateJoinersCount == 0)
        {
            return null;
        }

        VRCPlayerApi _ward = null;
        foreach (VRCPlayerApi lateJoiner in LateJoiners)
        {
            // ignore all nulls and (if specified) the player that left
            if (lateJoiner == null || lateJoiner == playerWhoLeft)
            {
                continue;
            }
            // set first player you find as ward, when ward is null
            if (_ward == null)
            {
                _ward = lateJoiner;
                continue;
            }
            if (lateJoiner.playerId < _ward.playerId)
            {
                _ward = lateJoiner;
                continue;
            }
        }
        return _ward;
    }

    bool PlayerHasHandler(VRCPlayerApi player)
    {
        // Go trough all handlers and check if the handlers owner matches the player
        foreach (THH_PlayerObjectHandler handler in handlers)
        {
            if (Networking.GetOwner(handler.gameObject) == player)
            {
                return true;
            }
        }
        return false;
    }

    void FindUnassignedHandler()
    {
        foreach (THH_PlayerObjectHandler handler in handlers)
        {
            if (Networking.GetOwner(handler.gameObject).isMaster && handler != masterHandler)
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Assigning handler {handler.name}");

                handler.TakeOwnership();

                assignedHandler = handler;
                handlerAssigned = true;
                assignmentDelayed = false;
                return;
            }
        }
        // Could not find a handler
        assignmentDelayed = false;
        Debug.LogError($"<color=green>[THH_PlayerManager]</color> No unassigned handler could be found, cannot assign one!");
    }
}
