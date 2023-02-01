using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Launcher : MonoBehaviourPunCallbacks
{
    public GameObject PlayerPrefab;
    public bool isOffline = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!isOffline)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        if (PlayerStats.displayName != "")
        {
            PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0, -3.0f, 0), Quaternion.identity);
        }
    }

    public override void OnConnectedToMaster()
    {
        if (!isOffline)
        {
            DebugLogger.Instance.LogText("Connected");
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.IsVisible = true;
            roomOptions.MaxPlayers = 0;
            PhotonNetwork.JoinOrCreateRoom("Default Room", roomOptions, TypedLobby.Default);
            PhotonNetwork.NickName = PlayerStats.displayName;
            DebugLogger.Instance.LogText("Joining room...");
        }
    }

    public override void OnJoinedRoom()
    {
        if (!isOffline)
        {
            DebugLogger.Instance.LogText("Room joined successfully");
            GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0, -3.0f, 0), Quaternion.identity);
        }
    }

    public void Logout()
    {
        PhotonNetwork.Disconnect();
    }
}
