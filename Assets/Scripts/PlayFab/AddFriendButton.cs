using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class AddFriendButton : MonoBehaviour
{
    FriendManager friendManager;
    [SerializeField]
    private TextMeshProUGUI displayName;
    // Start is called before the first frame update
    void Start()
    {
        friendManager = FindObjectOfType<FriendManager>();
    }

    public void OnAddFriend() // To add friend based on display name
    {
        friendManager.SendFriendRequest(FriendIdType.DisplayName, displayName.text);
    }

    public void OnUnfriend() // To add friend based on display name
    {
        friendManager.OnUnfriendConfirmation(displayName.text);
    }
}
