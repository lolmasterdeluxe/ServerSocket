using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class TradeManager : MonoBehaviour
{
    public string tradePlayerId;
    public GameObject acceptTradePanel;
    public GameObject waitForTradePanel;
    public GameObject tradePanel;
    public void AcceptTrade()
    {
        tradePanel.SetActive(true);
    }
    public void GiveItem(string secondPlayerId, string myItemInstanceId)
    {
        PlayFabClientAPI.OpenTrade(new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { secondPlayerId }, // PlayFab ID
            OfferedInventoryInstanceIds = new List<string> { myItemInstanceId }
        }, 
        result => 
        { 
            DebugLogger.Instance.LogText("Item " + result.Trade.AcceptedInventoryInstanceIds[0] + " to give success!"); 
        }, 
        DebugLogger.Instance.OnPlayFabError);
    }

    public void ExamineTrade(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.GetTradeStatus(new GetTradeStatusRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        },
        result =>
        { 
            DebugLogger.Instance.LogText("Trade examined successfully."); 
        }, 
        DebugLogger.Instance.OnPlayFabError);
    }

    // If trade requirements are acceptable, gift can be accepted using AcceptTrade
    public void AcceptGift(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        }, 
        result=> 
        { 
            DebugLogger.Instance.LogText("Trade " + result.Trade.TradeId + " successful"); 
        }, 
        DebugLogger.Instance.OnPlayFabError);
    }

}
