using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquippedItemManager : MonoBehaviour
{
    [SerializeField]
    private Transform equippedItemParent;
    [SerializeField]
    private GameObject equippedItemPrefab;
    // Start is called before the first frame update
    void Start()
    {
        UpdateEquippedItems();
    }

    public void UpdateEquippedItems()
    {
        foreach (Item i in PlayerStats.equippedItems)
        {
            GameObject equippedItem = Instantiate(equippedItemPrefab, equippedItemParent);
            equippedItem.transform.GetChild(0).GetComponent<Image>().sprite = i.itemIcon;
        }
    }

    public void ClearEquippedItems()
    {
        for (int i = 0; i < equippedItemParent.childCount; ++i)
        {
            Destroy(equippedItemParent.GetChild(i).gameObject);
        }
    }
}
