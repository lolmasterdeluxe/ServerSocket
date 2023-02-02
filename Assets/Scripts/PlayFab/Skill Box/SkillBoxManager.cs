using System.Collections;
using System.Collections.Generic; // SkillBoxManager.cs
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class SkillBoxManager : MonoBehaviour
{
    [SerializeField]
    SkillBox[] skillBoxes;

    public void SendJson()
    {
        List<Skill> skillList = new List<Skill>();
        foreach (var item in skillBoxes) skillList.Add(item.ReturnClass());
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<Skill>(skillList));
        Debug.Log("JSON data prepared" + stringListAsJson);
        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> // package as dictionary item
            {
                {"Skills",stringListAsJson }
            }
        };
        PlayFabClientAPI.UpdateUserData(req, result => Debug.Log("Data sent success!"), DebugLogger.Instance.OnPlayFabError);
    }
    
    public void LoadJson()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, DebugLogger.Instance.OnPlayFabError);
    }

    private void OnJSONDataReceived(GetUserDataResult r)
    {
        DebugLogger.Instance.LogText("Received JSON Data");
        if (r.Data != null && r.Data.ContainsKey("Skills"))
        {
            DebugLogger.Instance.LogText(r.Data["Skills"].Value);
            JSListWrapper<Skill> jlw = JsonUtility.FromJson<JSListWrapper<Skill>>(r.Data["Skills"].Value);
            for (int i = 0; i < skillBoxes.Length; ++i)
            {
                skillBoxes[i].SetUI(jlw.list[i]);
            }
        }
    }

}
