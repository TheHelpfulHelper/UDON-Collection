
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class THH_ChatManager : UdonSharpBehaviour
{
    [HideInInspector, UdonSynced(UdonSyncMode.None)]
    public string CHAT_MESSAGE = "";
    private string last_ChatMessage = "";

    [HideInInspector]
    public string MESSAGE = "SAMPLETEXT";
    
    private bool wasOwner;
    private bool reclaiming;
    private float chatDelay = 1f;
    private float sendStart;
    private float sendEnd;
    private bool messageSending;
    private bool locked;

    public THH_CanvasLogManager canvasLogManager;
    public InputField inputField;

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
                sendStart = Time.timeSinceLevelLoad;
                sendEnd = sendStart + chatDelay;
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
                CHAT_MESSAGE = $"<<color=lime>{Networking.LocalPlayer.displayName}</color>>: {MESSAGE}|{Networking.GetServerTimeInMilliseconds()}";
                messageSending = false;
            }
        }

        if (CHAT_MESSAGE != last_ChatMessage && !string.IsNullOrEmpty(CHAT_MESSAGE))
        {
            string[] M = CHAT_MESSAGE.Split('|');
            string message = M[0];
            MessageReceived(message);

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

    // Set MESSAGE first before calling this method!
    public bool SendChatMessage()
    {
        if (locked)
        {
            canvasLogManager.Log($"<color=red>You are sending messages too quickly</color>");
            return false;
        }
        if (Networking.GetOwner(gameObject).isMaster)
        {
            if (Networking.IsMaster)
            {
                CHAT_MESSAGE = $"<<color=lime>{Networking.LocalPlayer.displayName}</color>>: {MESSAGE}|{Networking.GetServerTimeInMilliseconds()}";
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
            Debug.Log($"<color=green>[THH_ChatManager]</color> ChatManager is currently occupied by someone else, retry later");
            canvasLogManager.Log($"<color=red>Your message could not be delivered, retry in a bit</color>");
            return false;
        }
    }

    public void InputFieldEndEdit()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            return;
        }
        MESSAGE = inputField.text;
        if (SendChatMessage())
        {
            inputField.text = string.Empty;
        }
    }

    void MessageReceived(string message)
    {
        Debug.Log($"<color=green>[THH_ChatManager]</color> Chat message received: '{message}'");
        canvasLogManager.Log(message);
    }
}
