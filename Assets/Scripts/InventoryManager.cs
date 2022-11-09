using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            int coins = r.VirtualCurrency["RM"]; // Replace CN with your currency
            DebugLogger.Instance.LogText("Coins: " + coins);
        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void GetCatalog()
    {
        var catreq = new GetCatalogItemsRequest
        {
            CatalogVersion = "1.0" //update catalog names
        };
        PlayFabClientAPI.GetCatalogItems(catreq,
        result =>
        {
            List<CatalogItem> items = result.Catalog;
            DebugLogger.Instance.LogText("Catalog Items");
            foreach (CatalogItem i in items)
            {
                DebugLogger.Instance.LogText(i.DisplayName + ", " + i.VirtualCurrencyPrices["RM"]);
            }
        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void GetPlayerInventory()
    {
        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv, result =>
        {
            List<ItemInstance> ii = result.Inventory;
            DebugLogger.Instance.LogText("Player Inventory");
            foreach (ItemInstance i in ii)
            {
                DebugLogger.Instance.LogText(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
            }
        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void BuyItem()
    {
        var buyreq = new PurchaseItemRequest
        {
            // Current sample is hardcoded, should make it more dynamic
            CatalogVersion = "1.0",
            ItemId = "WEP001",
            VirtualCurrency = "RM",
            Price = 2
        };
        PlayFabClientAPI.PurchaseItem(buyreq,
            result => { DebugLogger.Instance.LogText("Bought!"); }, 
            DebugLogger.Instance.OnPlayfabError);
    }
}
