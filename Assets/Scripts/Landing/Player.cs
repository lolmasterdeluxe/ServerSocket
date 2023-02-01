using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviourPunCallbacks
{
    public float moveSpeed = 7.0f;
    public bool canMove = true;

    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float directionX = 0f;
    private GameObject shopPanel;
    private GameObject guildPanel;
    private GameObject friendsPanel;
    private GameObject leaderPanel;
    private GameObject eButton;

    public GameObject displayName;
    public bool isOffline = false;
    public PlayerOptionsManager playerOptionsManager;

    enum CONTACT_TYPE
    {
        NIL,
        SHOP,
        GUILD,
        FRIEND,
        LEADERBOARD,
        GAME,
    }

    private CONTACT_TYPE contactType;
    private TextMeshProUGUI currentText;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        playerOptionsManager = GetComponent<PlayerOptionsManager>();

        guildPanel = GameObject.FindGameObjectWithTag("Guild"); 
        shopPanel = GameObject.FindGameObjectWithTag("Shop");
        friendsPanel = GameObject.FindGameObjectWithTag("Friends");
        leaderPanel = GameObject.FindGameObjectWithTag("Leaderboard");
        currentText = GameObject.FindGameObjectWithTag("CurrentText").GetComponent<TextMeshProUGUI>();
        eButton = GameObject.Find("EButton");

        canMove = true;
        eButton.SetActive(false);
        photonView.RPC("SetProfile", RpcTarget.AllBuffered, PhotonNetwork.NickName, PlayerStats.ID);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleBuildingInteraction();
    }

    public void HandleMovement()
    {
        if (photonView.IsMine || isOffline)
        {
            if (canMove)
            {
                directionX = Input.GetAxisRaw("Horizontal");
                rigidBody2D.velocity = new Vector2(directionX * moveSpeed, rigidBody2D.velocity.y);

                if (Input.GetKeyDown(KeyCode.R))
                {
                    animator.SetTrigger("Is_Dancing");
                }

                //Update the animation
                UpdateAnimationUpdate();
            }
        }
    }

    public void HandleBuildingInteraction()
    {
        //If we press E
        if (eButton.activeInHierarchy && Input.GetKeyDown(KeyCode.E))
        {
            DebugLogger.Instance.LogText("Detecting 'E' input");
            switch (contactType)
            {
                case CONTACT_TYPE.SHOP:
                    shopPanel.GetComponent<ShopController>().OpenPanel(ClosePanel);
                    break;

                case CONTACT_TYPE.FRIEND:
                    friendsPanel.GetComponent<FriendsController>().OpenPanel(ClosePanel);
                    break;

                case CONTACT_TYPE.LEADERBOARD:
                    leaderPanel.GetComponent<LeaderboardController>().OpenPanel(ClosePanel);
                    break;

                case CONTACT_TYPE.GUILD:
                    guildPanel.GetComponent<GuildController>().OpenPanel(ClosePanel);
                    break;

                case CONTACT_TYPE.GAME:
                    GoToGame();
                    break;
            }
            LockPlayer(true);
            eButton.SetActive(false);
        }
    }

    public void GoToGame()
    {
        if(!isOffline)
        {
            PhotonNetwork.Disconnect();
            PhotonNetwork.LoadLevel(1);
        }
        else
        {
            SceneManager.LoadScene("Game");
        }
    }

    private void UpdateAnimationUpdate()
    {
        if (directionX > 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = false;
        }
        else if (directionX < 0f)
        {
            animator.SetBool("Is_Running", true);
            spriteRenderer.flipX = true;
        }
        else
        {
            animator.SetBool("Is_Running", false);
        }

        if (!isOffline)
            photonView.RPC("FlipSprite", RpcTarget.AllBuffered, spriteRenderer.flipX);
    }

    #region Trigger Colliders
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (photonView.IsMine || isOffline)
        {
            if (collision.gameObject.CompareTag("Guild"))
            {
                contactType = CONTACT_TYPE.GUILD;
                eButton.SetActive(true);
                currentText.text = "Guild";
            }
            else if (collision.gameObject.CompareTag("Shop"))
            {           
                contactType = CONTACT_TYPE.SHOP;
                eButton.SetActive(true);
                currentText.text = "Shop";
            }
            else if (collision.gameObject.CompareTag("Friends"))
            {
                contactType = CONTACT_TYPE.FRIEND;
                eButton.SetActive(true);
                currentText.text = "Friends";
            }
            else if (collision.gameObject.CompareTag("Leaderboard"))
            {
                contactType = CONTACT_TYPE.LEADERBOARD;
                eButton.SetActive(true);
                currentText.text = "Leaderboard";
            }
            else if (collision.gameObject.CompareTag("Game"))
            {
                contactType = CONTACT_TYPE.GAME;
                eButton.SetActive(true);
                currentText.text = "Game";
            }

            DebugLogger.Instance.LogText("Is within range of bulding: " + contactType);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (photonView.IsMine || isOffline)
        {
            contactType = CONTACT_TYPE.NIL;
            eButton.SetActive(false);
            currentText.text = "";
        }
    }

    #endregion

    public void ClosePanel()
    {
        if (photonView.IsMine || isOffline)
        {
            LockPlayer(false);
            eButton.SetActive(true);
        }
    }

    public void LockPlayer(bool isTrue)
    {
        canMove = !isTrue;
        rigidBody2D.velocity = Vector3.zero;
        animator.enabled = !isTrue;
    }


    [PunRPC]
    private void SetProfile(string nickName, string playFabId)
    {
        displayName.SetActive(true);
        displayName.GetComponent<TextMeshProUGUI>().text = nickName;
        playerOptionsManager.playFabId = playFabId;
        gameObject.name = nickName;
    }

    [PunRPC]
    private void FlipSprite(bool isTrue)
    {
        spriteRenderer.flipX = isTrue;
    }
}
