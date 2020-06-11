
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DelayServiceExample : UdonSharpBehaviour
{
    // This field is required for the DelaySystem
    private DelayServiceHandler DS_Handler;

    void Interact()
    {
        // Example of creating a DelayedEvent
        DS_Handler.CreateDelay(10f, this, "DelayedEvent", "Local");
    }

    public void DelayedEvent()
    {
        Debug.Log("Hello World!");
    }
}
