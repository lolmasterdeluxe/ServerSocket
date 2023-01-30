using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class FriendManager : MonoBehaviour
{
    [Header("Friend Search Field")]
    [SerializeField]
    private TMP_InputField searchFriendInputField;
    private List<FriendInfo> _friends = null;

    [Header("Friend Bar Prefabs")]
    [SerializeField]
    private Transform friendListContainer;
    [SerializeField]
    private Transform friendRequestContainer;
    [SerializeField]
    private Transform addFriendContainer;
    [SerializeField]
    private GameObject friendListBar;
    [SerializeField]
    private GameObject friendRequestBar;
    [SerializeField]
    private GameObject addFriendBar;

    [Header("Notifications")]
    [SerializeField]
    private GameObject notificationPanel;
    [SerializeField]
    private GameObject unfriendConfirmationPanel;

    [Header("Buttons")]
    [SerializeField]
    private Image friendListButton;
    [SerializeField]
    private Image friendRequestButton;
    [SerializeField]
    private Image searchFriendButton;

    [Header("Misc.")]
    [SerializeField]
    private TextMeshProUGUI emptyText;
    private string unfriendDisplayName;

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
            OnGetPlayerLevel(f.FriendPlayFabId, level);

           /* System.DateTime currentDate = System.DateTime.Now;
            if (currentDate.Date == f.Profile.LastLogin)
                lastOnline.text = "Last Online: Today";
            else
            {
                double numOfDays = (currentDate - f.Profile.LastLogin.Value).TotalDays;
                lastOnline.text = "Last Online: " + Mathf.RoundToInt(((float)numOfDays)) + " days ago";
            }*/
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
            for (int i = 0; i < friendListContainer.childCount; ++i)
                Destroy(friendListContainer.GetChild(i).gameObject);
            _friends = result.Friends;
            if (_friends.Count == 0)
            {
                emptyText.gameObject.SetActive(true);
                emptyText.text = "No existing friends";
            }
            else
            {
                emptyText.gameObject.SetActive(false);
                DisplayFriends(_friends); // Triggers your UI
            }
            
        }, DebugLogger.Instance.OnPlayFabError);
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
                Notify("You have added " + friendId + " as friend!");
            },
            error =>
            {
                DebugLogger.Instance.OnPlayFabError(error);
                if (error.HttpCode == 1183)
                    Notify("User already added as friend!");
                else
                    Notify("Friend request failed, please try again.");
            });
    }

    // unlike AddFriend, RemoveFriend only takes a PlayFab ID
    // you can get this from the FriendInfo object under FriendPlayFabId
    public void RemoveFriend(FriendInfo friendInfo) // To investigate
    {
        PlayFabClientAPI.RemoveFriend(new RemoveFriendRequest
        {
            FriendPlayFabId = friendInfo.FriendPlayFabId
        }, result =>
        {
            _friends.Remove(friendInfo);
            GetFriends();
            Notify(friendInfo.TitleDisplayName + " unfriended successfully!");
        }, error => 
        { 
            DebugLogger.Instance.OnPlayFabError(error);
            Notify("Unfriend unsuccessful, please try again.");
        });
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
        }, DebugLogger.Instance.OnPlayFabError);
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
            for (int i = 0; i < addFriendContainer.childCount; ++i)
                Destroy(addFriendContainer.GetChild(i).gameObject);

            GameObject searchFriendGO = Instantiate(addFriendBar, addFriendContainer);
            TextMeshProUGUI username = searchFriendGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI level = searchFriendGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI lastOnline = searchFriendGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

            username.text = result.AccountInfo.TitleInfo.DisplayName;
            OnGetPlayerLevel(result.AccountInfo.PlayFabId, level);

            System.DateTime currentDate = System.DateTime.Now;
            if (currentDate.Date == result.AccountInfo.TitleInfo.Created.Date)
                lastOnline.text = "Last Online: Today";
            else
            {
                double numOfDays = (currentDate - result.AccountInfo.TitleInfo.Created.Date).TotalDays;
                lastOnline.text = "Last Online: " + Mathf.RoundToInt(((float)numOfDays)) + " days ago";
            }
        }, error => 
        {
            Notify("No account of that name exists");
            DebugLogger.Instance.OnPlayFabError(error);
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
            OnGetPlayerLevel(result.AccountInfo.PlayFabId, level);

            System.DateTime currentDate = System.DateTime.Now;
            if (currentDate.Date == result.AccountInfo.TitleInfo.LastLogin.Value.Date)
                lastOnline.text = "Last Online: Today";
            else
            {
                double numOfDays = (currentDate - result.AccountInfo.TitleInfo.LastLogin.Value.Date).TotalDays;
                lastOnline.text = "Last Online: " + Mathf.RoundToInt(((float)numOfDays)) + " days ago";
            }
        }, error =>
        {
            Notify("No account of that name exists");
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void OnGetPlayerLevel(string playFabID, TextMeshProUGUI levelText)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = playFabID,
            Keys = null
        }, result => {
            DebugLogger.Instance.LogText("Got player level from " + playFabID);

            if (result.Data == null || !result.Data.ContainsKey("Level"))
                DebugLogger.Instance.LogText("No Level");
            else
            {
                DebugLogger.Instance.LogText("Level: " + result.Data["Level"].Value);
                levelText.text = "Lvl " + result.Data["Level"].Value;
            }

        }, error => {
            DebugLogger.Instance.LogText("Got error retrieving user data:");
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
        
    }

    public void OnReturnFriendRequest(string displayName)
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest()
        {
            TitleDisplayName = displayName
        }, result =>
        {
            //SendFriendRequest(result.AccountInfo.PlayFabId, FriendIdType.PlayFabId, PlayerStats.ID);
        }, error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    private void Notify(string notificationText)
    {
        notificationPanel.SetActive(true);
        TextMeshProUGUI notiftext = notificationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        notiftext.text = notificationText;
    }

    public void OnUnfriendConfirmation(string displayName)
    {
        unfriendConfirmationPanel.SetActive(true);
        TextMeshProUGUI confirmationText = unfriendConfirmationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        confirmationText.text = "Unfriend " + displayName + "?";
        unfriendDisplayName = displayName;
    }

    public void OnUnfriend()
    {
        foreach (FriendInfo f in _friends)
        {
            if (f.TitleDisplayName == unfriendDisplayName)
                RemoveFriend(f);
        }
    }

    public void FriendlistButtonSelect(string friendListType)
    {
        if (friendListType == "Friendlist")
        {
            friendListButton.color = new Color32(255, 255, 255, 255);
            friendRequestButton.color = new Color32(200, 200, 200, 128);
            searchFriendButton.color = new Color32(200, 200, 200, 128);
        }
        else if (friendListType == "Friend Request")
        {
            friendListButton.color = new Color32(200, 200, 200, 128);
            friendRequestButton.color = new Color32(255, 255, 255, 255);
            searchFriendButton.color = new Color32(200, 200, 200, 128);
        }
        else if (friendListType == "Search Friend")
        {
            friendListButton.color = new Color32(200, 200, 200, 128);
            friendRequestButton.color = new Color32(200, 200, 200, 128);
            searchFriendButton.color = new Color32(255, 255, 255, 255);
        }
    }
}
public enum FriendIdType 
{ 
    PlayFabId, 
    Username, 
    Email, 
    DisplayName 
};

