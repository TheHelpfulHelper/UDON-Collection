
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class THH_ChatManager : UdonSharpBehaviour
{
    public THH_ChatMessenger messenger;

    public int maxRetries = 20;

    private THH_ChatHandler[] handlers;

    [System.NonSerialized] public bool requesting;

    [System.NonSerialized] public bool handlerAssigned;
    [System.NonSerialized] public THH_ChatHandler assignedHandler;
    [System.NonSerialized] public THH_ChatHandler masterHandler;

    [System.NonSerialized] public VRCPlayerApi Guardian;
    [System.NonSerialized] public VRCPlayerApi Ward;

    [System.NonSerialized] public bool isHead;
    [System.NonSerialized] public bool isTail;

    private bool isMaster;

    private VRCPlayerApi[] lateJoiners;
    private int lastLateJoinerSlot;

    private VRCPlayerApi lastLeaver;

    private string last_message;
    private bool last_autoRetry;
    private int last_retryCount;

    [System.NonSerialized] public bool attemptingToTransmitMessage;

    public void AttemptToSendChatMessage(string message, bool autoRetry, int retryCount)
    {
        if (attemptingToTransmitMessage) { return; }

        last_message = message;
        last_autoRetry = autoRetry;
        last_retryCount = retryCount;

        if (Networking.IsOwner(messenger.gameObject))
        {
            messenger.SendChatMessage(message);
            attemptingToTransmitMessage = false;
            messenger.logger.Unlock();
        }
        else
        {
            attemptingToTransmitMessage = true;
            messenger.logger.Lock();
            RequestChat();
        }
    }

    public void RequestAccepted()
    {
        requesting = false;
        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Request has been accepted.");
        
        if (attemptingToTransmitMessage)
        {
            messenger.SendChatMessage(last_message);
            attemptingToTransmitMessage = false;
            messenger.logger.Unlock();
        }
    }

    public void RequestDenied()
    {
        requesting = false;
        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Request has been denied.");

        if (attemptingToTransmitMessage)
        {
            if (last_autoRetry && last_retryCount < maxRetries)
            {
                AttemptToSendChatMessage(last_message, last_autoRetry, last_retryCount);
            }
            else
            {
                Debug.Log($"<color=green>[THH_ChatHandler]</color>: Auto-retry reached max retry counts, aborting message transmission.");
                attemptingToTransmitMessage = false;
                messenger.logger.Unlock();
            }
        }
    }


    public void Start()
    {
        GWS_Start();
        handlers = GetComponentsInChildren<THH_ChatHandler>();

        if (Networking.IsMaster)
        {
            handlerAssigned = true;
            assignedHandler = handlers[0];
            masterHandler = assignedHandler;
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(RequestMasterHandler));
            Debug.Log("<color=green>[THH_ChatManager]</color>: Requesting master handler...");
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        GWS_OnPlayerJoined(player);
        CheckAssignmentAvailability();
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        GWS_OnPlayerLeft(player);
        CheckAssignmentAvailability();
    }

    public void RequestMasterHandler()
    {
        if (!Networking.IsMaster) { return; }

        masterHandler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(THH_ChatHandler.SetAsMasterHandler));
    }

    public void OnMasterHandlerSet()
    {
        CheckAssignmentAvailability();
    }

    public THH_ChatHandler GetHandler()
    {
        foreach (THH_ChatHandler handler in handlers)
        {
            if (Networking.GetOwner(handler.gameObject) == Networking.GetOwner(gameObject) && handler != masterHandler)
            {
                return handler;
            }
        }

        Debug.LogError($"<color=green>[THH_ChatManager]</color>: Tried to get a handler, but could not find one that wasnt assigned. Maybe you need more handlers.");
        return null;
    }

    public void CheckAssignmentAvailability()
    {
        if (!handlerAssigned && Guardian != null && masterHandler != null && CheckPlayerForHandler(Guardian))
        {
            THH_ChatHandler handler = GetHandler();
            Debug.Log($"<color=green>[THH_ChatHandler]</color>: Taking ownership of handler '{handler.name}'...");
            Networking.SetOwner(Networking.LocalPlayer, handler.gameObject);
        }
    }

    public bool CheckPlayerForHandler(VRCPlayerApi player)
    {
        foreach (THH_ChatHandler handler in handlers)
        {
            if (player.IsOwner(handler.gameObject))
            {
                return true;
            }
        }

        return false;
    }

    public void RequestChat()
    {
        if (assignedHandler == null || requesting || Networking.IsOwner(messenger.gameObject)) { return; }

        Debug.Log($"<color=green>[THH_ChatManager]</color>: Requesting ownership of Chat Messenger");
        requesting = true;
        assignedHandler.RequestChat();
    }

    #region GuardianWardSystem


    public void GWS_Start()
    {
        if (Networking.IsMaster)
        {
            isMaster = true;
            isHead = true;
            isTail = true;

            Debug.Log("<color=green>[THH_GWS]</color>: You are master.");
        }
        else
        {
            isTail = true;
            Guardian = FindGuardian();
            if (Guardian == null)
            {
                Debug.LogError("<color=green>[THH_GWS]</color>: Tried to find a guardian, but failed. Something went wrong...");
            }
            else
            {
                OnGuardianAssigned();
            }
        }

        lateJoiners = new VRCPlayerApi[80];
    }

    public void GWS_OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.playerId > Networking.LocalPlayer.playerId)
        {
            if (Ward == null)
            {
                Ward = player;
                isTail = false;

                OnWardAssigned();
            }

            ProcessLateJoiner(player);
        }
    }

    public void GWS_OnPlayerLeft(VRCPlayerApi player)
    {
        lastLeaver = player;
        if (Networking.IsMaster && !isMaster)
        {
            Debug.Log("<color=green>[THH_GWS]</color>: Last master left, you are now master.");
            isMaster = true;
            isHead = true;

            // NOT GWS:
            Guardian = null;
            masterHandler = null;

            if (assignedHandler == null) 
            {
                assignedHandler = GetHandler();
                masterHandler = assignedHandler;
                RequestMasterHandler();
            }
            else
            {
                masterHandler = assignedHandler;
                RequestMasterHandler();
            }
        }
        else if (player == Guardian)
        {
            Debug.Log($"<color=green>[THH_GWS]</color>: Your guardian, {player.displayName}({player.playerId}), has left, trying to find a new one...");
            Guardian = FindGuardian();
            if (Guardian == null)
            {
                Debug.LogError("<color=green>[THH_GWS]</color>: Tried to find a guardian, but failed. Something went wrong...");
            }
            else
            {
                OnGuardianAssigned();
            }
        }
        else if (player == Ward)
        {
            Debug.Log($"<color=green>[THH_GWS]</color>: Your ward, {player.displayName}({player.playerId}), has left, trying to find a new one...");
            Ward = FindWard();
            if (Ward == null)
            {
                Debug.Log($"<color=green>[THH_GWS]</color>: Could not find a ward.");
                isTail = true;
            }
            else
            {
                OnWardAssigned();
            }
        }
    }

    private VRCPlayerApi FindGuardian()
    {
        for (int i = Networking.LocalPlayer.playerId - 1; i > 0; i--)
        {
            VRCPlayerApi potentialGuardian = VRCPlayerApi.GetPlayerById(i);
            if (potentialGuardian != null && potentialGuardian != lastLeaver)
            {
                return potentialGuardian;
            }
        }

        return null;
    }

    private VRCPlayerApi FindWard()
    {
        VRCPlayerApi ward = null;

        for (int i = lastLateJoinerSlot; i >= 0; i--)
        {
            VRCPlayerApi potentialWard = lateJoiners[i];
            if (potentialWard == null || potentialWard == lastLeaver) { lastLateJoinerSlot--; continue; }

            if (ward == null || potentialWard.playerId < ward.playerId)
            {
                ward = potentialWard;
            }
        }

        return ward;
    }

    private void ProcessLateJoiner(VRCPlayerApi player)
    {
        for (int i = 0; i < lateJoiners.Length; i++)
        {
            if (lateJoiners[i] == null)
            {
                lateJoiners[i] = player;

                if (i > lastLateJoinerSlot) { lastLateJoinerSlot = i; }

                return;
            }
        }

        Debug.LogError($"<color=green>[THH_GWS]</color>: Could not process later joiner {player.displayName}({player.playerId}), something went wrong...");
    }

    private void OnGuardianAssigned()
    {
        Debug.Log($"<color=green>[THH_GWS]</color>: {Guardian.displayName}({Guardian.playerId}) has been assigned as your guardian.");
        CheckAssignmentAvailability();
    }

    private void OnWardAssigned()
    {
        Debug.Log($"<color=green>[THH_GWS]</color>: {Ward.displayName}({Ward.playerId}) has been assigned as your ward.");
    }
    #endregion
}
