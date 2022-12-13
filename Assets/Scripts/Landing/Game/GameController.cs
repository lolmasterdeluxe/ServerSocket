using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    public GameObject gameOverObject;
    
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private TextMeshProUGUI highScoreText;
    [SerializeField]
    private TextMeshProUGUI finalScoreText;

    public int difficultyMax = 5;

    [HideInInspector]
    public bool isGameOver = false, distributeRewards = false;
    public float scrollSpeed = -2.5f;

    public int columnScore = 10;
    private int currentScore = 0;

    public PlayFabDataManager playFabDataManager;
    public InventoryManager inventoryManager;
    public ScrollingBackground scrollingBackground;

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

        highScoreText.text = PlayerStats.highscore.ToString();
        scoreText.text = "0";

        playFabDataManager = FindObjectOfType<PlayFabDataManager>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        scrollingBackground = GetComponent<ScrollingBackground>();
    }

    private void Update()
    {
        if (isGameOver)
        {
            foreach (Item i in PlayerStats.equippedItems)
            {
                inventoryManager.ConsumeItemRequest(i.itemInstanceId);
                PlayerStats.equippedItems.Remove(i);
            }
        }
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
        scoreText.gameObject.SetActive(false);
        highScoreText.gameObject.SetActive(false);
        scrollingBackground.backgroundScroll = false;

        if (!distributeRewards)
        {
            inventoryManager.AddVirtualCurrencies(currentScore);
            playFabDataManager.OnAddExperience(currentScore);
            distributeRewards = true;
        }

        isGameOver = true;
    }

    public void Scored(int value)
    {
        //Check if it is game over
        if (isGameOver)
            return;

        int multipliedValue = value;
        foreach (Item i in PlayerStats.equippedItems)
        {
            multipliedValue *= i.pointMultiplier;
        }
        currentScore += multipliedValue;
        scoreText.text = currentScore.ToString();
        finalScoreText.text = currentScore.ToString();

        if (currentScore >= PlayerStats.highscore)
        {
            PlayerStats.highscore = currentScore;
            playFabDataManager.OnScoreUpdate();
            highScoreText.text = PlayerStats.highscore.ToString();
            finalScoreText.text = PlayerStats.highscore.ToString();
        }
    }
}
