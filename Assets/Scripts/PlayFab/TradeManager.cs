using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;


public class TradeManager : MonoBehaviour
{
    public void GiveItem(string secondPlayerId, string myItemInstanceId)
    {
        PlayFabClientAPI.OpenTrade(new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { secondPlayerId }, // PlayFab ID
            OfferedInventoryInstanceIds = new List<string> { myItemInstanceId }
        }, GiveSuccess, DebugLogger.Instance.OnPlayFabError);
    }

    public void GiveSuccess(OpenTradeResponse r)
    {
        DebugLogger.Instance.LogText("Item " + r.Trade.AcceptedInventoryInstanceIds[0] + " to give success!");
    }

    public void ExamineTrade(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.GetTradeStatus(new GetTradeStatusRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        }, ExamineTradeSuccess, DebugLogger.Instance.OnPlayFabError);
    }

    public void ExamineTradeSuccess(GetTradeStatusResponse r)
    {
        DebugLogger.Instance.LogText("Trade examined successfully.");
    }

    // If trade requirements are acceptable, gift can be accepted using AcceptTrade
    public void AcceptGift(string firstPlayFabId, string tradeId)
    {
        PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
        {
            OfferingPlayerId = firstPlayFabId,
            TradeId = tradeId
        }, AcceptTradeSuccess, DebugLogger.Instance.OnPlayFabError);
    }

    public void AcceptTradeSuccess(AcceptTradeResponse r)
    {
        DebugLogger.Instance.LogText("Trade " + r.Trade.TradeId + " successful");
    }

}
