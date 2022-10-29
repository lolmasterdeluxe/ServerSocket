using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayFabDataManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField newScore, xpInputField, levelInputField;
    [SerializeField]
    private TextMeshProUGUI leaderboardText;

    public void OnGetLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "Highscore", // playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10,
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnGetLeaderboardSuccess, DebugLogger.Instance.OnPlayfabError);
    }

    private void OnGetLeaderboardSuccess(GetLeaderboardResult r)
    {
        string leaderboardString = "Leaderboard:\n";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.Position + " / " + item.PlayFabId + " / " + item.DisplayName + " / " + item.StatValue + "\n";
            leaderboardString += onerow;
            if (item.PlayFabId == PlayerStats.ID)
            {
                newScore.text = item.StatValue.ToString();
            }
        }
        DebugLogger.Instance.LogText(leaderboardString);
        leaderboardText.text = leaderboardString;
    }

    public void OnLeaderboardUpdate()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "Highscore",
                    Value = int.Parse(newScore.text)
                }
            }
        };
        DebugLogger.Instance.LogText("Submitting score: " + newScore.text);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdateSuccess, DebugLogger.Instance.OnPlayfabError);
    }

    private void OnLeaderboardUpdateSuccess(UpdatePlayerStatisticsResult r)
    {
        DebugLogger.Instance.LogText("Successful leaderboard sent: " + r.ToString());
        OnGetLeaderboard();
    }

    public void OnClientGetTitleData()
    {
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result => {
                if (result.Data == null || !result.Data.ContainsKey("MOTD")) Debug.Log("No MOTD");
                else DebugLogger.Instance.LogText("MOTD: " + result.Data["MOTD"]);
            },
            error => {
                DebugLogger.Instance.LogText("Got error getting titleData:");
                DebugLogger.Instance.LogText(error.GenerateErrorReport());
            }
        );
    }

    public void OnSetUserData()
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>() {
                {"XP", xpInputField.text},
                {"Level", levelInputField.text}
            }
        },
        result => { 
            DebugLogger.Instance.LogText("Successfully updated user data");
            OnGetUserData();
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
            if (result.Data == null || !result.Data.ContainsKey("XP")) DebugLogger.Instance.LogText("No XP");
            else DebugLogger.Instance.LogText("XP: " + result.Data["XP"].Value);
            if (result.Data == null || !result.Data.ContainsKey("Level")) DebugLogger.Instance.LogText("No Level");
            else DebugLogger.Instance.LogText("Level: " + result.Data["Level"].Value);
        }, (error) => {
            DebugLogger.Instance.LogText("Got error retrieving user data:");
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }
}
