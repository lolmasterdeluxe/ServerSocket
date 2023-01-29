using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildController : MonoBehaviour
{
    public GameObject panel;
    System.Action playerCallback;
    GuildManager guildManager;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
        guildManager = FindObjectOfType<GuildManager>();
    }
    public void OpenPanel(System.Action callBack = null)
    {
        panel.SetActive(true);
        guildManager.ListGroupsWithParams();
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        guildManager.ResetGrouplist();
        playerCallback();
    }
}
