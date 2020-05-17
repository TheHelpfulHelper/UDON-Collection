
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class THH_PlayerObjectHandler : UdonSharpBehaviour
{
    private THH_PlayerManager playerManager;
    [HideInInspector]
    public bool blocked;

    [HideInInspector]
    public UdonBehaviour customEventUdonTarget;
    [HideInInspector]
    public string customEventName;
    [HideInInspector]
    public bool isActive;

    public void SendCustomNetworkEventToPlayer()
    {
        Debug.Log(customEventUdonTarget.name);
        customEventUdonTarget.SendCustomEvent(customEventName);
    }

    void Start()
    {
        playerManager = transform.parent.GetComponent<THH_PlayerManager>();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "BroadcastHandlerState");
    }

    public void SetMasterHandler()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Setting master handler to {name}");
        playerManager.masterHandler = this;
        if (playerManager.masterTransfer)
        {
            playerManager.MasterTransferFinished();
        }
    }

    public void TakeOwnership()
    {
        if (playerManager.ownershipTargetID == Networking.LocalPlayer.playerId)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> You have been assigned handler {name}");
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            playerManager.assignedHandler = this;
            blocked = true;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Activate");
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.GetOwner(gameObject).isMaster)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Unblocking handler {name}");
            blocked = false;
            if (Networking.IsMaster && playerManager.assignedHandler != this)
            {
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Deactivate");
            }
        }
    }

    // This event should only be called by the owner!
    public void BroadcastHandlerState()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Debug.LogWarning($"<color=green>[THH_PlayerManager]</color> Unauthorized event call: BroadcastHandlerState; You are not owner!");
            return;
        }

        if (isActive)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Activate");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Deactivate");
        }
    }

    public void Activate()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Activating handler {name}; Owner is: {Networking.GetOwner(gameObject).displayName}");
        isActive = true;

    }

    public void Deactivate()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Deactivating handler {name}; Owner is: {Networking.GetOwner(gameObject).displayName}");
        isActive = false;
    }
}
