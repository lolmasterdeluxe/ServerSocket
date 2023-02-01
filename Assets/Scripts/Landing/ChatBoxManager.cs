using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class ChatBoxManager : RaiseEvents, IOnEventCallback
{
    public TMP_InputField chatInputField;
    public GameObject player;

    private void Update()
    {
        HandleMessageInput();
    }

    public void ToggleChatBox(GameObject chatBox)
    {
        player = GameObject.Find(PlayerStats.displayName);
        chatBox.SetActive(!chatBox.activeInHierarchy);
        player.GetComponent<Player>().LockPlayer(chatBox.activeInHierarchy);
    }

    public void HandleMessageInput()
    {
        if (player != null)
        {
            if (chatInputField.gameObject.activeInHierarchy && !string.IsNullOrEmpty(chatInputField.text) && Input.GetKeyDown(KeyCode.Return))
            {
                Vector3 chatboxPosition = new Vector3(player.transform.localPosition.x, player.transform.localPosition.y + 2f, player.transform.localPosition.z);
                object[] content = new object[] { chatInputField.text, chatboxPosition, PlayerStats.displayName };
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent((byte)EventCodes.ChatEventCode, content, raiseEventOptions, SendOptions.SendReliable);
            }
        }
    }

    public override void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)EventCodes.ChatEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameObject chatbox = PhotonNetwork.Instantiate("Chatbox", (Vector3)data[1], Quaternion.identity);
            Transform playerChatboxContainer = GameObject.Find((string)data[2]).transform.GetChild(0);
            chatbox.transform.SetParent(playerChatboxContainer);
            chatbox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (string)data[0];
            StartCoroutine(chatboxAnimation(chatbox));
        }
        DebugLogger.Instance.LogText("Chat event received!");
    }

    public IEnumerator chatboxAnimation(GameObject chatbox)
    {
        chatInputField.text = "";
        chatInputField.ForceLabelUpdate();
        RectTransform rectTransform = chatbox.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = chatbox.GetComponent<CanvasGroup>();
        yield return new WaitForSeconds(3);
        rectTransform.DOLocalMoveY(15, 5);
        canvasGroup.DOFade(0, 5);
        yield return new WaitForSeconds(10);
        PhotonNetwork.Destroy(chatbox);
        yield return null;
    }
}
