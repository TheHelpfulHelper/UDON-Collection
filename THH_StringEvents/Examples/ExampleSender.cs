
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ExampleSender : UdonSharpBehaviour
{
    //=== These members are required for the StringEventSystem ===========//
    public UdonBehaviour EventHandler;
    public string[] PARAMS;
    //====================================================================//

    public string EventToSend;

    void Interact()
    {
        // Example way to send a StringEvent
        EventHandler.SetProgramVariable("EventOutbox", EventToSend);
    }
}
