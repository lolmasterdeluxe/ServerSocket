using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PlayFabLeaderboardManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField newScore;
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
    }
}
