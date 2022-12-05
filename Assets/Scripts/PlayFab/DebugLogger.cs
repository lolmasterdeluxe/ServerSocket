using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugLogger : MonoBehaviour
{
    public static DebugLogger Instance { get; private set; }
    [SerializeField]
    private TextMeshProUGUI log;
    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void OnPlayfabError(PlayFabError e)
    {
        LogText("Playfab Error" + e.GenerateErrorReport());
    }

    public void LogText(string msg)
    {
        Debug.Log(msg);
        log.text +=  msg + "\n";
    }
}
