
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class THH_PlayerObjectHandler : UdonSharpBehaviour
{
    private THH_PlayerManager PlayerManager;
    [HideInInspector]
    public bool isAssigned;
    // This variable is only relevant and accurate for the master; Do not use it unless you know what youre doing
    private bool ownedByMaster = true;

    public void Initialize()
    { 
        PlayerManager = transform.parent.GetComponent<THH_PlayerManager>();
        if (PlayerManager == null)
        {
            Debug.LogError($"<color=green>[THH_PlayerManager]</color> Parent has no THH_PlayerManager Component");
        }
    }

    public void TakeOwnership()
    {
        Component[] behaviours = transform.GetComponentsInChildren(typeof(UdonBehaviour));
        foreach (Component behaviour in behaviours)
        {
            Networking.SetOwner(Networking.LocalPlayer, behaviour.gameObject);
        }
    }

    public void OnAssign()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Handler {name} was assigned to {Networking.GetOwner(gameObject).displayName}");
    }

    public void OnUnassign()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Handler {name} was unassigned");
    }

    public void BroadcastMasterHandler()
    {
        if (PlayerManager.masterHandler != this)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Master handler set to {name}");
            PlayerManager.masterHandler = this;
            PlayerManager.OnMasterHandlerSet();
        }
    }

    public void CheckStatus()
    {
        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        if (!owner.isMaster || (owner.isMaster && PlayerManager.masterHandler == this))
        {
            SetHandlerAssigned(true);
        }
        else
        {
            SetHandlerAssigned(false);
        }
    }

    public void SetHandlerAssigned(bool status)
    {
        bool prevStatus = isAssigned;
        isAssigned = status;
        if (prevStatus != status)
        {
            Component[] behaviours = GetComponentsInChildren(typeof(UdonBehaviour), true);
            foreach (Component behaviour in behaviours)
            {
                UdonBehaviour udon = (UdonBehaviour)behaviour;
                string method = isAssigned ? "OnAssign" : "OnUnassign";
                udon.SendCustomEvent(method);
            }
        }
    }

    public override void OnOwnershipTransferred()
    {
        if (Networking.IsMaster)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject) && !ownedByMaster)
            {
                // Failsafe
                ownedByMaster = true;
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckStatus");
            }
            else if (!Networking.LocalPlayer.IsOwner(gameObject) && ownedByMaster)
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Ownership has been transferred to {Networking.GetOwner(gameObject).displayName}");
                ownedByMaster = false;
                PlayerManager.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckHandlerAssignmentAvailability");
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckStatus");
            }
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.IsMaster)
        {
            // Last master has left, update ownedByMaster
            if (player.playerId < Networking.LocalPlayer.playerId && Networking.LocalPlayer.IsOwner(gameObject))
            {
                ownedByMaster = true;
            }
            // If any player left, check if you are owner again (master)
            if (Networking.LocalPlayer.IsOwner(gameObject) && !ownedByMaster)
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> The previous owner of handler {name}: {player.displayName} has left; Ownership has been transferred back to you");
                ownedByMaster = true;
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckStatus");
            }
        }
    }
}
