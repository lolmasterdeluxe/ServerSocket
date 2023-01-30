using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuildInfo : MonoBehaviour
{
    GuildManager guildManager;
    public string groupName;
    public string groupId;
    public string groupType;
    // Start is called before the first frame update
    void Start()
    {
        guildManager = FindObjectOfType<GuildManager>();
    }

    public void SetGroupData(string _groupName, string _groupId, string _groupType)
    {
        groupName = _groupName;
        groupId = _groupId;
        groupType = _groupType;
    }

    public void DisplayGroupInfo()
    {
        guildManager.GetGroupInfoWithParams(groupId, groupType);
        guildManager.guildName.text = groupName;
    }
}
