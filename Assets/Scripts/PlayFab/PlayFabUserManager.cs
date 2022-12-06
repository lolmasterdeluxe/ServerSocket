using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayFabUserManager : MonoBehaviour
{
    [SerializeField]
    private GameObject registerPanel, loginPanel, menuPanel;
    [SerializeField]
    private TMP_InputField reg_email, reg_username, reg_password, reg_confirm_password, login_email, login_password, update_displayName, recovery_email;
    [SerializeField]
    private TextMeshProUGUI account_username, account_ID, displayName, loginErrorMessage, registerErrorMessage, recoveryErrorMessage;

    public void OnRegister()
    {
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = reg_email.text,
            Username = reg_username.text,
            Password = reg_password.text,
            DisplayName = reg_username.text
        };
        if (reg_password.text == reg_confirm_password.text)
        {
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnRegisterError);
        }
        else
        {
            DebugLogger.Instance.LogText("Passwords do not match, please try again");
            registerErrorMessage.text = "Passwords do not match, please try again";
        }
    }

    private void OnRegisterError(PlayFabError e)
    {
        registerErrorMessage.text = e.ErrorMessage;
        DebugLogger.Instance.LogText(e.GenerateErrorReport());
    }

    private void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        //Debug.Log("Register success");
        reg_email.text = "";
        reg_username.text = "";
        reg_password.text = "";
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);

        DebugLogger.Instance.LogText("Registration success!");
    }

    public void OnLogin()
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = login_email.text,
            Password = login_password.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetUserAccountInfo = true,
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnLoginError);
    }

    private void OnLoginError(PlayFabError e)
    {
        loginErrorMessage.text = e.ErrorMessage;
        DebugLogger.Instance.LogText(e.GenerateErrorReport());
    }

    private void OnLoginSuccess(LoginResult r)
    {
        /*loginPanel.SetActive(false);
        menuPanel.SetActive(true);

        account_username.text = "Logged in as: " + r.InfoResultPayload.AccountInfo.Username;
        account_ID.text = "User ID: " + r.InfoResultPayload.AccountInfo.PlayFabId;

        displayName.text = r.InfoResultPayload.PlayerProfile.DisplayName;
        update_displayName.text = r.InfoResultPayload.PlayerProfile.DisplayName;*/

        PlayerStats.username = r.InfoResultPayload.AccountInfo.Username;
        PlayerStats.ID = r.InfoResultPayload.AccountInfo.PlayFabId;
        PlayerStats.displayName = r.InfoResultPayload.PlayerProfile.DisplayName;

        DebugLogger.Instance.LogText("Login Success!");
        SceneTransition("Landing");
    }

    public void OnLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        SceneTransition("MainGame");
    }

    public void OnGuestLogin()
    {
        var request = new LoginWithCustomIDRequest { CustomId = "RandomCustomID", CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(request, OnGuestLoginSuccess, DebugLogger.Instance.OnPlayfabError);
    }

    private void OnGuestLoginSuccess(LoginResult result)
    {
        /*loginPanel.SetActive(false);
        menuPanel.SetActive(true);*/
        SceneTransition("Landing");
        DebugLogger.Instance.LogText("Congratulations, you made your first successful API call!");
    }

    public void UpdateDisplayName()
    {
        var updateDisplayNameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = update_displayName.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(updateDisplayNameRequest, OnDisplayNameUpdate, DebugLogger.Instance.OnPlayfabError);
    }

    private void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        displayName.text = r.DisplayName;
        DebugLogger.Instance.LogText("Display name updated!" + r.DisplayName);
    }

    public void OnResetPassword()
    {
        var accountRecoveryRequest = new SendAccountRecoveryEmailRequest
        {
            Email = recovery_email.text,
            TitleId = PlayFabSettings.TitleId
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(accountRecoveryRequest, OnResetPasswordSuccess, OnResetPasswordError);
    }

    private void OnResetPasswordError(PlayFabError e)
    {
        recoveryErrorMessage.text = e.ErrorMessage;
        DebugLogger.Instance.LogText(e.GenerateErrorReport());
    }

    private void OnResetPasswordSuccess(SendAccountRecoveryEmailResult r)
    {
        DebugLogger.Instance.LogText("Reset password sent");
        registerErrorMessage.text = "Email to reset password has been sent.";
    }

    public void SceneTransition(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
