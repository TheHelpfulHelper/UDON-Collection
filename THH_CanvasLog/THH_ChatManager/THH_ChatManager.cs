
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using System;

public class THH_ChatManager : UdonSharpBehaviour
{
    public bool TEST;

    [HideInInspector, UdonSynced(UdonSyncMode.None)]
    public string CHAT_MESSAGE = "";
    private string last_ChatMessage = "";

    [HideInInspector]
    public string MESSAGE = "SAMPLETEXT";
    
    private bool wasOwner;
    private bool reclaiming;
    private float chatDelay = 0.2f;
    private float sendEnd;
    private bool messageSending;
    private bool locked;
    private int messageCounter;
    private bool autoRetry;
    private float retryInterval = 0.5f;
    private float retryTime;
    private int retryCount;
    private int maxRetries = 30;

    public THH_CanvasLogManager canvasLogManager;
    public InputField inputField;

    public void InputFieldEndEdit()
    {
        string inputString = inputField.text.Trim();
        if (string.IsNullOrEmpty(inputString))
        {
            return;
        }
        SendChatMessage(inputString);
        inputField.ActivateInputField();
    }

    void ClearInputField()
    {
        inputField.text = string.Empty;
    }

    void SetInputFieldLocked(bool b)
    {
        inputField.interactable = !b;
    }
    // == API ==================================================================================================//
    public void SendChatMessage(string message)
    {
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
        ClearInputField();
    }

    void OnMessageSentFail()
    {

    }

    void OnMessageReceived(string message)
    {
        canvasLogManager.Log(message);
    }

    //==================================================================================================//

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        canvasLogManager.Log($"<color=yellow>[System]: Player <color=lime>{player.displayName}</color> has joined.</color>");
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        canvasLogManager.Log($"<color=yellow>[System]: Player <color=lime>{player.displayName}</color> has left.</color>");
    }

    public void Start()
    {
        if (Networking.IsMaster)
        {
            wasOwner = true;
        }
    }

    public void Update()
    {
        bool isOwner = Networking.IsOwner(gameObject);
        if (isOwner ^ wasOwner) 
        { 
            wasOwner = !wasOwner; 
            if (isOwner)
            {
                Debug.Log($"<color=green>[THH_ChatManager]</color> You became owner of the ChatManager.");
                if (reclaiming)
                { 
                    reclaiming = false;
                    return;
                }
                sendEnd = Time.timeSinceLevelLoad + chatDelay;
                messageSending = true;
            }
            else
            {
                Debug.Log($"<color=green>[THH_ChatManager]</color> You lost ownership of the ChatManager.");
                locked = false;
            }
        }

        if (messageSending)
        {
            if (Time.timeSinceLevelLoad > sendEnd)
            {
                CHAT_MESSAGE = $"{Networking.LocalPlayer.playerId}|{MESSAGE}|{messageCounter.ToString("X")}";
                messageCounter++;
                messageSending = false;
            }
        }

        if (autoRetry)
        {
            if (Time.timeSinceLevelLoad > retryTime)
            {
                if (ProcessMessage())
                {
                    autoRetry = false;
                    SetInputFieldLocked(false);
                    ClearInputField();
                }
                else
                {
                    retryTime = Time.timeSinceLevelLoad + retryInterval;
                }
            }
        }

        if (CHAT_MESSAGE != last_ChatMessage && !string.IsNullOrEmpty(CHAT_MESSAGE))
        {
            string[] M = CHAT_MESSAGE.Split('|');
            string messageSender = VRCPlayerApi.GetPlayerById(Int32.Parse(M[0])).displayName;
            string messageContent = M[1];
            string message = $"<<color=lime>{messageSender}</color>>: {messageContent}";
            OnMessageReceived(message);

            // Reclaim ownership as master after message has been received
            if (Networking.IsMaster && !Networking.GetOwner(gameObject).isLocal)
            {
                Debug.Log($"<color=green>[THH_ChatManager]</color> Reclaiming ownership as master");
                reclaiming = true;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
            }
            last_ChatMessage = CHAT_MESSAGE;
        }
    }

    bool ProcessMessage()
    {
        if (locked)
        {
            canvasLogManager.Log($"<i><color=red>You are sending messages too quickly</color></i>");
            return false;
        }
        if (Networking.GetOwner(gameObject).isMaster && !TEST)
        {
            if (Networking.IsMaster)
            {
                CHAT_MESSAGE = $"{Networking.LocalPlayer.playerId}|{MESSAGE}|{messageCounter.ToString("X")}";
                messageCounter++;
                return true;
            }
            else
            {
                locked = true;
                Networking.SetOwner(Networking.LocalPlayer, gameObject);
                return true;
            }
        }
        else
        {
            if (autoRetry)
            {
                retryCount++;
                if (retryCount == maxRetries)
                {
                    Debug.LogError($"<color=green>[THH_ChatManager]</color> Message could not be delivered after {retryCount} retries, aborting...");
                    canvasLogManager.Log($"<i><color=red>Your message could not be delivered</color></i>");
                    autoRetry = false;
                    retryCount = 0;
                    SetInputFieldLocked(false);
                }
                return false;
            }
            Debug.Log($"<color=green>[THH_ChatManager]</color> The chat manager is currently occupied by someone else, retrying...");
            autoRetry = true;
            retryTime = Time.timeSinceLevelLoad + retryInterval;
            SetInputFieldLocked(true);
            return false;
        }
    }
}
