
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PpOP_Manager : UdonSharpBehaviour
{
    public GameObject ownedHandler;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (Networking.IsMaster)
        {
            if (player.isMaster)
            {
                GameObject handler = transform.GetChild(0).gameObject;

                Debug.Log("[<color=navy>THH_PpOP : Manager</color>] INFO: I am the first master, setting handler as " + handler.name);
                ownedHandler = handler;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject handler = transform.GetChild(i).gameObject;
                if (Networking.GetOwner(handler).isMaster && handler != ownedHandler)
                {
                    UdonBehaviour handlerUdon = (UdonBehaviour)handler.GetComponent(typeof(UdonBehaviour));

                    if (handlerUdon == null)
                    {
                        Debug.Log("[<color=navy>THH_PpOP : Manager</color>] <color=red>ERROR: " + handler.name + " has no UdonBehaviour!");
                        continue;
                    }

                    handlerUdon.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "BecomeOwner");
                    break;
                }
            }
        }
    } 
}
