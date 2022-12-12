using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordManager : MonoBehaviour
{
    [SerializeField]
    private Image loginPasswordEye, registerPasswordEye, confirmPasswordEye;
    [SerializeField]
    private List<Sprite> eye;
    [SerializeField]
    private TMP_InputField loginPassword, registerPassword, confirmPassword;

    public void ToggleLoginHidden()
    {
        if (loginPasswordEye.sprite == eye[0])
        {
            loginPasswordEye.sprite = eye[1];
            loginPassword.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            loginPasswordEye.sprite = eye[0];
            loginPassword.contentType = TMP_InputField.ContentType.Password;
        }
        loginPassword.ForceLabelUpdate();
    }

    public void ToggleRegisterHidden()
    {
        if (registerPasswordEye.sprite == eye[0])
        {
            registerPasswordEye.sprite = eye[1];
            registerPassword.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            registerPasswordEye.sprite = eye[0];
            registerPassword.contentType = TMP_InputField.ContentType.Password;
        }
        registerPassword.ForceLabelUpdate();
    }

    public void ToggleConfirmHidden()
    {
        if (confirmPasswordEye.sprite == eye[0])
        {
            confirmPasswordEye.sprite = eye[1];
            confirmPassword.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            confirmPasswordEye.sprite = eye[0];
            confirmPassword.contentType = TMP_InputField.ContentType.Password;
        }
        confirmPassword.ForceLabelUpdate();
    }
}
