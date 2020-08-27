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
    [System.NonSerialized] public string last_MESSAGE = "UNINITIALIZED";
    private bool syncStringInitialized;

    private int MessageCounter;

    public void Chat()
    {
        if (manager.attemptingToTransmitMessage || string.IsNullOrEmpty(logger.inputField.text)) { return; }

        manager.AttemptToSendChatMessage(logger.inputField.text, true, 0);
    }

    public void SendChatMessage(string message)
    {
        if (!Networking.IsOwner(gameObject)) { return; }

        string MID = ((MessageCounter + 1) % 1000000).ToString().PadLeft(6, '0');
        string PID = Networking.LocalPlayer.playerId.ToString().PadLeft(4, '0');

        MESSAGE = $"{MID}{PID}{message}";

        OnDeserialization();
    }

    private void OnMessageReceived(string message)
    {
        Debug.Log($"<color=green>[THH_ChatMessenger]</color>: Received Message.");

        string PID = message.Substring(6, 4);
        string messageContent = message.Substring(10);

        VRCPlayerApi sender = VRCPlayerApi.GetPlayerById(int.Parse(PID));

        logger.LogChatMessage(sender, messageContent);

    }

    public void Start()
    {
        isOwner = Networking.IsOwner(gameObject);

        if (Networking.IsMaster)
        {
            MESSAGE = "INITIALIZED";
            OnDeserialization();
        }
    }

    public override void OnDeserialization()
    {
        if (MESSAGE != last_MESSAGE)
        {
            if (!syncStringInitialized)
            {
                last_MESSAGE = MESSAGE;
                syncStringInitialized = true;
                return;
            }

            string MID = MESSAGE.Substring(0, 6);

            int newMessageCounter = int.Parse(MID);

            int historyValue = newMessageCounter - MessageCounter;
            bool pass = historyValue > 0 ? historyValue < 2 : historyValue < -2;

            if (!pass) { return; }

            MessageCounter = newMessageCounter;

            last_MESSAGE = MESSAGE;

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
