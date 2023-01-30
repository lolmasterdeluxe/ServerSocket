using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab.AuthenticationModels;

public class PlayFabUserManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject registerPanel;
    [SerializeField]
    private GameObject loginPanel;

    [Header("Input Fields")]
    [SerializeField]
    private TMP_InputField reg_email;
    [SerializeField]
    private TMP_InputField reg_username;
    [SerializeField]
    private TMP_InputField reg_password;
    [SerializeField]
    private TMP_InputField reg_confirm_password;
    [SerializeField]
    private TMP_InputField login_email;
    [SerializeField]
    private TMP_InputField login_password;
    [SerializeField]
    private TMP_InputField recovery_email;

    [Header("Error Message GUI")]
    [SerializeField]
    private TextMeshProUGUI loginErrorMessage;
    private TextMeshProUGUI registerErrorMessage;
    private TextMeshProUGUI recoveryErrorMessage;

    private void Start()
    {
        // Temporary Instant Login
        login_email.text = "kiddrifdi@gmail.com";
        login_password.text = "Revolver360";
        login_email.ForceLabelUpdate();
        login_password.ForceLabelUpdate();
    }

    public void OnRegister()
    {
        if (reg_password.text == reg_confirm_password.text)
        {
            PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest
            {
                Email = reg_email.text,
                Username = reg_username.text,
                Password = reg_password.text,
                DisplayName = reg_username.text
            },
            result =>
            {
                reg_email.text = "";
                reg_username.text = "";
                reg_password.text = "";
                registerPanel.SetActive(false);
                loginPanel.SetActive(true);
                registerErrorMessage.text = "Registration success!";
                DebugLogger.Instance.LogText("Registration success!");
            }
            , (error) =>
            {
                registerErrorMessage.text = error.ErrorMessage;
                DebugLogger.Instance.LogText(error.GenerateErrorReport());
            });
        }
        else
        {
            DebugLogger.Instance.LogText("Passwords do not match, please try again");
            registerErrorMessage.text = "Passwords do not match, please try again";
        }
    }

    public void OnLogin()
    {
        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest
        {
            Email = login_email.text,
            Password = login_password.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true,
                GetUserAccountInfo = true,
                GetPlayerStatistics = true
            }
        },
        result =>
        {
            PlayerStats.username = result.InfoResultPayload.AccountInfo.Username;
            PlayerStats.ID = result.InfoResultPayload.AccountInfo.PlayFabId;
            PlayerStats.displayName = result.InfoResultPayload.PlayerProfile.DisplayName;

            foreach (var item in result.InfoResultPayload.PlayerStatistics)
            {
                if (item.StatisticName == "Highscore")
                    PlayerStats.highscore = item.Value;
                DebugLogger.Instance.LogText("PlayerStats.highscore: " + PlayerStats.highscore);
            }

            PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest(),
            (entityResult) =>
            {
                PlayerStats.entityId = entityResult.Entity.Id;
                PlayerStats.entityType = entityResult.Entity.Type;
            }, DebugLogger.Instance.OnPlayFabError); // Define your own OnPlayFabError function to report errors

            DebugLogger.Instance.LogText("Login Success!");
            SceneTransition("Landing");
        }, 
        (error) =>
        {
            loginErrorMessage.text = error.ErrorMessage;
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void OnLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        SceneTransition("MainGame");
    }

    public void OnGuestLogin()
    {
        string customID = "GuestID_" + Random.Range(100000, 1000000); ; 
        var request = new LoginWithCustomIDRequest { CustomId = customID, CreateAccount = true };
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest { CustomId = customID, CreateAccount = true },
        result =>
        {
            SceneTransition("Landing");
        }, 
        (error) =>
        {
            loginErrorMessage.text = error.ErrorMessage;
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void OnResetPassword()
    {
        PlayFabClientAPI.SendAccountRecoveryEmail(new SendAccountRecoveryEmailRequest
        {
            Email = recovery_email.text,
            TitleId = PlayFabSettings.TitleId
        }, 
        result =>
        {
            DebugLogger.Instance.LogText("Reset password sent");
            recoveryErrorMessage.text = "Reset password email sent.";
        }, 
        (error) =>
        {
            recoveryErrorMessage.text = error.ErrorMessage;
            DebugLogger.Instance.LogText(error.GenerateErrorReport());
        });
    }

    public void SceneTransition(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
