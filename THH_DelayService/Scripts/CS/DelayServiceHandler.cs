
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DelayServiceHandler : UdonSharpBehaviour
{
    [SerializeField]
    private DelayServiceManager DS_Manager;

    private float DELAY;
    private UdonBehaviour TARGET;
    private string EVENT;
    private string NETWORKMODE;

    void Start()
    {
        if (DS_Manager == null)
        {
            DS_Manager = GameObject.Find("[DS_Manager]").GetComponent<DelayServiceManager>();
        }

        if (DS_Manager == null)
        {
            Debug.LogError("[DS_Handler]: Could not find DelayService Manager!");
            return;
        }

        UdonBehaviour parentUdon = (UdonBehaviour)transform.parent.gameObject.GetComponent(typeof(UdonBehaviour));
        if (parentUdon == null)
        {
            Debug.LogError("[DS_Handler]: Parent has no UdonBehaviour");
            return;
        }

        parentUdon.SetProgramVariable("DS_Handler", this);
    }

    public void CreateDelay(float delay, Component target, string eventName, string networkMode)
    {
        DS_Manager.CreateDelay(delay, target, eventName, networkMode);
    }

    public void CreateDelayGraph()
    {
        DS_Manager.CreateDelay(DELAY, TARGET, EVENT, NETWORKMODE);
    }
}
