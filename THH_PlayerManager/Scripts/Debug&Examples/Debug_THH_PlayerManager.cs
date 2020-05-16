
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UI = UnityEngine.UI;

public class Debug_THH_PlayerManager : UdonSharpBehaviour
{
    public THH_PlayerManager playerManager;
    public UI.Text masterHandlerText;
    public UI.Text assignedHandlerText;
    public Transform HandlerTextHolder;
    public GameObject HandlerTextPrefab;
    public UI.Text[] handlerTexts;

    void Start()
    {
        handlerTexts = new UI.Text[playerManager.handlerCount];
        for (int i = 0; i < playerManager.handlerCount; i++)
        {
            GameObject handlerText = VRCInstantiate(HandlerTextPrefab);
            handlerText.transform.SetParent(HandlerTextHolder, false);
            handlerTexts[i] = handlerText.GetComponent<UI.Text>();
        }
    }

    void Update()
    {
        if (playerManager.masterHandler == null)
        {
            masterHandlerText.text = $"Master Handler: NULL";
        }
        else
        {
            masterHandlerText.text = $"Master Handler: {playerManager.masterHandler.name}";
        }

        if (playerManager.assignedHandler == null)
        {
            assignedHandlerText.text = $"Assigned Handler: NULL";
        }
        else
        {
            assignedHandlerText.text = $"Assigned Handler: {playerManager.assignedHandler.name}";
        }

        for (int i = 0; i < handlerTexts.Length; i++)
        {
            THH_PlayerObjectHandler handler = playerManager.handlers[i];
            handlerTexts[i].text = $"Owner: {Networking.GetOwner(handler.gameObject).displayName}; Blocked?: {handler.blocked}; ";
        }
    }
}
