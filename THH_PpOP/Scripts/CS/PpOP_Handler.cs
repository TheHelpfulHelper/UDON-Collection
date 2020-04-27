
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PpOP_Handler : UdonSharpBehaviour
{
    private UdonBehaviour manager;

    public void Start()
    {
        manager = (UdonBehaviour)transform.parent.GetComponent(typeof(UdonBehaviour));
    }

    public void BecomeOwner()
    {
        if (manager.GetProgramVariable("ownedHandler") == null)
        {
            Debug.Log("[<color=navy>THH_PpOP : Handler</color>] INFO: Setting handler as " + gameObject.name);

            manager.SetProgramVariable("ownedHandler", gameObject);

            Component[] playerObjects = transform.GetComponentsInChildren(typeof(Transform), false);
            foreach (Component playerObject in playerObjects)
            {
                Transform objectTransform = (Transform)playerObject;
                Debug.Log(objectTransform.name);
                Networking.SetOwner(Networking.LocalPlayer, objectTransform.gameObject);
            }
        }
    }
}
