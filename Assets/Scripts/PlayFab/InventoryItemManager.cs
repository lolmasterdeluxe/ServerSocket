using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class InventoryItemManager : MonoBehaviour
{
    private InventoryManager inventoryManager;

    public string itemId;
    public string itemInstanceId;
    public int itemAmt;

    [SerializeField]
    private TextMeshProUGUI itemName, itemType, itemAmount;
    [SerializeField]
    private Image itemIcon, itemEquippedCheckbox;
    [SerializeField]
    private Sprite ticked, unticked;
    public Item item;
    [SerializeField]
    private bool isEquipped = false;

    private void Awake()
    {
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    private void Start()
    {
        SetItem();
    }

    public void SetItemVisuals(string itemName, string itemType, Sprite itemIcon)
    {
        this.itemName.text = itemName;
        this.itemType.text = itemType;
        this.itemIcon.sprite = itemIcon;
    }

    public void ToggleItemEquip()
    {
        if (isEquipped)
        {
            PlayerStats.equippedItems.Remove(item);
            itemEquippedCheckbox.sprite = unticked;
            isEquipped = false;
        }
        else
        {
            PlayerStats.equippedItems.Add(item);
            itemEquippedCheckbox.sprite = ticked;
            isEquipped = true;
        }
    }

    public void UpdateItemAmount()
    {
        itemAmount.text = itemAmt.ToString();
    }

    private void SetItem()
    {
        foreach (Item shopItem in inventoryManager.shopItems)
        {
            if (itemName.text == shopItem.itemName)
            {
                item = shopItem;
                item.itemInstanceId = itemInstanceId;
            }
        }

        foreach (Item inventoryItem in PlayerStats.equippedItems)
        {
            if (itemName.text == inventoryItem.itemName)
            {
                itemEquippedCheckbox.sprite = ticked;
                isEquipped = true;
            }
        }
    }
}
