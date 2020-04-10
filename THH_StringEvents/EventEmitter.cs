
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventEmitter : UdonSharpBehaviour
{
    [UdonSynced]
    public string EVENT;
    public string[] EVENT_ARRAY;

    [UdonSynced]
    public string Owner = "UNOWNED";

    [UdonSynced]
    public bool hasBeenClaimed = false;

    private string last_event = "null";

    private VRCPlayerApi LocalPlayerApi;
    private UdonBehaviour _EventManger;
    
    void Start()
    {
        LocalPlayerApi = Networking.LocalPlayer;
        _EventManger = (UdonBehaviour)GameObject.Find("[EventManager]").GetComponent(typeof(UdonBehaviour));
    }

    void Update()
    {
        if (LocalPlayerApi != null)
        {
            if (Owner.Equals(LocalPlayerApi.displayName))
            {
                Networking.SetOwner(LocalPlayerApi, gameObject);
                _EventManger.SetProgramVariable("LocallyOwnedEmitter", this);
                SetProgramVariable("hasBeenClaimed", true);
            }
            if (LocalPlayerApi.IsOwner(gameObject) && LocalPlayerApi.isMaster && !Owner.Equals(LocalPlayerApi.displayName) && hasBeenClaimed)
            {
                SetProgramVariable("Owner", "UNOWNED");
                hasBeenClaimed = false;
            }
        }
        if (!string.IsNullOrEmpty(EVENT) && last_event != EVENT)
        {
            ProcessEvent();
        }
        last_event = EVENT;
    }

    void ProcessEvent()
    {
        //EVENT = EVENT_ID.ToString() + ";" + EVENT;
        EVENT_ARRAY = EVENT.Split(';');
        if (EVENT_ARRAY.Length < 5 || EVENT_ARRAY.Length > 6)
        {
            Debug.Log("[EventEmitter] Error: invalid event format");
            return;
        }
        ProcessReceiver();
    }

    void ProcessReceiver()
    {
        string receiverName = (string)EVENT_ARRAY.GetValue(1);
        GameObject receiverObject = GameObject.Find(receiverName);
        if (receiverObject == null)
        {
            Debug.Log("[EventEmitter] Error: No Receiver found called " + receiverName);
            return;
        }
        UdonBehaviour eventReceiverHandler = (UdonBehaviour)receiverObject.transform.Find("[EventHandler]").GetComponent(typeof(UdonBehaviour));
        if (eventReceiverHandler == null)
        {
            Debug.Log("[EventEmitter] Error: No EventHandler found on target " + receiverObject.name);
            return;
        }
        eventReceiverHandler.SetProgramVariable("EventInbox", EVENT);
    }
}
