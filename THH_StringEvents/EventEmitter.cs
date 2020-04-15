
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventEmitter : UdonSharpBehaviour
{
    [UdonSynced]
    public string EVENT = "";
    public string[] EVENT_ARRAY;

    private string last_event = "";

    void Update()
    {
        if (!string.IsNullOrEmpty(EVENT) && last_event != EVENT)
        {
            ProcessEvent();
        }
        last_event = EVENT;
    }

    void ProcessEvent()
    {
        EVENT_ARRAY = EVENT.Split(';');
        if (EVENT_ARRAY.Length < 5 || EVENT_ARRAY.Length > 6)
        {
            Debug.Log("[SE_Emitter] Error: invalid event format");
            return;
        }
        ProcessReceiver();
    }

    void ProcessReceiver()
    {
        string receiverName = (string)EVENT_ARRAY.GetValue(1);
        GameObject receiverObject = GameObject.Find(receiverName);
        UdonBehaviour receiverUdon = (UdonBehaviour)receiverObject.GetComponent(typeof(UdonBehaviour));
        if (receiverObject == null)
        {
            Debug.Log("[SE_Emitter] Error: No Receiver found called " + receiverName);
            return;
        }
        UdonBehaviour eventReceiverHandler = (UdonBehaviour)receiverUdon.GetProgramVariable("EventHandler");
        if (eventReceiverHandler == null)
        {
            Debug.Log("[SE_Emitter] Error: No EventHandler found on target " + receiverObject.name);
            return;
        }
        eventReceiverHandler.SetProgramVariable("EventInbox", EVENT);
    }
}
