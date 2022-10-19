using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayFabUserMgt : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField email, username, password;
    [SerializeField]
    private TextMeshProUGUI log;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRegister()
    {
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = email.text,
            Username = username.text,
            Password = password.text,
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
    }

    private void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        //Debug.Log("Register success");
        UpdateMsg("Registration success!");
    }

    private void OnError(PlayFabError e)
    {
        //Debug.Log("Error"+e.GenerateErrorReport());
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    private void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        log.text = msg;
    }
}
