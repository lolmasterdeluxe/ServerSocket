using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [Header("Text GUI")]
    [SerializeField]
    private TextMeshProUGUI inventoryText;
    [SerializeField]
    private TextMeshProUGUI shopMoneyText;

    [Header("Shop & Inventory Prefabs")]
    [SerializeField]
    private GameObject catalogContent;
    [SerializeField]
    private GameObject inventoryContent;
    [SerializeField]
    private GameObject shopItemPrefab;
    [SerializeField]
    private GameObject inventoryItemPrefab;
    [SerializeField]
    private GameObject confirmationPanel;
    [SerializeField]
    private GameObject outcomePanel;

    [Header("Icons")]
    [SerializeField]
    public List<Sprite> itemIcons;
    public Sprite itemIcon;

    [Header("Items for sale in shop")]
    public List<Item> shopItems;
    private string catalogVersion, itemId, virtualCurrency;
    private int price;

    #region Getting Virtual Currencies
    public void GetVirtualCurrencies(TextMeshProUGUI moneyText = null)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        result =>
        {
            int coins = result.VirtualCurrency["SG"];
            DebugLogger.Instance.LogText("Coins: " + coins);

            if (moneyText == null)
                shopMoneyText.text = coins.ToString();
            else
                moneyText.text = coins.ToString();

        }, DebugLogger.Instance.OnPlayFabError);
    }

    public void AddVirtualCurrencies(int amount)
    {
        PlayFabClientAPI.AddUserVirtualCurrency(new AddUserVirtualCurrencyRequest
        {
            VirtualCurrency = "SG",
            Amount = amount,
        },
        result =>
        {
            DebugLogger.Instance.LogText("Coins added: " + result.BalanceChange);
            DebugLogger.Instance.LogText("Coins left: " + result.Balance);
        }, DebugLogger.Instance.OnPlayFabError);
    }
    #endregion

    #region Catalog Items & Player Inventory
    public void GetCatalog()
    {
        for (int i = 0; i < catalogContent.transform.childCount; ++i)
        {
            if (catalogContent.transform.GetChild(i).gameObject != null)
                Destroy(catalogContent.transform.GetChild(i).gameObject);
        }
        PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest
        {
            CatalogVersion = "1.0" //update catalog names
        },
        result =>
        {
            List<CatalogItem> items = result.Catalog;
            DebugLogger.Instance.LogText("Catalog Items");
            foreach (CatalogItem i in items)
            {
                GameObject catalogItem = Instantiate(shopItemPrefab, catalogContent.transform);
                catalogItem.GetComponent<ShopItemManager>().SetItemStats(i.CatalogVersion, i.ItemId, "SG", i.VirtualCurrencyPrices["SG"]);
                SetIcon(i);
                catalogItem.GetComponent<ShopItemManager>().SetItemVisuals(i.DisplayName, i.ItemClass, itemIcon);
                catalogItem.GetComponent<ShopItemManager>().SetItemPrice();
                DebugLogger.Instance.LogText(i.DisplayName + ", " + i.VirtualCurrencyPrices["SG"]);
            }
        }, DebugLogger.Instance.OnPlayFabError);
    }

    public void GetPlayerInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        result =>
        {
            List<ItemInstance> ii = result.Inventory;
            DebugLogger.Instance.LogText("Player Inventory");
            inventoryText.text = "";
            foreach (ItemInstance i in ii)
            {
                GameObject inventoryItem = Instantiate(inventoryItemPrefab, inventoryContent.transform);
                SetIcon(i);
                InventoryItemManager inventoryItemManager = inventoryItem.GetComponent<InventoryItemManager>();
                inventoryItemManager.itemId = i.ItemId;
                inventoryItemManager.itemInstanceId = i.ItemInstanceId;
                inventoryItemManager.SetItemVisuals(i.DisplayName, i.ItemClass, itemIcon);

                int tempCount = 0;
                for (int k = 0; k < inventoryContent.transform.childCount; ++k)
                {
                    InventoryItemManager itemManagerChild = inventoryContent.transform.GetChild(k).GetComponent<InventoryItemManager>();
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

        }, DebugLogger.Instance.OnPlayFabError);
    }

    #endregion

    #region Purchasing Item

    public void ConfirmPurchase(string itemName, string catalogVersion, string itemId, string virtualCurrency, int price)
    {
        this.catalogVersion = catalogVersion;
        this.itemId = itemId;
        this.virtualCurrency = virtualCurrency;
        this.price = price;
        confirmationPanel.SetActive(true);
        confirmationPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Confirm purchase of <color=yellow>" + itemName +
            "?</color>";
    }

    public void BuyItemRequest()
    {
        PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
        {
            CatalogVersion = catalogVersion,
            ItemId = itemId,
            VirtualCurrency = virtualCurrency,
            Price = price
        },
        result =>
        {
            DebugLogger.Instance.LogText("Bought!");
            GetVirtualCurrencies();
            outcomePanel.SetActive(true);
            outcomePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Purchase successful!";
        },
        error =>
        {
            DebugLogger.Instance.LogText(error.ErrorMessage);
            outcomePanel.SetActive(true);
            outcomePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Error in purchase: <color=red>" + error.ErrorMessage + "</color>";
        });
    }

    public void ConsumeItemRequest(string itemInstanceId)
    {
        PlayFabClientAPI.ConsumeItem(new ConsumeItemRequest
        {
            ConsumeCount = 1,
            ItemInstanceId = itemInstanceId
        },
        result => {
            DebugLogger.Instance.LogText("Item " + itemInstanceId + " consumed!");
        }, DebugLogger.Instance.OnPlayFabError);
    }
    #endregion

    #region Cleaning

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
    #endregion

    #region Icon Misc.

    public void SetIcon(CatalogItem item)
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

    public void SetIcon(ItemInstance item)
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
    #endregion
}
