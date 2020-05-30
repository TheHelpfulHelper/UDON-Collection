
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class THH_ChatLog : UdonSharpBehaviour
{
    public GameObject TextPrefab;
    public int maxLogs = 50;

    private Transform LogTextContainer;
    private int currentNumberOfLogs;
    private VerticalLayoutGroup VLG;

    public void Start()
    {
        LogTextContainer = transform.Find("Scroll View/Viewport/Content/TextContainer");
        VLG = LogTextContainer.GetComponent<VerticalLayoutGroup>();
    }
    public void Log(string _text)
    {
        GameObject newLog = VRCInstantiate(TextPrefab);
        Text newLogText = newLog.GetComponent<Text>();
        newLogText.text = _text;
        newLog.transform.SetParent(LogTextContainer, false);
        currentNumberOfLogs++;

        if (currentNumberOfLogs > maxLogs)
        {
            Destroy(LogTextContainer.GetChild(0).gameObject);
        }
    }
}
