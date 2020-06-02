
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class THH_ChatInputModule : UdonSharpBehaviour
{
    public InputField inputField;
    public THH_ChatManager chatManager;

    private bool immobilized;
    private float jumpImpulse;

    void Update()
    {
        if (!Networking.LocalPlayer.IsUserInVR())
        {
            if (!immobilized && inputField.isFocused)
            {
                Networking.LocalPlayer.Immobilize(true);
                jumpImpulse = Networking.LocalPlayer.GetJumpImpulse();
                Networking.LocalPlayer.SetJumpImpulse(0);
                immobilized = true;
            }
            else if (immobilized && (Input.GetKeyDown(KeyCode.Escape) || !inputField.isFocused))
            {
                Networking.LocalPlayer.Immobilize(false);
                Networking.LocalPlayer.SetJumpImpulse(jumpImpulse);
                immobilized = false;
            }
        }
    }

    public void InputFieldEndEdit()
    {
        string inputString = inputField.text.Trim();
        if (string.IsNullOrEmpty(inputString))
        {
            return;
        }
        chatManager.SendChatMessage(inputString);
        //inputField.ActivateInputField();
    }

    public void ClearInputField()
    {
        inputField.text = string.Empty;
    }

    public void SetInputFieldLocked(bool b)
    {
        inputField.interactable = !b;
    }
}
