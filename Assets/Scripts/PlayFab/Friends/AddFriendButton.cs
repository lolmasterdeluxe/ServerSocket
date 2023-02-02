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
    public string playFabId;
    // Start is called before the first frame update
    void Start()
    {
        friendManager = FindObjectOfType<FriendManager>();
    }

    public void SendFriendRequest() // To add friend based on display name
    {
        // friendManager.AddFriend(FriendIdType.DisplayName, displayName.text);
        friendManager.SendFriendRequest(displayName.text, PlayerStats.ID, playFabId);
    }

    public void Unfriend() // To add friend based on display name
    {
        friendManager.OnUnfriendConfirmation(displayName.text);
    }

    public void AcceptFriendRequest()
    {
        friendManager.AcceptFriendRequest(displayName.text, PlayerStats.ID, playFabId);
    }

    public void DeclineFriendRequest()
    {
        friendManager.DeclineFriendRequest(PlayerStats.ID, playFabId);
    }
}
