
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class THH_PT_Sphere : UdonSharpBehaviour
{
    public UdonBehaviour MessageReceiver;
    public bool IncludeRemotePlayers;
    public bool SendStayEvents;

    private SphereCollider sphereCollider;
    private Vector3 position;
    private float radius;
    private LayerMask layerMask = 0b0000_0000_0000_0100_0000_0000;
    private bool enter;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            Debug.LogError("<color=green>[THH_PlayerTriggers]</color>: No SphereCollider was found on " + gameObject.name);
            return;
        }
        if (!sphereCollider.isTrigger)
        {
            Debug.LogWarning("<color=green>[THH_PlayerTriggers]</color>: SphereCollider was not set to isTrigger on " + gameObject.name);
        }

        position = sphereCollider.bounds.center;
        radius = sphereCollider.radius;

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
        if (Physics.OverlapSphere(position, radius, layerMask, QueryTriggerInteraction.Collide).Length > 0)
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
