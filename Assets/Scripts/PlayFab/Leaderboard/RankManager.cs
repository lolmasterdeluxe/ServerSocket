using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RankManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI rankNumberText, playerNameText, scoreText;
    [SerializeField]
    private Sprite star, nostar;
    [SerializeField]
    private List<Image> starIcons;
    [SerializeField]
    private Image playerIcon;

    private int playerScore;

    public void SetPlayerLeaderboardStats(int position, string playerName, int score)
    {
        playerScore = score;
        rankNumberText.text = position.ToString();
        playerNameText.text = playerName;
        scoreText.text = score.ToString();
        SetStar();

        if (playerName == PlayerStats.displayName)
        {
            playerNameText.color = Color.green;
            playerIcon.color = Color.green;
        }
    }

    private void SetStar()
    {
        if (playerScore >= 10)
            starIcons[0].sprite = star;
        if (playerScore >= 100)
            starIcons[1].sprite = star;
        if (playerScore >= 1000)
            starIcons[2].sprite = star;
        if (playerScore >= 10000)
            starIcons[3].sprite = star;
        if (playerScore >= 100000)
            starIcons[4].sprite = star;
    }
}
