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
    private List<FriendInfo> _friendRequests = null;

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
    [SerializeField]
    private GameObject inGameNotificationPanel;

    [Header("Buttons")]
    [SerializeField]
    private Image friendListButton;
    [SerializeField]
    private Image friendRequestButton;
    [SerializeField]
    private Image searchFriendButton;

    [Header("Misc.")]
    [SerializeField]
    private TextMeshProUGUI emptyFriendListText;
    [SerializeField]
    private TextMeshProUGUI emptyFriendRequestText;

    [Header("Send Friend Request Options Menu")]
    public string recipientDisplayName;
    public string recipientID;
    private string unfriendDisplayName;

    #region List friends & friend requests
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

            emptyFriendListText.gameObject.SetActive(true);
            emptyFriendListText.text = "No existing friends";
            DisplayFriends(_friends); // Triggers your UI

        }, DebugLogger.Instance.OnPlayFabError);
    }

    public void DisplayFriends(List<FriendInfo> friendsCache)
    {
        friendsCache.ForEach(f =>
        {
            foreach (string tag in f.Tags)
                DebugLogger.Instance.LogText(tag);
            if (f.Tags[0] == "Confirmed" )
            {
                Debug.Log(f.FriendPlayFabId + ", " + f.TitleDisplayName);

                GameObject friendGO = Instantiate(friendListBar, friendListContainer);
                TextMeshProUGUI username = friendGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI level = friendGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI isOnline = friendGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();

                username.text = f.TitleDisplayName;
                OnGetPlayerLevel(f.FriendPlayFabId, level);
                emptyFriendListText.gameObject.SetActive(false);
                bool canFind = GameObject.Find(f.TitleDisplayName);

                 if (canFind)
                     isOnline.text = "<color=green>Online</color>";
                 else
                    isOnline.text = "<color=red>Offline</color>";
            }
        });
    }

    public void GetFriendRequests()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
            IncludeSteamFriends = false,
            IncludeFacebookFriends = false,
            XboxToken = null
        }, result =>
        {
            // Refresh list
            for (int i = 0; i < friendRequestContainer.childCount; ++i)
                Destroy(friendRequestContainer.GetChild(i).gameObject);

            _friendRequests = result.Friends;
            emptyFriendRequestText.gameObject.SetActive(true);
            emptyFriendRequestText.text = "No friend requests";
            ListFriendRequests(_friendRequests); // Triggers your UI

        }, DebugLogger.Instance.OnPlayFabError);
    }

    public void ListFriendRequests(List<FriendInfo> friendsCache)
    {
        friendsCache.ForEach(f =>
        {
            foreach (string tag in f.Tags)
                DebugLogger.Instance.LogText(tag);
            if (f.Tags[0] == "Pending Received" )
            {
                Debug.Log(f.FriendPlayFabId + ", " + f.TitleDisplayName);

                GameObject friendRequestGO = Instantiate(friendRequestBar, friendRequestContainer);
                TextMeshProUGUI username = friendRequestGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI level = friendRequestGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI isOnline = friendRequestGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
                AddFriendButton addFriendButton = friendRequestGO.GetComponent<AddFriendButton>();

                addFriendButton.playFabId = f.FriendPlayFabId;
                username.text = f.TitleDisplayName;
                OnGetPlayerLevel(f.FriendPlayFabId, level);
                emptyFriendRequestText.gameObject.SetActive(false);

                bool canFind = GameObject.Find(f.TitleDisplayName);
                if (canFind)
                    isOnline.text = "<color=green>Online</color>";
                else
                    isOnline.text = "<color=red>Offline</color>";
            }
        });
    }

    #endregion

    #region Add Friend / Unfriend
    public void AddFriend(FriendIdType idType, string friendId)
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
                Notify("Friend request failed, " + error);
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
    public void SendFriendRequestInGame()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "SendFriendRequest",
            FunctionParameter = new
            {
                SenderPlayFabId = PlayerStats.ID,
                FriendPlayFabId = recipientID,
            },
        },
        result =>
        {
            DebugLogger.Instance.LogText("Friend request successfully sent to " + recipientDisplayName);
            NotifyInGame("Friend request sent to " + recipientDisplayName);
        },
        error =>
        {
            NotifyInGame("Friend request failed, " + error);
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void SendFriendRequest(string displayName, string senderPlayFabId, string recipientPlayFabId)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "SendFriendRequest",
            FunctionParameter = new 
            { 
                SenderPlayFabId = senderPlayFabId,
                FriendPlayFabId = recipientPlayFabId,
            },
        },
        result =>
        {
            DebugLogger.Instance.LogText("Friend request successfully sent to " + displayName);
            Notify("Friend request sent to " + displayName);
        },
        error =>
        {
            Notify("Friend request failed, " + error);
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void AcceptFriendRequest(string displayName, string senderPlayFabId, string recipientPlayFabId)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "AcceptFriendRequest",
            FunctionParameter = new
            {
                SenderPlayFabId = senderPlayFabId,
                FriendPlayFabId = recipientPlayFabId,
            },
        },
        result =>
        {
            GetFriendRequests();
            Notify("You are now friends with " + displayName + "!");
            DebugLogger.Instance.LogText("You are now friends with " + displayName + "!");
        },
        error =>
        {
            Notify("Friend acceptance failed, " + error);
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void DeclineFriendRequest(string senderPlayFabId, string recipientPlayFabId)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "Unfriend",
            FunctionParameter = new
            {
                SenderPlayFabId = senderPlayFabId,
                FriendPlayFabId = recipientPlayFabId,
            },
        },
        result =>
        {
            GetFriendRequests();
            DebugLogger.Instance.LogText("Friend request declined");
        },
        error =>
        {
            Notify("Friend declining failed, " + error);
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void UnfriendRequest(string senderPlayFabId, string recipientPlayFabId)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "Unfriend",
            FunctionParameter = new
            {
                SenderPlayFabId = senderPlayFabId,
                FriendPlayFabId = recipientPlayFabId,
            },
        },
        result =>
        {
            GetFriends();
            DebugLogger.Instance.LogText("Friend removed");
        },
        error =>
        {
            Notify("Friend couldnt be removed, " + error);
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }
    #endregion

    #region Search for friend

    public void SearchFriend()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest
        {
            TitleDisplayName = searchFriendInputField.text
        }
        , result =>
        {
            for (int i = 0; i < addFriendContainer.childCount; ++i)
                Destroy(addFriendContainer.GetChild(i).gameObject);

            GameObject searchFriendGO = Instantiate(addFriendBar, addFriendContainer);
            TextMeshProUGUI username = searchFriendGO.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI level = searchFriendGO.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI isOnline = searchFriendGO.transform.GetChild(3).GetComponent<TextMeshProUGUI>();
            searchFriendGO.GetComponent<AddFriendButton>().playFabId = result.AccountInfo.PlayFabId;

            username.text = result.AccountInfo.TitleInfo.DisplayName;
            OnGetPlayerLevel(result.AccountInfo.PlayFabId, level);

            bool canFind = GameObject.Find(result.AccountInfo.TitleInfo.DisplayName);
            if (canFind)
                isOnline.text = "<color=green>Online</color>";
            else
                isOnline.text = "<color=red>Offline</color>";
        }, error => 
        {
            Notify("No account of that name exists");
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    #endregion

    #region Get friend data
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
    #endregion

    #region Notifications
    private void Notify(string notificationText)
    {
        notificationPanel.SetActive(true);
        TextMeshProUGUI notiftext = notificationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        notiftext.text = notificationText;
        
    }

    private void NotifyInGame(string notificationText)
    {
        inGameNotificationPanel.SetActive(true);
        TextMeshProUGUI notiftext = inGameNotificationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        notiftext.text = notificationText;
    }

    public void OnUnfriendConfirmation(string displayName)
    {
        unfriendConfirmationPanel.SetActive(true);
        TextMeshProUGUI confirmationText = unfriendConfirmationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        confirmationText.text = "Unfriend " + displayName + "?";
        unfriendDisplayName = displayName;
    }
    #endregion

    #region Button functions
    public void OnUnfriend()
    {
        foreach (FriendInfo f in _friends)
        {
            if (f.TitleDisplayName == unfriendDisplayName)
                UnfriendRequest(PlayerStats.ID, f.FriendPlayFabId);
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
    #endregion
}
public enum FriendIdType 
{ 
    PlayFabId, 
    Username, 
    Email, 
    DisplayName 
};

