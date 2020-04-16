
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DelayServiceHandler : UdonSharpBehaviour
{
    private UdonBehaviour DS_Manager;

    public float DELAY;
    public UdonBehaviour TARGET;
    public string EVENT;

    void Start()
    {
        DS_Manager = (UdonBehaviour)GameObject.Find("[DS_Manager]").GetComponent(typeof(UdonBehaviour));
        if (DS_Manager == null)
        {
            Debug.Log("[DS_Handler] ERROR: Could not find DelayService Manager!");
            return;
        }
        UdonBehaviour parentUdon = (UdonBehaviour)transform.parent.gameObject.GetComponent(typeof(UdonBehaviour));
        if (parentUdon == null)
        {
            Debug.Log("[DS_Handler] ERROR: Parent has no UdonBehaviour");
            return;
        }
        parentUdon.SetProgramVariable("DS_Handler", this);
    }

    public void CreateDelay()
    {
        DS_Manager.SetProgramVariable("DELAY", DELAY);
        DS_Manager.SetProgramVariable("TARGET", TARGET);
        DS_Manager.SetProgramVariable("EVENT", EVENT);
        DS_Manager.SendCustomEvent("CreateDelay");
    }
}
