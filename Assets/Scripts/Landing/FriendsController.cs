using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsController : MonoBehaviour
{
    public GameObject panel;
    private FriendManager friendManager;
    System.Action playerCallback;

    // Start is called before the first frame update
    void Start()
    {
        friendManager = FindObjectOfType<FriendManager>();
        panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OpenPanel(System.Action callBack = null)
    {
        panel.SetActive(true);
        friendManager.GetFriends();
        playerCallback = callBack;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        playerCallback();
    }
}
