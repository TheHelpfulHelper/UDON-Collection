
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class CHAT_DEBUG : UdonSharpBehaviour
{
    public THH_ChatManager manager;

    public Text Guardian;
    public Text Ward;

    public Text IsHead;
    public Text IsTail;

    public Text MasterHandler;
    public Text Handler;

    public Text MessengerOwner;

    public Text Message;
    public Text LastMessage;

    public void Update()
    {
        Guardian.text = manager.Guardian == null ? "Guardian: NULL" : $"Guardian: {manager.Guardian.displayName}({manager.Guardian.playerId})";
        Ward.text = manager.Ward == null ? "Ward: NULL" : $"Ward: {manager.Ward.displayName}({manager.Ward.playerId})";

        IsHead.text = $"IsHead: {manager.isHead}";
        IsTail.text = $"IsTail: {manager.isTail}";

        MasterHandler.text = manager.masterHandler == null ? "Master Handler: NULL" : $"Master Handler: {manager.masterHandler.name}";
        string assignedHandler = manager.assignedHandler == null ? "NULL" : $"{manager.assignedHandler.name}";
        Handler.text = $"Handler: {manager.handlerAssigned}, {assignedHandler}";

        VRCPlayerApi messengerOwner = Networking.GetOwner(manager.messenger.gameObject);
        MessengerOwner.text = $"Messenger Owner: {messengerOwner.displayName}({messengerOwner.playerId})";

        Message.text = $"Message: {manager.messenger.MESSAGE}";
        LastMessage.text = $"{manager.messenger.last_MESSAGE}";
    }
}
