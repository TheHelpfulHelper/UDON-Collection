
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DelayServiceManager : UdonSharpBehaviour
{
    // Arbitrary
    public int MAX_TRACKED_EVENTS = 100;

    private float[] delays;
    private Component[] targets;
    private string[] events;
    private string[] networkModes;

    private int lastActiveEvent = -1;

    void Start()
    {
        delays = new float[MAX_TRACKED_EVENTS];
        targets = new Component[MAX_TRACKED_EVENTS];
        events = new string[MAX_TRACKED_EVENTS];
        networkModes = new string[MAX_TRACKED_EVENTS];
    }

    public void CreateDelay(float delay, Component target, string eventName, string networkMode)
    {
        int slot_index = -1;
        // Find empty slot
        for (int i = 0; i < MAX_TRACKED_EVENTS; i++)
        {
            if (delays[i] == 0f)
            {
                slot_index = i;
                break;
            }
        }

        if (slot_index == -1)
        {
            Debug.LogWarning("[DS_Manager]: No empty delay slot available! The event '" + eventName + "' had to be discarded.");
            return;
        }

        // Set the values
        delays[slot_index] = delay + Time.realtimeSinceStartup;
        targets[slot_index] = target;
        events[slot_index] = eventName;
        networkModes[slot_index] = networkMode;

        if (lastActiveEvent < slot_index)
        {
            lastActiveEvent = slot_index;
        }
    }

    void Update()
    {
        for (int i = 0; i <= lastActiveEvent; i++)
        {
            float d = delays[i];

            if (i == lastActiveEvent && d == 0f)
            {
                lastActiveEvent--;
            }

            else if ( d > 0f && Time.realtimeSinceStartup > d)
            {
                // Send event
                UdonBehaviour targetUdon = (UdonBehaviour)targets[i];
                string eventName = events[i];
                string networkMode = networkModes[i];
                switch (networkMode)
                {
                    case "Local":
                        targetUdon.SendCustomEvent(eventName);
                        break;
                    case "All":
                        targetUdon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, eventName);
                        break;
                    case "Owner":
                        targetUdon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, eventName);
                        break;
                }

                // Reset slot
                delays[i] = 0f;
                targets[i] = null;
                events[i] = null;
                networkModes[i] = null;
            }
        }
    }
}
