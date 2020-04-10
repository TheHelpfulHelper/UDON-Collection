
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ExampleSender : UdonSharpBehaviour
{
    // These members are required for the StringEventSystem
    private UdonBehaviour EventHandler;
    public string[] PARAMS;
    void SendStringEvent(string eventString)
    {
        EventHandler.SetProgramVariable("EventOutbox", eventString);
    }

    //====================================================================//

    public string EventToSend;

    void Interact()
    {
        // Example way to send a StringEvent
        SendStringEvent(EventToSend);
    }
}
