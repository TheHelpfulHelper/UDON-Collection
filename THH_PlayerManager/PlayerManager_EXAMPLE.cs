
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerManager_EXAMPLE : UdonSharpBehaviour
{
    public UdonBehaviour PlayerManager;
    void Interact()
    {
        VRCPlayerApi[] players = (VRCPlayerApi[])PlayerManager.GetProgramVariable("PLAYERAPIS");

        for (int i = 0; i < (int)PlayerManager.GetProgramVariable("NUMBER_OF_PLAYERS"); i++)
        {
            VRCPlayerApi player = (VRCPlayerApi)players.GetValue(i);
            Debug.Log(player.displayName);
        }
    }
}
