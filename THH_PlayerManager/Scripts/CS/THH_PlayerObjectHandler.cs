
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class THH_PlayerObjectHandler : UdonSharpBehaviour
{
    private THH_PlayerManager PlayerManager;
    // This variable is only relevant and accurate for the master; Do not use it unless you know what youre doing
    private bool ownedByMaster = true;
    void Start()
    { 
        GameObject PlayerManagerGO = GameObject.Find("[THH_PlayerManager]");
        if (PlayerManagerGO == null)
        {
            Debug.LogError($"<color=green>[THH_PlayerManager]</color> Could not find Player Manager GameObject");
        }
        PlayerManager = PlayerManagerGO.GetComponent<THH_PlayerManager>();
        if (PlayerManager == null)
        {
            Debug.LogError($"<color=green>[THH_PlayerManager]</color> PlayerManager GameObject has no THH_PlayerManager Component");
        }
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

    public override void OnOwnershipTransferred()
    {
        if (Networking.IsMaster)
        {
            if (Networking.LocalPlayer.IsOwner(gameObject) && !ownedByMaster)
            {
                ownedByMaster = true;
            }
            else if (!Networking.LocalPlayer.IsOwner(gameObject) && ownedByMaster)
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> Ownership has been transferred to {Networking.GetOwner(gameObject).displayName}");
                ownedByMaster = false;
                PlayerManager.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "CheckHandlerAssignmentAvailability");
            }
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.IsMaster)
        {
            // If any player left, check if you are owner again (master)
            if (Networking.LocalPlayer.IsOwner(gameObject) && !ownedByMaster)
            {
                Debug.Log($"<color=green>[THH_PlayerManager]</color> The previous owner of handler {name}: {player.displayName} has left; Ownership has been transferred back to you");
                ownedByMaster = true;
            }
        }
    }
}
