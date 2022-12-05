using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class BuyItem : MonoBehaviour
{
    [SerializeField]
    private string catalogVersion, itemId, virtualCurrency;
    [SerializeField]
    private uint price;
    public void SetItemStats(string catalogVersion, string itemId, string virtualCurrency, uint price)
    {
        this.catalogVersion = catalogVersion;
        this.itemId = itemId;
        this.virtualCurrency = virtualCurrency;
        this.price = price;
    }
    public void BuyItemRequest()
    {
        // Current sample is hardcoded, should make it more dynamic
        /*CatalogVersion = "1.0",
        ItemId = "WEP001",
        VirtualCurrency = "RM",
        Price = 2*/
        var buyreq = new PurchaseItemRequest
        {
            // Current sample is hardcoded, should make it more dynamic
            CatalogVersion = catalogVersion,
            ItemId = itemId,
            VirtualCurrency = virtualCurrency,
            Price = (int)price
        };
        PlayFabClientAPI.PurchaseItem(buyreq,
            result => { DebugLogger.Instance.LogText("Bought!"); },
            DebugLogger.Instance.OnPlayfabError);
    }
}
