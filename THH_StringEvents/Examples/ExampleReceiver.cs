
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ExampleReceiver : UdonSharpBehaviour
{
    // These members are required for the StringEventSystem
    private UdonBehaviour EventHandler;
    public string[] PARAMS;
    void SendStringEvent(string eventString)
    {
        EventHandler.SetProgramVariable("EventOutbox", eventString);
    }

    //====================================================================//

    public void Add()
    {
        if (PARAMS.Length != 2)
        {
            Debug.Log("Invalid parameter amount!");
            return;
        }
        int add1 = System.Int32.Parse((string)PARAMS.GetValue(0));
        int add2 = System.Int32.Parse((string)PARAMS.GetValue(1));
        int sum = add1 + add2;
        Debug.Log(sum.ToString());
    }
}
