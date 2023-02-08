using PlayFab;
using PlayFab.GroupsModels;
using PlayFab.DataModels;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayFab.CloudScriptModels;

/// <summary>
/// Assumptions for this controller:
/// + Entities can be in multiple groups
///   - This is game specific, many games would only allow 1 group, meaning you'd have to perform some additional checks to validate this.
/// </summary>
public class GuildManager : MonoBehaviour
{
    // A local cache of some bits of PlayFab data
    // This cache pretty much only serves this example , and assumes that entities are uniquely identifiable by EntityId alone, which isn't technically true. Your data cache will have to be better.
    [Header("Guild Panels")]
    public GameObject guildListPanel;
    public GameObject guildPanel;
    public GameObject guildDataPanel;
    public GameObject guildRecruitmentPage;
    public GameObject guildInfoPage;

    [Header("Guild Creation Input Fields")]
    public TMP_InputField guildNameInputField;
    public TMP_InputField guildTagInputField;
    public TMP_Dropdown guildLanguageDropdown;
    public TMP_Dropdown guildRegionDropdown;
    public TMP_InputField guildDescriptionInputField;

    [Header("Guild Information GUI")]
    public TextMeshProUGUI guildName;
    public TextMeshProUGUI guildTag;
    public TextMeshProUGUI guildLanguage;
    public TextMeshProUGUI guildRegion;
    public TextMeshProUGUI guildDescription;

    [Header("Guild GUI")]
    public TextMeshProUGUI inGuildName;
    public TextMeshProUGUI inGuildBulletin;

    [Header("Guild List Prefabs")]
    public GameObject guildBarPrefab;
    public Transform guildBarContainer;

    [Header("Guild Members Prefabs")]
    public GameObject memberBarPrefab;
    public Transform memberBarContainer;

    [Header("Notification")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;

    [Header("Search bar")]
    public TMP_InputField guildSearch;
    private PlayFab.GroupsModels.EntityKey guildEntityKey;
    public string guildToJoinId;

    private PlayFab.DataModels.EntityKey adminEntityKey;
    //public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
    //public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

    #region Entity Fundamentals
    public static PlayFab.GroupsModels.EntityKey EntityKeyMaker(string entityId)
    {
        return new PlayFab.GroupsModels.EntityKey { Id = entityId };
    }
    public static PlayFab.GroupsModels.EntityKey EntityKeyMaker(string entityId, string entityType)
    {
        return new PlayFab.GroupsModels.EntityKey { Id = entityId , Type = entityType};
    }

    private void OnSharedError(PlayFab.PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    #endregion

    #region List Groups
    public void ResetGrouplist()
    {
        guildDataPanel.SetActive(false);
        guildRecruitmentPage.SetActive(false);
        guildInfoPage.SetActive(false);
        for (int i = 0; i < guildBarContainer.childCount; ++i)
        {
            Destroy(guildBarContainer.GetChild(i).gameObject);
        }
    }
    
    public void ListGroupsWithParams()
    {
        GetPlayerJoinedGroup(new PlayFab.DataModels.EntityKey { Id = PlayerStats.entityId, Type = PlayerStats.entityType });

        // Refresh list
        ResetGrouplist();
    }

    private void GetPlayerJoinedGroup(PlayFab.DataModels.EntityKey entityKey)
    {
        DebugLogger.Instance.LogText("Getting group from " + entityKey);
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "GetGuilds",
            FunctionParameter = new { Entity = entityKey },
        },
        result =>
        {
            // Parse Json & Get groups
            ListMembershipResponse groupJsonData = JsonUtility.FromJson<ListMembershipResponse>(result.FunctionResult.ToString());
            if (groupJsonData.Groups.Count == 0)
            {
                guildListPanel.SetActive(true);
                guildPanel.SetActive(false);
                DebugLogger.Instance.LogText("Groups are null");
                ListAllGroups();
            }
            else
            {
                guildEntityKey = groupJsonData.Groups[0].Group;
                inGuildName.text = groupJsonData.Groups[0].GroupName;
                ListGroupMembersWithParams();
                guildListPanel.SetActive(false);
                guildPanel.SetActive(true);
                DebugLogger.Instance.LogText("Groups is " + groupJsonData.Groups[0].GroupName);
            }
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }
    // List all guilds with admin's entity key
    private void ListAllGroups()
    {
        DebugLogger.Instance.LogText("Listing groups with " + adminEntityKey);
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
        {
            FunctionName = "GetGuilds",
            FunctionParameter = new { Entity = adminEntityKey },
        },
        result =>
        {
            // Parse Json & Get groups
            ListMembershipResponse groupJsonData = JsonUtility.FromJson<ListMembershipResponse>(result.FunctionResult.ToString());
            DebugLogger.Instance.LogText("List All Groups Json Data: " + result.FunctionResult.ToString());

            foreach (var pair in groupJsonData.Groups)
            {
                GameObject guildBar = Instantiate(guildBarPrefab, guildBarContainer);

                // Set guild visual bar
                guildBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pair.GroupName;
                guildBar.GetComponent<GuildInfo>().SetGroupData(pair.GroupName, pair.Group.Id, pair.Group.Type);

                TextMeshProUGUI memberCountText = guildBar.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI guildTotalWealthText = guildBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
                GetGroupMemberCount(pair.Group, memberCountText);
                GetGroupTotalWealth(pair.Group, guildTotalWealthText);

                DebugLogger.Instance.LogText("Showing group: " + pair.GroupName);
            }
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }
    #endregion

    #region Group Creation and deletion
    public void CreateGroupWithParams()
    {
        CreateGroup(guildNameInputField.text, EntityKeyMaker(PlayerStats.entityId, PlayerStats.entityType));
    }
    private void CreateGroup(string groupName, PlayFab.GroupsModels.EntityKey entityKey)
    {
        // A player-controlled entity creates a new group
        PlayFabGroupsAPI.CreateGroup(new CreateGroupRequest { GroupName = groupName, Entity = entityKey, CustomTags = new Dictionary<string, string>() { { "Tag", guildTagInputField.text } } },
        result => {
            Debug.Log("Group Created: " + result.GroupName + " - " + result.Group.Id);

            // Setting group data
            result.AdminRoleId = "ROLE00";
            result.MemberRoleId = "ROLE03";
            CreateRole(result.Group, "ROLE00", "Leader");
            CreateRole(result.Group, "ROLE01", "Co-Leader");
            CreateRole(result.Group, "ROLE02", "Moderator");
            CreateRole(result.Group, "ROLE03", "Member");

            // Connect admin account to every guild
            ConnectGroupToAdmin(entityKey);

            SetGroupInfo(new PlayFab.DataModels.EntityKey { Id = result.Group.Id, Type = result.Group.Type });
            ListGroupsWithParams();
        }, (error) => {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void DeleteGroup(string groupId)
    {
        // A title, or player-controlled entity with authority to do so, decides to destroy an existing group
        PlayFabGroupsAPI.DeleteGroup(new DeleteGroupRequest { Group = EntityKeyMaker(groupId) },
        result => {
            var prevRequest = (DeleteGroupRequest)result.Request;
            Debug.Log("Group Deleted: " + prevRequest.Group.Id);
            ListGroupsWithParams();
        }, (error) => {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void DeleteGroupWithAdminRights(string groupId)
    {
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
        {
            FunctionName = "DeleteGroup",
            FunctionParameter = new { GroupEntity = EntityKeyMaker(groupId) },
        },
        result =>
        {
            //DebugLogger.Instance.LogText(result.FunctionResult.ToString());
            //var prevRequest = (DeleteGroupRequest)result.Request;
            //Debug.Log("Group Deleted: " + prevRequest.Group.Id);
            ListGroupsWithParams();
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void WipeOffTestGroups()
    {
        for (int i = 0; i < guildBarContainer.childCount; ++i)
        {
            DeleteGroupWithAdminRights(guildBarContainer.GetChild(i).GetComponent<GuildInfo>().groupId);
        }
        
    }

    private void ConnectGroupToAdmin(PlayFab.GroupsModels.EntityKey entityKey)
    {
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
        {
            FunctionName = "ApplyGuild",
            FunctionParameter = new { GroupEntity = entityKey, Entity = adminEntityKey },
        },
        result =>
        {
            PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
            {
                FunctionName = "AcceptGuildApplication",
                FunctionParameter = new { GroupEntity = entityKey, Entity = adminEntityKey },
            },
            result =>
            {
                //DebugLogger.Instance.LogText(result.FunctionResult.ToString());
            },
            error =>
            {
                DebugLogger.Instance.OnPlayFabError(error);
            });
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }
    #endregion

    #region Group Role Management
    private void CreateRole(PlayFab.GroupsModels.EntityKey entityKey, string roleId, string roleName)
    {
        PlayFabGroupsAPI.CreateRole(new CreateGroupRoleRequest() { Group = entityKey, RoleId = roleId, RoleName = roleName },
        result => {
            DebugLogger.Instance.LogText(roleId + ": " + roleName + " created successfully");
        }, (error) => {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    private void SetRole(PlayFab.GroupsModels.EntityKey groupEntityKey, PlayFab.GroupsModels.EntityKey memberEntityKey, string roleId)
    {
        List<PlayFab.GroupsModels.EntityKey> membersToChange = new List<PlayFab.GroupsModels.EntityKey>() { memberEntityKey };
        PlayFabGroupsAPI.ChangeMemberRole(new ChangeMemberRoleRequest() 
        { 
            Group = groupEntityKey,  
            Members = membersToChange , 
            OriginRoleId = "ROLE03",
            DestinationRoleId = roleId
        },
        result => {
            var prevRequest = (ChangeMemberRoleRequest)result.Request;
            DebugLogger.Instance.LogText(prevRequest.Members + " changed from " + prevRequest.OriginRoleId + " to " + prevRequest.DestinationRoleId);
        }, (error) => {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }
    #endregion

    #region List Group members
    public void ResetMemberlist()
    {
        for (int i = 0; i < memberBarContainer.childCount; ++i)
        {
            Destroy(memberBarContainer.GetChild(i).gameObject);
        }
    }
    public void ListGroupMembersWithParams()
    {
        // List group members in guild
        ListGroupMembers(guildEntityKey);
    }
    private void ListGroupMembers(PlayFab.GroupsModels.EntityKey entityKey)
    {
        ResetMemberlist();
        PlayFabGroupsAPI.ListGroupMembers(new ListGroupMembersRequest { Group = entityKey }, 
        result =>
        {
            for (int i = 0; i < result.Members.Count; ++i)
            {
                for (int k = 0; k < result.Members[i].Members.Count; ++k)
                {
                    GameObject memberBar = Instantiate(memberBarPrefab, memberBarContainer);

                    PlayFab.GroupsModels.EntityKey masterEntity;
                    result.Members[i].Members[k].Lineage.TryGetValue("master_player_account", out masterEntity);

                    DebugLogger.Instance.LogText("PlayFab ID: " + PlayerStats.ID);
                    DebugLogger.Instance.LogText("Master Entity ID: " + masterEntity.Id);
                    // Get respective player data and display
                    GetMemberProfile(masterEntity, memberBar.transform.GetChild(1).GetComponent<TextMeshProUGUI>());

                    //GetMemberData(masterEntity, memberBar.transform.GetChild(5).GetChild(0).GetComponent<TextMeshProUGUI>());

                    //GetMemberVirtualCurrency(masterEntity, memberBar.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>());

                    memberBar.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Role: " + result.Members[0].RoleName;
                    DebugLogger.Instance.LogText("Guild member listed: " + result.Members[0].Members[i].Key.Id);
                }
                
            }
        }, 
        (error) =>
        {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    private void GetGroupMemberCount(PlayFab.GroupsModels.EntityKey entityKey, TextMeshProUGUI groupMemberText)
    {
        int memberCount = 0;
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
        {
            FunctionName = "GetGuildMembers",
            FunctionParameter = new { GroupEntity = entityKey },
        },
        result =>
        {
            ListGroupMembersResponse groupJsonData = JsonUtility.FromJson<ListGroupMembersResponse>(result.FunctionResult.ToString());
            for (int i = 0; i < groupJsonData.Members.Count; ++i)
            {
                memberCount += groupJsonData.Members[i].Members.Count;
            }
            groupMemberText.text = memberCount.ToString();
        },
        (error) =>
        {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    private void GetGroupTotalWealth(PlayFab.GroupsModels.EntityKey entityKey, TextMeshProUGUI totalWealthText)
    {
        int totalWealth = 0;
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
        {
            FunctionName = "GetGuildMembers",
            FunctionParameter = new { GroupEntity = entityKey },
        },
        result =>
        {
            ListGroupMembersResponse groupJsonData = JsonUtility.FromJson<ListGroupMembersResponse>(result.FunctionResult.ToString());
            for (int i = 0; i < groupJsonData.Members.Count; ++i)
            {
                for (int k = 0; k < groupJsonData.Members[i].Members.Count; ++k)
                {
                    AddMemberVirtualCurrency(totalWealth, groupJsonData.Members[i].Members[k].Key);
                }
            }
            totalWealthText.text = "$" + totalWealth;
        },
        (error) =>
        {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    #endregion

    #region Get Member Data
    private void GetMemberProfile(PlayFab.GroupsModels.EntityKey entityKey, TextMeshProUGUI profileText)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "GetPlayerProfile",
            FunctionParameter = new { PlayFabId = entityKey.Id },
        },
        result =>
        {
            // Parse Json & Get groups
            GetPlayerProfileResult profile = JsonUtility.FromJson<GetPlayerProfileResult>(result.FunctionResult.ToString());
            profileText.text = profile.PlayerProfile.DisplayName;
            DebugLogger.Instance.LogText("Member profile received successfully.");
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    private void GetMemberData(PlayFab.GroupsModels.EntityKey entityKey, TextMeshProUGUI dataText)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "GetPlayerData",
            FunctionParameter = new { PlayFabId = entityKey.Id },
        },
        result =>
        {
            // Parse Json & Get groups
            GetUserDataResult playerData = JsonUtility.FromJson<GetUserDataResult>(result.FunctionResult.ToString());
            if (playerData.Data != null && playerData.Data.ContainsKey("Level"))
                dataText.text = playerData.Data["Level"].Value;
            DebugLogger.Instance.LogText("Player data received successfully.");
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    private void GetMemberVirtualCurrency(PlayFab.GroupsModels.EntityKey entityKey, TextMeshProUGUI moneyText)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "GetPlayerInventory",
            FunctionParameter = new { PlayFabId = entityKey.Id },
        },
        result =>
        {
            // Parse Json & Get groups
            int money = 0;
            GetUserInventoryResult playerInventory = JsonUtility.FromJson<GetUserInventoryResult>(result.FunctionResult.ToString());
            DebugLogger.Instance.LogText(result.FunctionResult.ToString());
            money = playerInventory.VirtualCurrency["SG"];
            moneyText.text = money.ToString();
            DebugLogger.Instance.LogText("Player virtual currency received successfully.");
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    private void AddMemberVirtualCurrency(int moneyToAdd, PlayFab.GroupsModels.EntityKey entityKey)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
        {
            FunctionName = "GetPlayerInventory",
            FunctionParameter = new { PlayFabId = entityKey.Id },
        },
        result =>
        {
            // Parse Json & Get groups
            int money = 0;
            GetUserInventoryResult playerInventory = JsonUtility.FromJson<GetUserInventoryResult>(result.FunctionResult.ToString());
            playerInventory.VirtualCurrency.TryGetValue("SGD", out money);
            moneyToAdd += money;
            DebugLogger.Instance.LogText("Player virtual currency received successfully.");
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }
    #endregion

    #region Search for group
    public void SearchForGuild(TMP_InputField guildName)
    {
        GetGroup(guildName.text);
    }
    private void GetGroup(string groupName)
    {
        ResetGrouplist();
        PlayFabGroupsAPI.GetGroup(new GetGroupRequest { GroupName = groupName }, 
        result => 
        {
            GameObject guildBar = Instantiate(guildBarPrefab, guildBarContainer);
            guildBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = result.GroupName;
            guildBar.GetComponent<GuildInfo>().SetGroupData(result.GroupName, result.Group.Id, result.Group.Type);

            TextMeshProUGUI memberCountText = guildBar.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI guildTotalWealthText = guildBar.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
            DebugLogger.Instance.LogText("Group searched: " + result.Group.Id + "\nType: " + result.Group.Type);
            GetGroupMemberCount(result.Group, memberCountText);
            GetGroupTotalWealth(result.Group, guildTotalWealthText);

            DebugLogger.Instance.LogText("Searched for group: " + result.GroupName);
        }, 
        (error) => {
            notificationPanel.SetActive(true);
            notificationText.text = "No such group of that name exists!";
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    #endregion

    #region Group data setting / getting

    private void SetGroupInfo(PlayFab.DataModels.EntityKey entity)
    {
        GroupData groupData = new GroupData(guildTagInputField.text, guildLanguageDropdown.itemText.text, guildRegionDropdown.itemText.text, guildDescriptionInputField.text);
        string groupJsonData = JsonUtility.ToJson(groupData);
        Debug.Log("JSON data prepared" + groupJsonData);

        List<SetObject> dataList = new List<SetObject>()
        {
            new SetObject()
            {
                ObjectName = "GroupData",
                DataObject = groupJsonData
            },
            // A free-tier customer may store up to 3 objects on each entity
        };

        PlayFabDataAPI.SetObjects(new SetObjectsRequest()
        {
            Entity = entity,
            Objects = dataList,
        }, (setResult) => {
            DebugLogger.Instance.LogText(setResult.ProfileVersion.ToString());
        }, DebugLogger.Instance.OnPlayFabError);
    }

    public void GetGroupInfoWithParams(string entityId, string entityType)
    {
        //GetGroupInfo(new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType });
        GetGroupInfoNonMember(new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType });
    }

    private void GetGroupInfo(PlayFab.DataModels.EntityKey entity)
    {
        var getRequest = new GetObjectsRequest { Entity = entity };
        PlayFabDataAPI.GetObjects(getRequest,
            result => 
            { 
                var objs = result.Objects;
                DisplayGroupInfo(objs);
                DebugLogger.Instance.LogText("Group info received successfully");
            },
            DebugLogger.Instance.OnPlayFabError
        );
    }

    private void GetGroupInfoNonMember(PlayFab.DataModels.EntityKey entity)
    {
        PlayFabCloudScriptAPI.ExecuteEntityCloudScript(new ExecuteEntityCloudScriptRequest
        {
            FunctionName = "GetGuildInfo",
            FunctionParameter = new { GroupEntity = entity },
        },
        result =>
        {
            // Parse Json & Get groups
            Debug.Log(result.FunctionResult.ToString());
            GetObjectsResponse groupJsonData = JsonUtility.FromJson<GetObjectsResponse>(result.FunctionResult.ToString());
            DisplayGroupInfo(groupJsonData.Objects);
            DebugLogger.Instance.LogText("Group info received successfully");
        },
        error =>
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }

    public void DisplayGroupInfo(Dictionary<string, ObjectResult> groupData)
    {
        ObjectResult groupDataResult;
        groupData.TryGetValue("GroupData", out groupDataResult);
        DebugLogger.Instance.LogText(groupDataResult.ToJson());

        GroupData groupJsonData = JsonUtility.FromJson<GroupData>(groupDataResult.DataObject.ToString());
        guildTag.text = groupJsonData.tag;
        guildLanguage.text = groupJsonData.language;
        guildRegion.text = groupJsonData.region;
        guildDescription.text = groupJsonData.description;
        guildDataPanel.SetActive(true);
        guildRecruitmentPage.SetActive(true);
        guildInfoPage.SetActive(false);
    }

    #endregion

    #region Group Invitations and Applications

    public void InviteToGroup(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        // A player-controlled entity invites another player-controlled entity to an existing group
        PlayFabGroupsAPI.InviteToGroup(new InviteToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey },
        result =>
        {
            OnInvite(result);
        },
        error =>
        {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }
    public void OnInvite(InviteToGroupResponse response)
    {
        var prevRequest = (InviteToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request

        PlayFabGroupsAPI.AcceptGroupInvitation(new AcceptGroupInvitationRequest { Group = EntityKeyMaker(prevRequest.Group.Id), Entity = prevRequest.Entity },
        result =>
        {
            var prevRequest = (AcceptGroupInvitationRequest)response.Request;
            Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
        },
        error =>
        {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void JoinGroupWithParams()
    {
        ApplyToGroup(guildToJoinId, new PlayFab.GroupsModels.EntityKey { Id = PlayerStats.entityId , Type = PlayerStats.entityType});
    }

    public void ApplyToGroup(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        PlayFabGroupsAPI.ApplyToGroup(new ApplyToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey },  
            result=> 
            {
                var prevRequest1 = (ApplyToGroupRequest)result.Request;
                AcceptApplication(prevRequest1.Group, prevRequest1.Entity);
            },
            error=> 
            {
                DebugLogger.Instance.LogText(error.GenerateErrorReport());
            });
    }
    public void AcceptApplication(PlayFab.GroupsModels.EntityKey groupKey, PlayFab.GroupsModels.EntityKey entityKey)
    {
        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        PlayFabGroupsAPI.AcceptGroupApplication(new AcceptGroupApplicationRequest { Group = groupKey, Entity = entityKey },
        result =>
        {
            var prevRequest2 = (AcceptGroupApplicationRequest)result.Request;
            Debug.Log("Entity Added to Group: " + prevRequest2.Entity.Id + " to " + prevRequest2.Group.Id);
        },
        error =>
        {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    #endregion

    #region Group Administrative Functions
    public void LeaveGroup()
    {
        KickMember(guildEntityKey.Id, new PlayFab.GroupsModels.EntityKey { Id = PlayerStats.entityId, Type = PlayerStats.entityType });
    }
    public void KickMember(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        PlayFabGroupsAPI.RemoveMembers(new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<PlayFab.GroupsModels.EntityKey> { entityKey } },
        result => {
            var prevRequest = (RemoveMembersRequest)result.Request;
            Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
            ListGroupsWithParams();
        }, (error) => {
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }
    #endregion
}

[System.Serializable]
public class GroupData
{
    public string tag;
    public string language;
    public string region;
    public string description;

    public GroupData(string _tag, string _language, string _region, string _description)
    {
        tag = _tag;
        language = _language;
        region = _region;
        description = _description;
    }
}
