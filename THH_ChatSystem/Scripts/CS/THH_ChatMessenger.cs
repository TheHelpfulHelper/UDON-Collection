using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class THH_ChatMessenger : UdonSharpBehaviour
{
    public THH_ChatManager manager;
    public THH_ChatLog logger;

    private bool isOwner;
    private bool transferringOwnership;

    [System.NonSerialized, UdonSynced(UdonSyncMode.None)] public string MESSAGE = "UNINITIALIZED";
    private string last_MESSAGE = "UNINITIALIZED";
    private bool syncStringInitialized;

    private int MessageCounter;

    public void Chat()
    {
        if (manager.attemptingToTransmitMessage) { return; }

        manager.AttemptToSendChatMessage(logger.inputField.text, true, 0);
    }

    public void SendChatMessage(string message)
    {
        if (!Networking.IsOwner(gameObject)) { return; }

        string PID = Networking.LocalPlayer.playerId.ToString("X");
        string MID = MessageCounter.ToString();

        MESSAGE = $"{MID}_{PID}_{message}";

        MessageCounter = (MessageCounter + 1) % 10;

        OnDeserialization();
    }

    private void OnMessageReceived(string message)
    {
        Debug.Log($"<color=green>[THH_ChatMessenger]</color>: Received Message.");

        string[] messageContext = message.Split('_');

        VRCPlayerApi sender = VRCPlayerApi.GetPlayerById(int.Parse(messageContext[1], System.Globalization.NumberStyles.HexNumber));
        string messageContent = messageContext[2];

        logger.LogChatMessage(sender, messageContent);

    }

    public void Start()
    {
        isOwner = Networking.IsOwner(gameObject);

        if (Networking.IsMaster)
        {
            MESSAGE = "IGNORE";
            OnDeserialization();
        }
    }

    public override void OnDeserialization()
    {
        if (MESSAGE != last_MESSAGE)
        {
            last_MESSAGE = MESSAGE;

            if (last_MESSAGE == "IGNORE") { return; }

            OnMessageReceived(last_MESSAGE);
        }
    }

    public void ChatRequest(THH_ChatHandler handler)
    {
        if (!transferringOwnership)
        {
            VRCPlayerApi requester = Networking.GetOwner(handler.gameObject);
            transferringOwnership = true;
            Networking.SetOwner(requester, gameObject);
            Debug.Log($"<color=green>[THH_ChatMessenger]</color>: Ownership request of '{requester.displayName}' has been accepted. Transferring ownership.");
        }
        else
        {
            handler.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(THH_ChatHandler.RequestDenied));
        }
    }

    //OnOwnershipTransferred is currently not behaving correctly.This is the code if it were behaving correctly:
    //public override void OnOwnershipTransferred()
    //{
    //    if (isOwner && !Networking.IsOwner(gameObject))
    //    {
    //        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Lost ownership of the chat messenger!");
    //        isOwner = false;

    //        transferringOwnership = false;
    //    }
    //    else if (!isOwner && Networking.IsOwner(gameObject))
    //    {
    //        Debug.Log($"<color=green>[THH_ChatHandler]</color>: Taken ownership of handler '{name}'!");
    //        isOwner = true;

    //        if (manager.requesting) { manager.RequestAccepted(); }
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
            Debug.Log($"<color=green>[THH_ChatHandler]</color>: Lost ownership of the chat messenger!");
            isOwner = false;

            transferringOwnership = false;
        }
        else if (!isOwner && Networking.IsOwner(gameObject))
        {
            Debug.Log($"<color=green>[THH_ChatHandler]</color>: Taken ownership of handler '{name}'!");
            isOwner = true;

            if (manager.requesting) { manager.RequestAccepted(); }
        }
    }
}
