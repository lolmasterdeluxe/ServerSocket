using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public class RaiseEvents : MonoBehaviourPun, IOnEventCallback
{
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public virtual void OnEvent(EventData photonEvent)
    {

    }
}

public enum EventCodes
{
    ChatEventCode = 0,
    RequestTradeEventCode = 1,
}