using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    public GameObject gameOverObject;
    
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    public int difficultyMax = 5;

    [HideInInspector]
    public bool isGameOver = false;
    public float scrollSpeed = -2.5f;

    public int columnScore = 1;
    private int score = 0;
    private int highestScore = 0;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        LoadHighScore();
        scoreText.text = "0";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetGame()
    {
        //Reset the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToLanding()
    {
        SceneManager.LoadScene("Landing");
    }

    public void GameOver()
    {
        gameOverObject.SetActive(true);
        isGameOver = true;
    }

    public void Scored(int value)
    {
        //Check if it is game over
        if (isGameOver)
            return;

        score += value;
        scoreText.text = score.ToString();

        if(score >= highestScore)
        {
            SaveHighScore(score);
        }
    }

    private void SaveHighScore(int score)
    {
        highestScore = score;
        PlayerPrefs.SetInt("highestScore", highestScore);
        highScoreText.text = highestScore.ToString();
    }

    private void LoadHighScore()
    {
        if(PlayerPrefs.HasKey("highestScore"))
        {
            highestScore = PlayerPrefs.GetInt("highestScore");
            highScoreText.text = highestScore.ToString();
        }
    }
}
