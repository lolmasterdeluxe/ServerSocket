using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class FriendManager : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI friendList;
    [SerializeField]
    private TMP_InputField tgtFriend, tgtUnfriend;
    private List<FriendInfo> _friends = null;

    public enum FriendIdType { PlayFabId, Username, Email, DisplayName };

    public void DisplayFriends(List<FriendInfo> friendsCache)
    {
        friendList.text = "";
        friendsCache.ForEach(f =>
        {
            Debug.Log(f.FriendPlayFabId + "," + f.TitleDisplayName);
            friendList.text += f.TitleDisplayName + "\n";
            if (f.Profile != null)
                Debug.Log(f.FriendPlayFabId + "/" + f.Profile.DisplayName);
        });
    }

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result =>
        {
            _friends = result.Friends;
            DisplayFriends(_friends); // Triggers your UI
        }, DebugLogger.Instance.OnPlayfabError);
    }

    private void AddFriend(FriendIdType idType, string friendId)
    {
        var request = new AddFriendRequest();
        switch (idType)
        {
            case FriendIdType.PlayFabId:
                request.FriendPlayFabId = friendId;
                break;
            case FriendIdType.Username:
                request.FriendUsername = friendId;
                break;
            case FriendIdType.Email:
                request.FriendEmail = friendId;
                break;
            case FriendIdType.DisplayName:
                request.FriendTitleDisplayName = friendId;
                break;
            default:
                break;
        }
        // Execute request and update friends when we are done
        PlayFabClientAPI.AddFriend(request, result => { Debug.Log("Friend added successfully!"); }, 
            DebugLogger.Instance.OnPlayfabError);
    }

    public void OnAddFriend() // To add friend based on display name
    {
        AddFriend(FriendIdType.DisplayName, tgtFriend.text);
    }

    // unlike AddFriend, RemoveFriend only takes a PlayFab ID
    // you can get this from the FriendInfo object under FriendPlayFabId
    void RemoveFriend(FriendInfo friendInfo) // To investigate
    {
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest
        {
            FriendPlayFabId = friendInfo.FriendPlayFabId
        }, result =>
        {
            _friends.Remove(friendInfo);
        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void OnUnfriend()
    {
        RemoveFriend(tgtUnfriend.text);
    }

    private void RemoveFriend(string pfid)
    {
        var req = new RemoveFriendRequest
        {
            FriendPlayFabId = pfid
        };
        PlayFabClientAPI.RemoveFriend(req
        , result => {
            Debug.Log("unfriend");
        }, DebugLogger.Instance.OnPlayfabError);
    }
}
