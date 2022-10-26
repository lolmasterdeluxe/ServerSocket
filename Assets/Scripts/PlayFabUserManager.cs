using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayFabUserManager : MonoBehaviour
{
    [SerializeField]
    private GameObject registerPanel, loginPanel, menuPanel;
    [SerializeField]
    private TMP_InputField reg_email, reg_username, reg_password, reg_confirm_password, login_email, login_password;
    [SerializeField]
    private TextMeshProUGUI log;

    public void OnRegister()
    {
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = reg_email.text,
            Username = reg_username.text,
            Password = reg_password.text,
        };
        if (reg_password.text == reg_confirm_password.text)
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
        else
            UpdateMsg("Passwords do not match, please try again");
    }

    private void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        //Debug.Log("Register success");
        reg_email.text = "";
        reg_username.text = "";
        reg_password.text = "";
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        UpdateMsg("Registration success!");
    }

    public void OnLogin()
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = login_email.text,
            Password = login_password.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
    }

    private void OnLoginSuccess(LoginResult r)
    {
        loginPanel.SetActive(false);
        menuPanel.SetActive(true);
        UpdateMsg("Login Success!");
    }

    public void OnLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        login_email.text = "";
        login_password.text = "";
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
