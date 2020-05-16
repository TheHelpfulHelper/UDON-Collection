
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventToPlayerTest : UdonSharpBehaviour
{
    public THH_PlayerManager playerManager;
    public void Interact()
    {
        playerManager.customEventPlayerTarget = Networking.LocalPlayer;
        playerManager.customEventUdonTarget = (UdonBehaviour)gameObject.GetComponent(typeof(UdonBehaviour));
        playerManager.customEventName = "Test";

        playerManager.SendCustomNetworkEventToPlayer();
    }

    public void Test()
    {
        Debug.Log("============= EVENT TO PLAYER TEST RECEIVED ================");
    }
}
