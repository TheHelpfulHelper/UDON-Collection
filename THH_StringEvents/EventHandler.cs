
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventHandler : UdonSharpBehaviour
{
    public string EventOutbox;
    public string EventInbox;

    private string[] EVENT_ARRAY;
    private UdonBehaviour _EventManger;

    void Start()
    {
        _EventManger = (UdonBehaviour)GameObject.Find("[SE_Manager]").GetComponent(typeof(UdonBehaviour));
        UdonBehaviour master = (UdonBehaviour)transform.parent.GetComponent(typeof(UdonBehaviour));
        master.SetProgramVariable("EventHandler", this);
    }

    void Update()
    {
        CheckInbox();
        CheckOutbox();
    }

    void CheckOutbox()
    {
        if (!string.IsNullOrEmpty(EventOutbox))
        {
            _EventManger.SetProgramVariable("EVENT", EventOutbox);
            EventOutbox = string.Empty;
        }
    }

    void CheckInbox()
    {
        if (!string.IsNullOrEmpty(EventInbox))
        {
            ProcessEvent();
        }
    }

    void ProcessEvent()
    {
        EVENT_ARRAY = EventInbox.Split(';');
        CheckTarget();
        EventInbox = string.Empty;
    }

    void CheckTarget()
    {
        string target = (string)EVENT_ARRAY.GetValue(3);
        if (target.Equals("O"))
        {
            if (Networking.LocalPlayer.IsOwner(gameObject.transform.parent.gameObject))
            {
                ProcessPayload();
                return;
            }
            else
            {
                return;
            }
        }
        if (target.Equals("A"))
        {
            ProcessPayload();
            return;
        }
        Debug.Log("[SE_Handler] Error: invalid target; Must be 'O'(wner) or 'A'(ll)");
    }

    void ProcessPayload()
    {
        Component[] receiverComponents = transform.parent.GetComponents(typeof(UdonBehaviour));
        int componentIndex = System.Int32.Parse((string)EVENT_ARRAY.GetValue(2));
        if (componentIndex < receiverComponents.GetLowerBound(0) || componentIndex > receiverComponents.GetUpperBound(0))
        {
            Debug.Log("[SE_Handler] Error: Component index " + componentIndex + " is out of bounds");
            return;
        }
        UdonBehaviour receiverUdon = (UdonBehaviour)receiverComponents.GetValue(componentIndex);
        if (receiverUdon == null)
        {
            Debug.Log("[SE_Handler] Error: Could not access UdonBehaviour at index " + componentIndex);
            return;
        }
        if (EVENT_ARRAY.Length == 6)
        {
            string parametersString = (string)EVENT_ARRAY.GetValue(5);
            string[] parametersArray = parametersString.Split('|');
            receiverUdon.SetProgramVariable("PARAMS", parametersArray);
        }
        string udonEventName = (string)EVENT_ARRAY.GetValue(4);
        receiverUdon.SendCustomEvent(udonEventName);        
    }
}
