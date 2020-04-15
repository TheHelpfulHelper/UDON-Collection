
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventManager : UdonSharpBehaviour
{
    public UdonBehaviour LocallyOwnedEmitter;
    public GameObject Emitters;

    public string EVENT;

    private VRCPlayerApi LocalPlayerApi;

    void Start()
    {
        LocalPlayerApi = Networking.LocalPlayer;
        GetEmitter();
    }

    void OnPlayerLeft(VRCPlayerApi leftPlayer)
    {
        if (LocalPlayerApi.isMaster)
        {
            Debug.Log("Im Master Now!");
            UdonBehaviour emitterUdon = (UdonBehaviour)Emitters.transform.GetChild(0).GetComponent(typeof(UdonBehaviour));
            LocallyOwnedEmitter = emitterUdon;
        }
    }

    void GetEmitter()
    {
        if (LocalPlayerApi != null)
        {
            if (LocalPlayerApi.isMaster)
            {
                UdonBehaviour emitterUdon = (UdonBehaviour)Emitters.transform.GetChild(0).GetComponent(typeof(UdonBehaviour));
                LocallyOwnedEmitter = emitterUdon;
                return;
            }
            else
            {
                int emitterCount = Emitters.transform.childCount;
                for (int i = 1; i < emitterCount; i++)
                {
                    UdonBehaviour emitterUdon = (UdonBehaviour)Emitters.transform.GetChild(i).GetComponent(typeof(UdonBehaviour));
                    if (Networking.GetOwner(emitterUdon.gameObject).isMaster)
                    {
                        Networking.SetOwner(LocalPlayerApi, emitterUdon.gameObject);
                        LocallyOwnedEmitter = emitterUdon;
                        return;
                    }
                }
            }
        }
        else
        {
            UdonBehaviour emitter = (UdonBehaviour)Emitters.transform.GetChild(0).GetComponent(typeof(UdonBehaviour));
            emitter.SetProgramVariable("Owner", "UNITY_EDITOR");
            LocallyOwnedEmitter = emitter;
            return;
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
                Debug.Log("[SE_Manager] Error: Client owns no emitter");
                return;
            }
            string eventString = Networking.GetServerTimeInMilliseconds().ToString() + ";" + EVENT;
            LocallyOwnedEmitter.SetProgramVariable("EVENT", eventString);
            EVENT = string.Empty;
        }
    }
}
