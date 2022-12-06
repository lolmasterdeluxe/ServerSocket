using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    [SerializeField]
    private string catalogVersion, virtualCurrency;
    private InventoryManager inventoryManager;

    public string itemId;
    public int itemAmt;

    [SerializeField]
    private uint price;
    [SerializeField]
    private TextMeshProUGUI itemName, itemType, itemPrice, itemAmount;
    [SerializeField]
    private Image itemIcon;

    public void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void SetItemStats(string catalogVersion, string itemId, string virtualCurrency, uint price)
    {
        this.catalogVersion = catalogVersion;
        this.itemId = itemId;
        this.virtualCurrency = virtualCurrency;
        this.price = price;
    }

    public void SetItemVisuals(string itemName, string itemType, Sprite itemIcon)
    {
        this.itemName.text = itemName;
        this.itemType.text = itemType;
        this.itemIcon.sprite = itemIcon;
    }

    public void SetItemPrice()
    {
        itemPrice.text = "$" + price.ToString();
    }

    public void UpdateItemAmount()
    {
        itemAmount.text = itemAmt.ToString();
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
            result => 
            { 
                DebugLogger.Instance.LogText("Bought!");
                inventoryManager.GetVirtualCurrencies();
            },
            DebugLogger.Instance.OnPlayfabError);
    }
}
