
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
    public bool ownedByNonMaster;

    [HideInInspector]
    public UdonBehaviour customEventUdonTarget;
    [HideInInspector]
    public string customEventName;

    public void SendCustomNetworkEventToPlayer()
    {
        Debug.Log(customEventUdonTarget.name);
        customEventUdonTarget.SendCustomEvent(customEventName);
    }

    void Start()
    {
        playerManager = transform.parent.GetComponent<THH_PlayerManager>();   
    }

    public void SetMasterHandler()
    {
        Debug.Log($"<color=green>[THH_PlayerManager]</color> Setting master handler to {name}");
        playerManager.masterHandler = this;
    }

    public void TakeOwnership()
    {
        if (playerManager.ownershipTargetID == Networking.LocalPlayer.playerId)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> You have been assigned handler {name}");
            Transform[] children = GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                Networking.SetOwner(Networking.LocalPlayer, child.gameObject);
            }
            playerManager.assignedHandler = this;
            blocked = true;
        }
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (Networking.GetOwner(gameObject).isMaster && ownedByNonMaster)
        {
            Debug.Log($"<color=green>[THH_PlayerManager]</color> Unblocking handler {name}");
            ownedByNonMaster = false;
            blocked = false;
        }
    }
}
