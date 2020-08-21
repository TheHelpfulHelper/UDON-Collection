
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class THH_ChatHandler : UdonSharpBehaviour
{
    private THH_ChatManager manager;

    private bool isOwner;

    public void Start()
    {
        manager = transform.parent.GetComponent<THH_ChatManager>();
        isOwner = Networking.IsOwner(gameObject);
    }

    public void SetAsMasterHandler()
    {
        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Set handler '{name}' as master handler.");
        manager.masterHandler = this;
        manager.OnMasterHandlerSet();
    }

    // OnOwnershipTransferred is currently not behaving correctly. This is the code if it were behaving correctly:
    //public override void OnOwnershipTransferred()
    //{
    //    if (isOwner && !Networking.IsOwner(gameObject))
    //    {
    //        // Lost Ownership, normally shouldnt happen
    //    }
    //    else if (!isOwner && Networking.IsOwner(gameObject))
    //    {
    //        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Taken ownership of handler '{name}'!");
    //        isOwner = true;

    //        if(!Networking.IsMaster)
    //        {
    //            manager.handlerAssigned = true;
    //            manager.assignedHandler = this;

    //            manager.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(THH_ChatManager.CheckAssignmentAvailability));
    //        }
    //    }
    //}

    public void Update()
    {
        CheckOwnershipTransferred();
    }

    public void CheckOwnershipTransferred()
    {
        if (isOwner && !Networking.IsOwner(gameObject))
        {
            VRCPlayerApi newOwner = Networking.GetOwner(gameObject);

            Debug.Log($"<color=green>[THH_ChatHandler]</color>: Transferred ownership of handler '{name}' to {newOwner.displayName}({newOwner.playerId})!");
            isOwner = false;
        }
        else if (!isOwner && Networking.IsOwner(gameObject))
        {
            Debug.Log($"<color=green>[THH_ChatHandler]</color>: Taken ownership of handler '{name}'!");
            isOwner = true;

            if (!Networking.IsMaster)
            {
                manager.handlerAssigned = true;
                manager.assignedHandler = this;

                manager.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(THH_ChatManager.CheckAssignmentAvailability));
            }
        }
    }

    public void RequestChat()
    {
        if (!Networking.IsOwner(gameObject)) { return; }

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ChatRequest));
    }

    public void RequestDenied()
    {
        manager.RequestDenied();
    }

    public void ChatRequest()
    {
        if (!Networking.IsOwner(manager.messenger.gameObject)) { return; }

        VRCPlayerApi owner = Networking.GetOwner(gameObject);
        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Handler '{name}' of '{owner.displayName}'({owner.playerId}) has requested ownership of the Chat Messenger.");
        manager.messenger.ChatRequest(this);
    }
}
