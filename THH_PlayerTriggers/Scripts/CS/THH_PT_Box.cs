
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class THH_PT_Box : UdonSharpBehaviour
{
    public UdonBehaviour MessageReceiver;
    public bool IncludeRemotePlayers;
    public bool SendStayEvents;

    private BoxCollider boxCollider;
    private Vector3 center;
    private Vector3 extents;
    private Quaternion orientation;
    private LayerMask layerMask = 0b0000_0000_0000_0100_0000_0000;
    private bool enter;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogError("<color=green>[THH_PlayerTriggers]</color>: No BoxCollider was found on " + gameObject.name);
            return;
        }
        if (!boxCollider.isTrigger)
        {
            Debug.LogWarning("<color=green>[THH_PlayerTriggers]</color>: BoxCollider was not set to isTrigger on " + gameObject.name);
        }
        Bounds boxBounds = boxCollider.bounds;
        center = boxBounds.center;
        extents = boxBounds.extents;
        orientation = boxCollider.transform.rotation;
        if (IncludeRemotePlayers)
        {
            layerMask = 0b0000_0000_0000_0110_0000_0000;
        }
        if (MessageReceiver == null)
        {
            Debug.LogWarning("<color=green>[THH_PlayerTriggers]</color>: No message receiver set on " + gameObject.name);
        }
    }

    public void FixedUpdate()
    {
        if (Physics.OverlapBox(center, extents, orientation, layerMask, QueryTriggerInteraction.Collide).Length > 0)
        {
            if (!enter)
            {
                enter = true;
                OnPlayerEnter();
            }
            else if (SendStayEvents)
            {
                OnPlayerStay();
            }
        }
        else if (enter)
        {
            enter = false;
            OnPlayerExit();
        }
    }

    private void OnPlayerEnter()
    {
        //Debug.Log("<color=green>[THH_PlayerTriggers]</color>: Event 'OnPlayerEnter' on " + gameObject.name);
        if (MessageReceiver != null)
        {
            MessageReceiver.SendCustomEvent("OnPlayerEnter");
        }
    }

    private void OnPlayerStay()
    {
        //Debug.Log("<color=green>[THH_PlayerTriggers]</color>: Event 'OnPlayerStay' on " + gameObject.name);
        if (MessageReceiver != null)
        {
            MessageReceiver.SendCustomEvent("OnPlayerStay");
        }
    }

    private void OnPlayerExit()
    {
        //Debug.Log("<color=green>[THH_PlayerTriggers]</color>: Event 'OnPlayerExit' on " + gameObject.name);
        if (MessageReceiver != null)
        {
            MessageReceiver.SendCustomEvent("OnPlayerExit");
        }
    }
}
