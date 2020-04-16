
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DelayServiceManager : UdonSharpBehaviour
{
    // Arbitrary
    public int MAX_TRACKED_EVENTS = 100;

    public float DELAY;
    public UdonBehaviour TARGET;
    public string EVENT;

    private float[] delays;
    private Component[] targets;
    private string[] events;

    private int lastActiveEvent = -1;

    void Start()
    {
        delays = new float[MAX_TRACKED_EVENTS];
        targets = new Component[MAX_TRACKED_EVENTS];
        events = new string[MAX_TRACKED_EVENTS];
    }

    public void CreateDelay()
    {
        int slot_index = -1;
        // Find empty slot
        for (int i = 0; i < MAX_TRACKED_EVENTS; i++)
        {
            if ((float)delays.GetValue(i) == 0f)
            {
                slot_index = i;
                break;
            }
        }

        if (slot_index == -1)
        {
            Debug.Log("[DS_Manager] ERROR: No empty delay slot available! The event '" + EVENT + "' had to be discarded.");
            return;
        }

        // Set the values
        delays.SetValue(DELAY + Time.timeSinceLevelLoad, slot_index);
        targets.SetValue(TARGET, slot_index);
        events.SetValue(EVENT, slot_index);

        if (lastActiveEvent < slot_index)
        {
            lastActiveEvent = slot_index;
        }
    }

    void Update()
    {
        for (int i = 0; i <= lastActiveEvent; i++)
        {
            float d = (float)delays.GetValue(i);
            if (i == lastActiveEvent && d == 0f)
            {
                lastActiveEvent--;
            }
            else if ( d > 0f && Time.timeSinceLevelLoad > d)
            {
                // Send event
                UdonBehaviour targetUdon = (UdonBehaviour)targets.GetValue(i);
                targetUdon.SendCustomEvent((string)events.GetValue(i));

                // Reset slot
                delays.SetValue(null, i);
                targets.SetValue(null, i);
                events.SetValue(null, i);

            }
        }
    }
}
