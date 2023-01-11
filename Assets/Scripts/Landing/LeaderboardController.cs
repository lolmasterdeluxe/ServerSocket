using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardController : MonoBehaviour
{
    public GameObject panel;
    System.Action playerCallback;
    PlayFabDataManager playFabDataManager;

    // Start is called before the first frame update
    void Start()
    {
        panel.SetActive(false);
        playFabDataManager = FindObjectOfType<PlayFabDataManager>();
    }

    public void OpenPanel(System.Action callBack = null)
    {
        panel.SetActive(true);
        playFabDataManager.OnGetLeaderboard();
        playFabDataManager.OnGetLeaderboardAroundPlayer();
        playFabDataManager.OnGetFriendsLeaderboard();
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playFabDataManager.ClearLeaderboards();
        playerCallback();
    }
}
