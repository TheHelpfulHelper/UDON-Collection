
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;

public class THH_ChatLog : UdonSharpBehaviour
{
    public Transform Content;
    public GameObject ChatMessagePrefab;

    public InputField inputField;

    public void LogChatMessage(VRCPlayerApi player, string message)
    {
        GameObject newChatMessageObject = VRCInstantiate(ChatMessagePrefab);

        newChatMessageObject.transform.SetParent(Content, false);
        newChatMessageObject.transform.GetChild(0).GetComponent<Text>().text = $"<color=green><{player.displayName}></color>: {message}";
    }

    public void LogMessage(string message)
    {
        GameObject newChatMessageObject = VRCInstantiate(ChatMessagePrefab);

        newChatMessageObject.transform.SetParent(Content, false);
        newChatMessageObject.transform.GetChild(0).GetComponent<Text>().text = message;
    }

    public void Lock()
    {
        inputField.interactable = false;
    }

    public void Unlock()
    {
        inputField.interactable = true;
        inputField.text = "";
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        LogMessage($"<color=yellow>{player.displayName} has joined.</color>");
    }

    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        LogMessage($"<color=yellow>{player.displayName} has left.</color>");
    }
}
