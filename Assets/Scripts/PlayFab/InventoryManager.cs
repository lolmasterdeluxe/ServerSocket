using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI inventoryText, shopMoneyText;
    [SerializeField]
    private GameObject catalogContent, inventoryContent, shopItemPrefab, inventoryItemPrefab;
    [SerializeField]
    private List<Sprite> itemIcons;
    private Sprite itemIcon;

    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            int coins = r.VirtualCurrency["SG"]; // Replace CN with your currency
            DebugLogger.Instance.LogText("Coins: " + coins);
            shopMoneyText.text = coins.ToString();
        }, DebugLogger.Instance.OnPlayfabError);
    }
    public void GetVirtualCurrencies(TextMeshProUGUI moneyText)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            int coins = r.VirtualCurrency["SG"]; // Replace CN with your currency
            DebugLogger.Instance.LogText("Coins: " + coins);
            moneyText.text = coins.ToString();
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
                GameObject catalogItem = Instantiate(shopItemPrefab, catalogContent.transform);
                catalogItem.GetComponent<ItemManager>().SetItemStats(i.CatalogVersion, i.ItemId, "SG", i.VirtualCurrencyPrices["SG"]);
                SetIcon(i);
                catalogItem.GetComponent<ItemManager>().SetItemVisuals(i.DisplayName, i.ItemClass, itemIcon);
                catalogItem.GetComponent<ItemManager>().SetItemPrice();
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
                GameObject inventoryItem = Instantiate(inventoryItemPrefab, inventoryContent.transform);
                SetIcon(i);
                ItemManager inventoryItemManager = inventoryItem.GetComponent<ItemManager>();
                inventoryItemManager.itemId = i.ItemId;
                inventoryItemManager.SetItemVisuals(i.DisplayName, i.ItemClass, itemIcon);

                int tempCount = 0;
                for (int k = 0; k < inventoryContent.transform.childCount; ++k)
                {
                    ItemManager itemManagerChild = inventoryContent.transform.GetChild(k).GetComponent<ItemManager>();
                    if (inventoryItemManager.itemId == itemManagerChild.itemId)
                    {
                        tempCount++;
                        itemManagerChild.itemAmt++;
                        itemManagerChild.UpdateItemAmount();
                    }
                    if (tempCount > 1)
                        Destroy(inventoryItem);
                }
                DebugLogger.Instance.LogText(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
            }

        }, DebugLogger.Instance.OnPlayfabError);
    }

    public void ClearShop()
    {
        for (int i = 0; i < catalogContent.transform.childCount; ++i)
        {
            Destroy(catalogContent.transform.GetChild(i).gameObject);
        }
    }

    public void ClearPlayerInventory()
    {
        for (int i = 0; i < inventoryContent.transform.childCount; ++i)
        {
            Destroy(inventoryContent.transform.GetChild(i).gameObject);
        }
    }

    private void SetIcon(CatalogItem item)
    {
        switch (item.ItemId)
        {
            case "POW001":
                itemIcon = itemIcons[0];
                break;
            case "POW002":
                itemIcon = itemIcons[1];
                break;
            case "POW003":
                itemIcon = itemIcons[2];
                break;
            case "POW004":
                itemIcon = itemIcons[3];
                break;
            case "WEP001":
                itemIcon = itemIcons[4];
                break;
            case "WEP002":
                itemIcon = itemIcons[5];
                break;
            case "WEP003":
                itemIcon = itemIcons[6];
                break;
            case "WEP004":
                itemIcon = itemIcons[7];
                break;
            default:
                break;
        }
    }

    private void SetIcon(ItemInstance item)
    {
        switch (item.ItemId)
        {
            case "POW001":
                itemIcon = itemIcons[0];
                break;
            case "POW002":
                itemIcon = itemIcons[1];
                break;
            case "POW003":
                itemIcon = itemIcons[2];
                break;
            case "POW004":
                itemIcon = itemIcons[3];
                break;
            case "WEP001":
                itemIcon = itemIcons[4];
                break;
            case "WEP002":
                itemIcon = itemIcons[5];
                break;
            case "WEP003":
                itemIcon = itemIcons[6];
                break;
            case "WEP004":
                itemIcon = itemIcons[7];
                break;
            default:
                break;
        }
    }

}
