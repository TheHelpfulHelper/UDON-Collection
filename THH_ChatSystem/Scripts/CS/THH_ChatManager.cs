
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;

public class THH_ChatManager : UdonSharpBehaviour
{
    public THH_ChatLog ChatLog;
    public THH_ChatInputModule CIM;

    [HideInInspector, UdonSynced(UdonSyncMode.None)]
    public string CHAT_MESSAGE = "";
    private string last_ChatMessage = "";
    [HideInInspector]
    public string MESSAGE = "SAMPLETEXT";

    private bool wasOwner;

    private float chatDelay = 0.5f;
    private float messageCooldown = 2f;
    private float retryInterval = 0.5f;
    private int maxRetries = 30;

    private float sendEndTime;
    private float retryEndTime;
    private float cooldownEndTime;

    private bool isSendingMessage;
    private bool isAutoRetrying;
    private bool isSendingBlocked;
    private int messageCount;
    private int retryCount;

    // == API ==================================================================================================//
    public void SendChatMessage(string message)
    {
        message = message.Replace('<', ' ');
        message = message.Replace('>', ' ');
        MESSAGE = message;
        if (ProcessMessage())
        {
            OnMessageSentSuccess();
        }
        else
        {
            OnMessageSentFail();
        }
    }

    void OnMessageSentSuccess()
    {
        cooldownEndTime = Time.timeSinceLevelLoad + messageCooldown;
        CIM.ClearInputField();
    }

    void OnMessageSentFail()
    {

    }

    void OnMessageReceived(string message)
    {
        ChatLog.Log(message);
        Debug.Log($"<color=green>[THH_ChatManager]</color> Received a chat message.");
    }

    //==================================================================================================//

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        ChatLog.Log($"<color=yellow>[System]: Player <color=lime>{player.displayName}</color> has joined.</color>");
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        ChatLog.Log($"<color=yellow>[System]: Player <color=lime>{player.displayName}</color> has left.</color>");
    }

    public void Update()
    {
        if (Networking.IsMaster)
        {

        }
        else
        {
            bool isOwner = Networking.IsOwner(gameObject);
            if (isOwner ^ wasOwner)
            {
                wasOwner = !wasOwner;
                if (isOwner)
                {
                    Debug.Log($"<color=green>[THH_ChatManager]</color> You became owner of the ChatManager.");
                    sendEndTime = Time.timeSinceLevelLoad + chatDelay;
                    isSendingMessage = true;
                }
                else
                {
                    Debug.Log($"<color=green>[THH_ChatManager]</color> You lost ownership of the ChatManager.");
                    isSendingBlocked = false;
                }
            }
        }

        if (isSendingMessage)
        {
            if (Time.timeSinceLevelLoad > sendEndTime)
            {
                int messageID = messageCount + 1;
                CHAT_MESSAGE = $"{Networking.LocalPlayer.playerId}|{MESSAGE}|{messageID.ToString("X")}";
                isSendingMessage = false;
            }
        }

        if (isAutoRetrying)
        {
            if (Time.timeSinceLevelLoad > retryEndTime)
            {
                if (ProcessMessage())
                {
                    isAutoRetrying = false;
                    CIM.SetInputFieldLocked(false);
                    CIM.ClearInputField();
                }
                else
                {
                    retryEndTime = Time.timeSinceLevelLoad + retryInterval;
                }
            }
        }

        if (CHAT_MESSAGE != last_ChatMessage && !string.IsNullOrEmpty(CHAT_MESSAGE))
        {
            string[] M = CHAT_MESSAGE.Split('|');
            int messageID = Convert.ToInt32(M[2], 16);
            if (messageID > messageCount)
            {
                messageCount = messageID;
                string messageSender = VRCPlayerApi.GetPlayerById(Int32.Parse(M[0])).displayName;
                string messageContent = M[1];
                string message = $"<<color=lime>{messageSender}</color>>: {messageContent}";
                OnMessageReceived(message);

                if (Networking.IsMaster && !Networking.GetOwner(gameObject).isLocal)
                {
                    // Reclaim Ownership
                    Debug.Log($"<color=green>[THH_ChatManager]</color> Reclaiming ownership of the ChatManager as Master.");
                    Networking.SetOwner(Networking.LocalPlayer, gameObject);
                }
            }
            last_ChatMessage = CHAT_MESSAGE;
        }
    }

    bool ProcessMessage()
    {
        if (isSendingBlocked || !(Time.timeSinceLevelLoad > cooldownEndTime))
        {
            ChatLog.Log($"<i><color=red>You are sending messages too quickly</color></i>");
            return false;
        }
        if (Networking.GetOwner(gameObject).isMaster)
        {
            if (Networking.IsMaster)
            {
                int messageID = messageCount + 1;
                CHAT_MESSAGE = $"{Networking.LocalPlayer.playerId}|{MESSAGE}|{messageID.ToString("X")}";
                return true;
            }
            else
            {
                isSendingBlocked = true;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                return true;
            }
        }
        else
        {
            if (isAutoRetrying)
            {
                retryCount++;
                if (retryCount == maxRetries)
                {
                    Debug.LogError($"<color=green>[THH_ChatManager]</color> Message could not be delivered after {retryCount} retries, aborting...");
                    ChatLog.Log($"<i><color=red>Your message could not be delivered</color></i>");
                    isAutoRetrying = false;
                    retryCount = 0;
                    CIM.SetInputFieldLocked(false);
                }
                return false;
            }
            Debug.Log($"<color=green>[THH_ChatManager]</color> The chat manager is currently occupied by someone else, retrying...");
            isAutoRetrying = true;
            retryEndTime = Time.timeSinceLevelLoad + retryInterval;
            CIM.SetInputFieldLocked(true);
            return false;
        }
    }
}
