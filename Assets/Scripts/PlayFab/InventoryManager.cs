using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI inventoryText, moneyText;
    [SerializeField]
    private GameObject catalogContent, itemPrefab;
    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            int coins = r.VirtualCurrency["SG"]; // Replace CN with your currency
            DebugLogger.Instance.LogText("Coins: " + coins);
            moneyText.text = "Coins: " + coins;
        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void GetCatalog()
    {
        var catreq = new GetCatalogItemsRequest
        {
            CatalogVersion = "1.0" //update catalog names
        };
        for (int i = 0; i < catalogContent.transform.childCount; ++i)
        {
            if (catalogContent.transform.GetChild(i).gameObject != null)
                Destroy(catalogContent.transform.GetChild(i).gameObject);
        }
        PlayFabClientAPI.GetCatalogItems(catreq,
        result =>
        {
            List<CatalogItem> items = result.Catalog;
            DebugLogger.Instance.LogText("Catalog Items");
            foreach (CatalogItem i in items)
            {
                GameObject catalogItem = Instantiate(itemPrefab, catalogContent.transform);
                catalogItem.GetComponent<BuyItem>().SetItemStats(i.CatalogVersion, i.ItemId, "SG", i.VirtualCurrencyPrices["SG"]);
                catalogItem.GetComponent<TextMeshProUGUI>().text = i.DisplayName + ", $" + i.VirtualCurrencyPrices["SG"];
                DebugLogger.Instance.LogText(i.DisplayName + ", " + i.VirtualCurrencyPrices["SG"]);
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
            inventoryText.text = "";
            foreach (ItemInstance i in ii)
            {
                DebugLogger.Instance.LogText(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
                inventoryText.text += i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId + "\n";
            }
        }, DebugLogger.Instance.OnPlayfabError);
    }

    
}
