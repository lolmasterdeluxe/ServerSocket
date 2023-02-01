using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public class PlayerOptionsManager : RaiseEvents, IOnEventCallback
{
    public string playFabId;
    public TradeManager tradeManager;
    public FriendManager friendManager;
    // Start is called before the first frame update
    void Start()
    {
        tradeManager = FindObjectOfType<TradeManager>();
        friendManager = FindObjectOfType<FriendManager>();
    }

    public override void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == (byte)EventCodes.RequestTradeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == playFabId)
                tradeManager.acceptTradePanel.SetActive(true);
        }
        DebugLogger.Instance.LogText("PlayFab ID set!");
    }

    public void RequestTrade()
    {
        tradeManager.waitForTradePanel.SetActive(true);
        object[] content = new object[] { playFabId };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.RequestTradeEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }


}
