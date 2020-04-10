
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerManager : UdonSharpBehaviour
{
    public int MaxTrackedPlayers = 80;
    public int NUMBER_OF_PLAYERS;
    public VRCPlayerApi[] PLAYERAPIS;

    void Start()
    {
        PLAYERAPIS = new VRCPlayerApi[MaxTrackedPlayers];
    }

    void OnPlayerJoined(VRCPlayerApi joinedPlayerApi)
    {
        PLAYERAPIS.SetValue(joinedPlayerApi, NUMBER_OF_PLAYERS);
        NUMBER_OF_PLAYERS++;
    }

    void OnPlayerLeft(VRCPlayerApi leftPlayerApi)
    {
        bool leftPlayerFound = false;
        for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
        {
            if (!leftPlayerFound)
            {
                VRCPlayerApi inspectedPlayerApi = (VRCPlayerApi)PLAYERAPIS.GetValue(i);
                if (inspectedPlayerApi == leftPlayerApi)
                {
                    leftPlayerFound = true;
                    continue;
                }
            }
            else
            {
                PLAYERAPIS.SetValue(PLAYERAPIS.GetValue(i), i - 1);
            }
        }
        PLAYERAPIS.SetValue(null, MaxTrackedPlayers - 1);
        NUMBER_OF_PLAYERS--;
    }

    void Log()
    {
        Debug.Log("=================================================");
        Debug.Log("Number of Players:" + NUMBER_OF_PLAYERS);
        for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
        {
            VRCPlayerApi player = (VRCPlayerApi)PLAYERAPIS.GetValue(i);
            Debug.Log(player.displayName);
        }
        Debug.Log("=================================================");
    }
}