
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DelayServiceExample : UdonSharpBehaviour
{
    // This field is required for the DelaySystem
    public UdonBehaviour DS_Handler;

    void Interact()
    {
        // Example of creating a DelayedEvent
        DS_Handler.SetProgramVariable("DELAY", 10f);
        DS_Handler.SetProgramVariable("TARGET", this);
        DS_Handler.SetProgramVariable("EVENT", "DelayedEvent");
        DS_Handler.SendCustomEvent("CreateDelay");
    }

    public void DelayedEvent()
    {
        Debug.Log("Hello World!");
    }
}
