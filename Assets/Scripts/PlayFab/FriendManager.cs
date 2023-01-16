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
    private TMP_InputField searchFriendInputField, unfriendText;
    private List<FriendInfo> _friends = null;
    [SerializeField]
    private Transform friendListContainer, friendRequestContainer, addFriendContainer;
    [SerializeField]
    private GameObject friendListBar, friendRequestBar, addFriendBar, notificationPanel;

    public void DisplayFriends(List<FriendInfo> friendsCache)
    {
        friendsCache.ForEach(f =>
        {
            Debug.Log(f.FriendPlayFabId + "," + f.TitleDisplayName);

            GameObject friendGO = Instantiate(friendListBar, friendListContainer);
            TextMeshProUGUI username = friendGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI level = friendGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI lastOnline = friendGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            username.text = f.TitleDisplayName;
            level.text = OnGetPlayerLevel(f.FriendPlayFabId);
            
            System.DateTime currentDate = System.DateTime.Now;
            if (currentDate.Date == f.Profile.LastLogin.Value.Date)
                lastOnline.text = "Last Online: Today";
            else
            {
                double numOfDays = (currentDate - f.Profile.LastLogin.Value).TotalDays;
                lastOnline.text = "Last Online: " + numOfDays.ToString() + " days ago";
            }
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
            // Refresh list
            foreach (GameObject GO in friendListContainer)
                Destroy(GO);
            _friends = result.Friends;
            DisplayFriends(_friends); // Triggers your UI
        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void SendFriendRequest(FriendIdType idType, string friendId)
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
        PlayFabClientAPI.AddFriend(request,
            result =>
            {
                DebugLogger.Instance.LogText("Friend added successfully!");
                Notify("Friend request sent!");
            },
            error =>
            {
                DebugLogger.Instance.OnPlayfabError(error);
                Notify("Friend request failed, please try again.");
            });
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
        RemoveFriend(unfriendText.text);
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

    public void SearchFriend()
    {
        var req = new GetAccountInfoRequest
        {
            TitleDisplayName = searchFriendInputField.text
        };
        PlayFabClientAPI.GetAccountInfo(req
        , result =>
        {
            GameObject searchFriendGO = Instantiate(friendRequestBar, friendListContainer);
            TextMeshProUGUI username = searchFriendGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI level = searchFriendGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI lastOnline = searchFriendGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            username.text = result.AccountInfo.TitleInfo.DisplayName;
            level.text = OnGetPlayerLevel(result.AccountInfo.PlayFabId);

            System.DateTime currentDate = System.DateTime.Now;
            if (currentDate.Date == result.AccountInfo.TitleInfo.LastLogin.Value.Date)
                lastOnline.text = "Last Online: Today";
            else
            {
                double numOfDays = (currentDate - result.AccountInfo.TitleInfo.LastLogin.Value.Date).TotalDays;
                lastOnline.text = "Last Online: " + numOfDays.ToString() + " days ago";
            }
        }, error => 
        {
            Notify("No account of that name exists");
            DebugLogger.Instance.OnPlayfabError(error);
        });
    }

    public void ListFriendRequests()
    {
        var req = new GetAccountInfoRequest
        {
            TitleDisplayName = searchFriendInputField.text
        };
        PlayFabClientAPI.GetAccountInfo(req
        , result =>
        {
            GameObject searchFriendGO = Instantiate(friendRequestBar, friendListContainer);
            TextMeshProUGUI username = searchFriendGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI level = searchFriendGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI lastOnline = searchFriendGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            username.text = result.AccountInfo.TitleInfo.DisplayName;
            level.text = OnGetPlayerLevel(result.AccountInfo.PlayFabId);

            System.DateTime currentDate = System.DateTime.Now;
            if (currentDate.Date == result.AccountInfo.TitleInfo.LastLogin.Value.Date)
                lastOnline.text = "Last Online: Today";
            else
            {
                double numOfDays = (currentDate - result.AccountInfo.TitleInfo.LastLogin.Value.Date).TotalDays;
                lastOnline.text = "Last Online: " + numOfDays.ToString() + " days ago";
            }
        }, error =>
        {
            Notify("No account of that name exists");
            DebugLogger.Instance.OnPlayfabError(error);
        });
    }

    public string OnGetPlayerLevel(string playFabID)
    {
        string level = "0";
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = PlayerStats.ID,
            Keys = null
        }, result => {
            DebugLogger.Instance.LogText("Got player level:");

            if (result.Data == null || !result.Data.ContainsKey("Level"))
                DebugLogger.Instance.LogText("No Level");
            else
            {
                level = result.Data["Level"].Value;
                DebugLogger.Instance.LogText("Level: " + result.Data["Level"].Value);
            }

        }, (error) => {
            DebugLogger.Instance.LogText("Got error retrieving user data:");
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });

        return level;
    }

    private void Notify(string notificationText)
    {
        notificationPanel.SetActive(true);
        TextMeshProUGUI notiftext = notificationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        notiftext.text = notificationText;
    }
}
public enum FriendIdType 
{ 
    PlayFabId, 
    Username, 
    Email, 
    DisplayName 
};

