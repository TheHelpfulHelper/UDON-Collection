
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class THH_PlayerObjectHandler : UdonSharpBehaviour
{
    private THH_PlayerManager PlayerManager;
    [HideInInspector]
    public bool isActive;
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
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
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
            SetHandlerActive(true);
        }
        else
        {
            SetHandlerActive(false);
        }
    }

    public void SetHandlerActive(bool status)
    {
        //Debug.Log($"<color=green>[THH_PlayerManager]</color> Setting Active status of handler {name} to {status}");
        isActive = status;
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
