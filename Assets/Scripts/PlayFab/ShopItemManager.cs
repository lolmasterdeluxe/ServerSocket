using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class ShopItemManager : MonoBehaviour
{
    [SerializeField]
    private string catalogVersion, virtualCurrency;
    private InventoryManager inventoryManager;

    public string itemId;

    [SerializeField]
    private uint price;
    [SerializeField]
    private TextMeshProUGUI itemName, itemType, itemPrice;
    [SerializeField]
    private Image itemIcon;

    private void Awake()
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

    public void Confirmation()
    {
        inventoryManager.ConfirmPurchase(itemName.text, catalogVersion, itemId, virtualCurrency, (int)price);
    }
}
