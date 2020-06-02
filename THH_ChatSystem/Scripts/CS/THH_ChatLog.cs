
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class THH_ChatLog : UdonSharpBehaviour
{
    public GameObject TextPrefab;
    public int maxLogs = 50;
    public Transform TextContainer;

    private int currentNumberOfLogs;

    public void Log(string _text)
    {
        GameObject newLog = VRCInstantiate(TextPrefab);
        Text newLogText = newLog.transform.GetChild(0).GetComponent<Text>();
        newLogText.text = _text;
        newLog.transform.SetParent(TextContainer, false);
        currentNumberOfLogs++;

        if (currentNumberOfLogs > maxLogs)
        {
            Destroy(TextContainer.GetChild(0).gameObject);
        }
    }
}
