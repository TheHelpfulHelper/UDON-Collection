
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class THH_CanvasLogManager : UdonSharpBehaviour
{
    public GameObject LogTextPrefab;
    private Transform LogTextContainer;

    private int maxLogs = 50;
    private int currentNumberOfLogs;

    public bool logMetaInfo = true;

    public void Start()
    {
        LogTextContainer = transform.Find("Scroll View/Viewport/Content/LogTextContainer");
    }
    public void Log(string text)
    {
        GameObject newLog = VRCInstantiate(LogTextPrefab);
        Text newLogText = newLog.GetComponent<Text>();
        newLogText.text = logMetaInfo ? $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")} <color=white>Log</color> - {text}" : $"{text}";
        newLog.transform.SetParent(LogTextContainer, false);
        currentNumberOfLogs++;

        if (currentNumberOfLogs > maxLogs)
        {
            Destroy(LogTextContainer.GetChild(0).gameObject);
        }
    }

    public void LogWarning(string text)
    {
        GameObject newLog = VRCInstantiate(LogTextPrefab);
        Text newLogText = newLog.GetComponent<Text>();
        newLogText.text = logMetaInfo ? $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")} <color=yellow>Warning</color> - {text}" : $"{text}";
        newLog.transform.SetParent(LogTextContainer, false);
        currentNumberOfLogs++;

        if (currentNumberOfLogs > maxLogs)
        {
            Destroy(LogTextContainer.GetChild(0).gameObject);
        }
    }

    public void LogError(string text)
    {
        GameObject newLog = VRCInstantiate(LogTextPrefab);
        Text newLogText = newLog.GetComponent<Text>();
        newLogText.text = logMetaInfo ? $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")} <color=red>ERROR</color> - {text}" : $"{text}";
        newLog.transform.SetParent(LogTextContainer, false);
        currentNumberOfLogs++;

        if (currentNumberOfLogs > maxLogs)
        {
            Destroy(LogTextContainer.GetChild(0).gameObject);
        }
    }
}
