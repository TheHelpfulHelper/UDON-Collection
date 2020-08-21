
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using UnityEngine.UI;

public class THH_ChatLog : UdonSharpBehaviour
{
    public Transform Content;
    public GameObject ChatMessagePrefab;

    public InputField inputField;

    public void LogMessage(VRCPlayerApi player, string message)
    {
        GameObject newChatMessageObject = VRCInstantiate(ChatMessagePrefab);

        newChatMessageObject.transform.SetParent(Content, false);
        newChatMessageObject.transform.GetChild(0).GetComponent<Text>().text = $"<color=green>{player.displayName}</color>: {message}";
    }

    public void Lock()
    {
        inputField.enabled = false;
    }

    public void Unlock()
    {
        inputField.enabled = true;
        inputField.text = "";
    }
}
