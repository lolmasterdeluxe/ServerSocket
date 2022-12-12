using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class ICManager : MonoBehaviour
{
    [SerializeField]
    ICField[] icFields;
    [SerializeField]
    private GameObject displayNameInputField, applyChangesButton;
    [SerializeField]
    private Image editModeCheckbox;
    [SerializeField]
    private Sprite tick, notTick;
    public void SendJson()
    {
        List<IC> icList = new List<IC>();
        foreach (var item in icFields) icList.Add(item.ReturnClass());
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<IC>(icList));
        Debug.Log("JSON data prepared" + stringListAsJson);
        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> // package as dictionary item
            {
                {"IC",stringListAsJson }
            }
        };
        PlayFabClientAPI.UpdateUserData(req, 
            result => { Debug.Log("Data sent success!");
            LoadJson();}, DebugLogger.Instance.OnPlayfabError);
    }

    public void LoadJson()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, DebugLogger.Instance.OnPlayfabError);
    }

    private void OnJSONDataReceived(GetUserDataResult r)
    {
        DebugLogger.Instance.LogText("Received JSON Data");
        if (r.Data != null && r.Data.ContainsKey("IC"))
        {
            DebugLogger.Instance.LogText(r.Data["IC"].Value);
            JSListWrapper<IC> jlw = JsonUtility.FromJson<JSListWrapper<IC>>(r.Data["IC"].Value);
            for (int i = 0; i < icFields.Length; ++i)
            {
                icFields[i].SetUI(jlw.list[i]);
            }
        }
    }

    public void ToggleEditMode()
    {
        GameObject[] icInputFields = new GameObject[icFields.Length];
        for (int i = 0; i < icFields.Length; ++i)
        {
            icInputFields[i] = icFields[i].transform.GetChild(2).gameObject;
        }
        foreach (GameObject icField in icInputFields)
        {
            if (icField.activeSelf)
            {
                icField.SetActive(false);
                displayNameInputField.SetActive(false);
                applyChangesButton.SetActive(false);
                editModeCheckbox.sprite = notTick;
            }
            else
            {
                icField.SetActive(true);
                displayNameInputField.SetActive(true);
                applyChangesButton.SetActive(true);
                editModeCheckbox.sprite = tick;
            }
        }
    }

}

[System.Serializable]
public class JSListWrapper<T> // Wrap JSON to list
{
    public List<T> list;
    public JSListWrapper(List<T> list) => this.list = list;
}
