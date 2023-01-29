using PlayFab;
using PlayFab.GroupsModels;
using PlayFab.DataModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Assumptions for this controller:
/// + Entities can be in multiple groups
///   - This is game specific, many games would only allow 1 group, meaning you'd have to perform some additional checks to validate this.
/// </summary>
public class GuildManager : MonoBehaviour
{
    // A local cache of some bits of PlayFab data
    // This cache pretty much only serves this example , and assumes that entities are uniquely identifiable by EntityId alone, which isn't technically true. Your data cache will have to be better.
    [Header("Guild Creation Input Fields")]
    public TMP_InputField guildName;
    public TMP_InputField guildTag;
    public TMP_InputField guildRegion;
    public TMP_InputField guildLanguage;
    public TMP_InputField guildDescription;

    public TextMeshProUGUI recruitmentDescription;

    [Header("Guild Bar Prefabs")]
    public GameObject guildBarPrefab;
    public Transform guildBarContainer;
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
        for (int i = 0; i < guildBarContainer.childCount; ++i)
        {
            Destroy(guildBarContainer.GetChild(i).gameObject);
        }
    }
    
    public void ListGroupsWithParams()
    {
        ListGroups(EntityKeyMaker(PlayerStats.entityId, PlayerStats.entityType));
        DebugLogger.Instance.LogText("Listing groups with " + PlayerStats.entityId);
        // Refresh list
        ResetGrouplist();
    }
    private void ListGroups(PlayFab.GroupsModels.EntityKey entityKey)
    {
        var request = new ListMembershipRequest { Entity = entityKey };
        PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
    }
    private void OnListGroups(ListMembershipResponse response)
    {
        var prevRequest = (ListMembershipRequest)response.Request;

        foreach (var pair in response.Groups)
        {
            GameObject guildBar = Instantiate(guildBarPrefab, guildBarContainer);
            guildBar.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pair.GroupName;
            guildBar.GetComponent<GuildInfo>().SetGroupData(pair.GroupName, pair.Group.Id, pair.Group.Type);

            DebugLogger.Instance.LogText("Showing group: " + pair.GroupName);
        }
    }
    #endregion

    #region Group Creation and deletion
    public void CreateGroupWithParams()
    {
        CreateGroup(guildName.text, EntityKeyMaker(PlayerStats.entityId, PlayerStats.entityType));
    }
    private void CreateGroup(string groupName, PlayFab.GroupsModels.EntityKey entityKey)
    {
        // A player-controlled entity creates a new group
        var request = new CreateGroupRequest { GroupName = groupName, Entity = entityKey, CustomTags = new Dictionary<string, string>() { { "Tag", guildTag.text } } };
        PlayFabGroupsAPI.CreateGroup(request, OnCreateGroup, OnSharedError);
    }

    private void OnCreateGroup(CreateGroupResponse response)
    {
        Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);

        var prevRequest = (CreateGroupRequest)response.Request;
        // Setting group data
        SetGroupInfo(new PlayFab.DataModels.EntityKey { Id = response.Group.Id, Type = response.Group.Type });
        ListGroupsWithParams();
    }
    public void DeleteGroup(string groupId)
    {
        // A title, or player-controlled entity with authority to do so, decides to destroy an existing group
        var request = new DeleteGroupRequest { Group = EntityKeyMaker(groupId) };
        PlayFabGroupsAPI.DeleteGroup(request, OnDeleteGroup, OnSharedError);
    }
    private void OnDeleteGroup(EmptyResponse response)
    {
        var prevRequest = (DeleteGroupRequest)response.Request;
        Debug.Log("Group Deleted: " + prevRequest.Group.Id);
        ListGroupsWithParams();
    }

    public void WipeOffTestGroups()
    {
        for (int i = 0; i < guildBarContainer.childCount; ++i)
        {
            DeleteGroup(guildBarContainer.GetChild(i).GetComponent<GuildInfo>().groupId);
        }
        
    }
    #endregion

    #region Group data setting / getting

    private void SetGroupInfo(PlayFab.DataModels.EntityKey entity)
    {
        GroupData groupData = new GroupData(guildLanguage.text, guildRegion.text, guildDescription.text);
        string groupJsonData = JsonUtility.ToJson(groupData);
        Debug.Log("JSON data prepared" + groupJsonData);

        var dataList = new List<SetObject>()
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
        GetGroupInfo(new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType });
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

    public void DisplayGroupInfo(Dictionary<string, ObjectResult> groupData)
    {
        ObjectResult groupDataResult;
        groupData.TryGetValue("GroupData", out groupDataResult);
        //groupDataResult = groupData.GetValueOrDefault("GroupData");
        DebugLogger.Instance.LogText(groupDataResult.ToJson());

        GroupData groupJsonData = JsonUtility.FromJson<GroupData>(groupDataResult.DataObject.ToString());
        recruitmentDescription.text = groupJsonData.description;
    }

    #endregion

    #region Group Invitations and Applications

    public void InviteToGroup(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        // A player-controlled entity invites another player-controlled entity to an existing group
        var request = new InviteToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
        PlayFabGroupsAPI.InviteToGroup(request, OnInvite, OnSharedError);
    }
    public void OnInvite(InviteToGroupResponse response)
    {
        var prevRequest = (InviteToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupInvitationRequest { Group = EntityKeyMaker(prevRequest.Group.Id), Entity = prevRequest.Entity };
        PlayFabGroupsAPI.AcceptGroupInvitation(request, OnAcceptInvite, OnSharedError);
    }
    public void OnAcceptInvite(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupInvitationRequest)response.Request;
        Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
    }

    public void ApplyToGroup(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        // A player-controlled entity applies to join an existing group (of which they are not already a member)
        var request = new ApplyToGroupRequest { Group = EntityKeyMaker(groupId), Entity = entityKey };
        PlayFabGroupsAPI.ApplyToGroup(request, OnApply, OnSharedError);
    }
    public void OnApply(ApplyToGroupResponse response)
    {
        var prevRequest = (ApplyToGroupRequest)response.Request;

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        var request = new AcceptGroupApplicationRequest { Group = prevRequest.Group, Entity = prevRequest.Entity };
        PlayFabGroupsAPI.AcceptGroupApplication(request, OnAcceptApplication, OnSharedError);
    }
    public void OnAcceptApplication(EmptyResponse response)
    {
        var prevRequest = (AcceptGroupApplicationRequest)response.Request;
        Debug.Log("Entity Added to Group: " + prevRequest.Entity.Id + " to " + prevRequest.Group.Id);
    }

    #endregion

    #region Group Administrative Functions
    public void KickMember(string groupId, PlayFab.GroupsModels.EntityKey entityKey)
    {
        var request = new RemoveMembersRequest { Group = EntityKeyMaker(groupId), Members = new List<PlayFab.GroupsModels.EntityKey> { entityKey } };
        PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
    }
    private void OnKickMembers(EmptyResponse response)
    {
        var prevRequest = (RemoveMembersRequest)response.Request;

        Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
    }
    #endregion
}

[System.Serializable]
public class GroupData
{
    public string language;
    public string region;
    public string description;

    public GroupData(string _language, string _region, string _description)
    {
        language = _language;
        region = _region;
        description = _description;
    }
}
