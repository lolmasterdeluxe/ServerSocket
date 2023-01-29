using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayFabDataManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField displayNameInputField;
    [SerializeField]
    private TextMeshProUGUI accountIDText, displayNameText, levelText, MOTDText;
    [SerializeField]
    private GameObject globalLeaderboard, localLeaderboard, friendsLeaderboard, playerRankPrefab;
    [SerializeField]
    private Image globalButton, localButton, friendsButton, expBar;
    [SerializeField]
    private bool isLanding = false;
    [SerializeField]
    private float MOTDAnimSpeed = 1, MOTDAnimTimer = 10;
    private float MOTDAnimDelay = 10;
    private bool MOTDBroadcasted = false;

    private void Start()
    {
        if (isLanding)
        {
            OnClientGetTitleData();
            OnGetUserData();
        }
    }

    private void Update()
    {
        if (isLanding)
            AnimateMOTD();
    }

    public void OnGetLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "Highscore", // playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 100,
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnGetLeaderboardSuccess, DebugLogger.Instance.OnPlayFabError);
    }

    public void OnGetLeaderboardAroundPlayer()
    {
        var lbreq = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "Highscore", // playfab leaderboard statistic name
            MaxResultsCount = 10,
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(lbreq, OnGetLeaderboardAroundPlayerSuccess, DebugLogger.Instance.OnPlayFabError);
    }

    public void OnGetFriendsLeaderboard()
    {
        var lbreq = new GetFriendLeaderboardRequest
        {
            StatisticName = "Highscore", // playfab leaderboard statistic name
            MaxResultsCount = 100,
        };
        PlayFabClientAPI.GetFriendLeaderboard(lbreq, OnGetFriendLeaderboardSuccess, DebugLogger.Instance.OnPlayFabError);
    }


    private void OnGetLeaderboardSuccess(GetLeaderboardResult r)
    {
        foreach (var item in r.Leaderboard)
        {
            GameObject playerRank = Instantiate(playerRankPrefab, globalLeaderboard.transform);
            RankManager playerRankManager = playerRank.GetComponent<RankManager>();

            playerRankManager.SetPlayerLeaderboardStats(item.Position, item.DisplayName, item.StatValue);
        }
    }

    private void OnGetLeaderboardAroundPlayerSuccess(GetLeaderboardAroundPlayerResult r)
    {
        foreach (var item in r.Leaderboard)
        {
            GameObject playerRank = Instantiate(playerRankPrefab, localLeaderboard.transform);
            RankManager playerRankManager = playerRank.GetComponent<RankManager>();

            playerRankManager.SetPlayerLeaderboardStats(item.Position, item.DisplayName, item.StatValue);
        }
    }

    private void OnGetFriendLeaderboardSuccess(GetLeaderboardResult r)
    {
        foreach (var item in r.Leaderboard)
        {
            GameObject playerRank = Instantiate(playerRankPrefab, friendsLeaderboard.transform);
            RankManager playerRankManager = playerRank.GetComponent<RankManager>();

            playerRankManager.SetPlayerLeaderboardStats(item.Position, item.DisplayName, item.StatValue);
        }
    }

    public void OnScoreUpdate()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Highscore",
                    Value = PlayerStats.highscore
                }
            }
        };
        DebugLogger.Instance.LogText("Submitting score: " + PlayerStats.highscore);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnScoreUpdateSuccess, DebugLogger.Instance.OnPlayFabError);
    }

    private void OnScoreUpdateSuccess(UpdatePlayerStatisticsResult r)
    {
        DebugLogger.Instance.LogText("Successful leaderboard sent: " + r.ToString());
    }

    public void ClearLeaderboards()
    {
        for (int i = 0; i < globalLeaderboard.transform.childCount; ++i)
        {
            Destroy(globalLeaderboard.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < localLeaderboard.transform.childCount; ++i)
        {
            Destroy(localLeaderboard.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < friendsLeaderboard.transform.childCount; ++i)
        {
            Destroy(friendsLeaderboard.transform.GetChild(i).gameObject);
        }
    }

    public void LeaderboardButtonSelect(string leaderboardType)
    {
        if (leaderboardType == "local")
        {
            localButton.color = new Color32(255, 255, 255, 255);
            globalButton.color = new Color32(200, 200, 200, 128);
            friendsButton.color = new Color32(200, 200, 200, 128);
        }
        else if (leaderboardType == "global")
        {
            localButton.color = new Color32(200, 200, 200, 128);
            globalButton.color = new Color32(255, 255, 255, 255);
            friendsButton.color = new Color32(200, 200, 200, 128);
        }
        else if (leaderboardType == "friends")
        {
            localButton.color = new Color32(200, 200, 200, 128);
            globalButton.color = new Color32(200, 200, 200, 128);
            friendsButton.color = new Color32(255, 255, 255, 255);
        }
    }

    public void OnClientGetTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result => {
                if
                    (result.Data == null || !result.Data.ContainsKey("MOTD")) Debug.Log("No MOTD");
                else
                {
                    DebugLogger.Instance.LogText("MOTD: " + result.Data["MOTD"]);
                    MOTDText.text = result.Data["MOTD"];
                }
            },
            error => {
                DebugLogger.Instance.LogText("Got error getting titleData:");
                DebugLogger.Instance.LogText(error.GenerateErrorReport());
            }
        );
    }

    public void OnAddExperience(int experience)
    {
        PlayerStats.experience += experience;
        while ((PlayerStats.experience / 10) >= PlayerStats.level)
        {
            PlayerStats.level++;
        }
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"XP", PlayerStats.experience.ToString()},
                {"Level", PlayerStats.level.ToString()}
            }
        },
        result => { 
            DebugLogger.Instance.LogText("Successfully updated user data");
        },
        error => {
            DebugLogger.Instance.LogText("Error setting user data");
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void OnGetUserData()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayerStats.ID,
            Keys = null
        }, result => {
            DebugLogger.Instance.LogText("Got user data:");

            if (result.Data == null || !result.Data.ContainsKey("XP"))
                DebugLogger.Instance.LogText("No XP");
            else
            {
                PlayerStats.experience = int.Parse(result.Data["XP"].Value);
                DebugLogger.Instance.LogText("XP: " + result.Data["XP"].Value);
            }

            if (result.Data == null || !result.Data.ContainsKey("Level"))
                DebugLogger.Instance.LogText("No Level");
            else
            {
                PlayerStats.level = int.Parse(result.Data["Level"].Value);
                levelText.text = "Level " + result.Data["Level"].Value;
                DebugLogger.Instance.LogText("Level: " + result.Data["Level"].Value);
            }

            float exp = PlayerStats.experience;
            float lvl = PlayerStats.level;
            expBar.fillAmount = exp / (lvl * 10);
        }, (error) => {
            DebugLogger.Instance.LogText("Got error retrieving user data:");
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void OnDisplayProfileDetails()
    {
        accountIDText.text = PlayerStats.ID.Substring(0, 9);
        displayNameText.text = PlayerStats.displayName;
        displayNameInputField.text = PlayerStats.displayName;
    }

    public void UpdateDisplayName()
    {
        var updateDisplayNameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayNameInputField.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(updateDisplayNameRequest, OnDisplayNameUpdate, DebugLogger.Instance.OnPlayFabError);
    }

    private void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        displayNameText.text = r.DisplayName;
        PlayerStats.displayName = r.DisplayName;
        DebugLogger.Instance.LogText("Display name updated!" + r.DisplayName);
    }

    private void AnimateMOTD()
    {
        if (MOTDBroadcasted)
            MOTDAnimDelay -= Time.deltaTime;

        RectTransform MOTDRect = MOTDText.GetComponent<RectTransform>();
        float newPosition = MOTDRect.anchoredPosition.x - MOTDAnimSpeed * Time.deltaTime;
        MOTDRect.anchoredPosition = new Vector2(newPosition, MOTDRect.anchoredPosition.y);

        if (MOTDRect.anchoredPosition.x <= -1920)
            MOTDBroadcasted = true;

        if (MOTDAnimDelay <= 0)
        {
            MOTDAnimDelay = MOTDAnimTimer;
            MOTDBroadcasted = false;
            MOTDRect.anchoredPosition = new Vector2(1920, MOTDRect.anchoredPosition.y);
        }
    }
}

public static class PlayerStats
{
    public static string username = "";
    public static string ID = "";
    public static string displayName = "";
    public static int highscore = 0;
    public static int experience = 0;
    public static int level= 0;
    public static List<Item> equippedItems = new List<Item>();
    public static string entityId;
    public static string entityType;
}
