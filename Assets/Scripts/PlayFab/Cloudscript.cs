using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;

public class Cloudscript : MonoBehaviour
{
    private void Start()
    {
        ExeCloudScript();
    }
    public void ExeCloudScript()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest 
        { 
            FunctionName = "cldfn1", 
            FunctionParameter = new { name = PlayerStats.displayName } ,
            GeneratePlayStreamEvent = false,
            CustomTags = new Dictionary<string, string>() { { "Friend Request Status", "Pending" } }
        }, 
        result => 
        {
            DebugLogger.Instance.LogText(result.FunctionResult.ToString());
        }, 
        error => 
        {
            DebugLogger.Instance.OnPlayFabError(error);
        });
    }
}
