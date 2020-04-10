
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventManager : UdonSharpBehaviour
{
    public UdonBehaviour LocallyOwnedEmitter;
    public string EVENT;

    private VRCPlayerApi LocalPlayerApi;

    void Start()
    {
        LocalPlayerApi = Networking.LocalPlayer;

        if (LocalPlayerApi != null)
        {
        }
        else 
        {
            UdonBehaviour emitter = (UdonBehaviour)gameObject.transform.GetChild(0).GetComponent(typeof(UdonBehaviour));
            emitter.SetProgramVariable("Owner", "UNITY_EDITOR");
            LocallyOwnedEmitter = emitter;
        }
    }

    void OnPlayerJoined(VRCPlayerApi joinedPlayer)
    {
        if (LocalPlayerApi.isMaster)
        {
            int emitterCount = transform.childCount;
            for (int i = 0; i < emitterCount; i++)
            {
                UdonBehaviour emitter = (UdonBehaviour)gameObject.transform.GetChild(i).GetComponent(typeof(UdonBehaviour));
                Debug.Log(emitter.name);
                if (emitter.GetProgramVariable("Owner").Equals("UNOWNED"))
                {
                    emitter.SetProgramVariable("Owner", joinedPlayer.displayName);
                    return;
                }
            }
        }
    }

    void OnPlayerLeft(VRCPlayerApi leftPlayer)
    {
        if (LocalPlayerApi.isMaster)
        {
            int emitterCount = transform.childCount;
            for (int i = 0; i < emitterCount; i++)
            {
                UdonBehaviour emitter = (UdonBehaviour)transform.GetChild(i).GetComponent(typeof(UdonBehaviour));
                if (emitter.GetProgramVariable("Owner").Equals(leftPlayer.displayName))
                {
                    emitter.SetProgramVariable("Owner", "UNOWNED");
                    return;
                }
            }
        }
    }

    void Update()
    {
        CheckEvent();
    }

    void CheckEvent()
    {
        if (!string.IsNullOrEmpty(EVENT))
        {
            if (LocallyOwnedEmitter == null)
            {
                Debug.Log("[EventManager]: Client owns no emitter");
                return;
            }
            string eventString = Networking.GetServerTimeInMilliseconds().ToString() + ";" + EVENT;
            LocallyOwnedEmitter.SetProgramVariable("EVENT", eventString);
            EVENT = string.Empty;
        }
    }
}
