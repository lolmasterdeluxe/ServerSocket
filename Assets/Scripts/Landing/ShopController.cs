using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopController : MonoBehaviour
{
    public GameObject shopPanel;
    public GameObject inventoryPanel;
    System.Action playerCallback;
    EquippedItemManager equippedItemManager;
    InventoryManager inventoryManager;

    // Start is called before the first frame update
    void Start()
    {
        shopPanel.SetActive(false);
        equippedItemManager = FindObjectOfType<EquippedItemManager>();
        inventoryManager = FindObjectOfType<InventoryManager>();
    }

    public void OpenPanel(System.Action callBack = null)
    {
        shopPanel.SetActive(true);
        equippedItemManager.ClearEquippedItems();
        inventoryManager.GetCatalog();
        inventoryManager.GetVirtualCurrencies();
        playerCallback = callBack;
    }

    public void OpenInventory(TextMeshProUGUI moneyText)
    {
        inventoryPanel.SetActive(true);
        shopPanel.SetActive(false);
        inventoryManager.GetVirtualCurrencies(moneyText);
        inventoryManager.GetPlayerInventory();
    }

    public void OpenShop(TextMeshProUGUI moneyText)
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(true);
        inventoryManager.GetVirtualCurrencies(moneyText);
        inventoryManager.ClearPlayerInventory();
    }

    public void ClosePanel()
    {
        shopPanel.SetActive(false);
        inventoryPanel.SetActive(false);
        inventoryManager.ClearShop();
        inventoryManager.ClearPlayerInventory();
        equippedItemManager.UpdateEquippedItems();
        playerCallback();
    }
}
