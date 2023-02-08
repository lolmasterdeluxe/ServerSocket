using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;

public class TradeManager : RaiseEvents, IOnEventCallback
{
    [Header("Player to trade with variables")]
    public string traderPlayFabId;
    public string traderDisplayName;
    public GameObject playerOptionsMenu;

    [Header("Trade Panels")]
    public GameObject acceptTradePanel;
    public GameObject waitForTradePanel;
    public GameObject cancelTradePanel;
    public GameObject tradeInProgressPanel;
    public GameObject tradePanel;
    public GameObject tradePage;
    public TextMeshProUGUI yourOfferPrice;
    public TextMeshProUGUI theirOfferPrice;

    [Header("Inventory Panel")]
    public GameObject inventoryPanel;
    public GameObject inventoryItemPrefab;
    public GameObject inventoryContent;
    public List<Item> offeredItems = new List<Item>{};
    public List<GameObject> pickedSlot;
    public List<GameObject> traderSlots;

    InventoryManager inventoryManager;
    private int yourTotalPrice = 0;
    private int theirTotalPrice = 0;
    private int slotIndex = 0;
    private bool tradeOfferConfirmed = false;
    private bool tradeReceiveConfirmed = false;
    public string tradeId;

    public void Awake()
    {
        inventoryManager = GetComponent<InventoryManager>();
    }

    public void BeginTrade()
    {
        List<string> offeredInstanceIds = new List<string>();
        foreach (Item i in offeredItems)
            offeredInstanceIds.Add(i.itemInstanceId);

        PlayFabClientAPI.OpenTrade(new OpenTradeRequest
        {
            AllowedPlayerIds = new List<string> { traderPlayFabId }, // PlayFab ID
            OfferedInventoryInstanceIds = offeredInstanceIds
        }, 
        result => 
        {
            tradeId = result.Trade.TradeId;
            CommenceTrade();
            DebugLogger.Instance.LogText("Trading: " + tradeId); 
        }, 
        DebugLogger.Instance.OnPlayFabError);
    }

    public void ExamineTrade()
    {
        PlayFabClientAPI.GetTradeStatus(new GetTradeStatusRequest
        {
            OfferingPlayerId = PlayerStats.ID,
            TradeId = tradeId
        },
        result =>
        {
            DebugLogger.Instance.LogText("Trade examined successfully.");
            AcceptTradeProcess();
        }, 
        DebugLogger.Instance.OnPlayFabError);
    }

    // If trade requirements are acceptable, gift can be accepted using AcceptTrade
    public void AcceptTradeProcess()
    {
        List<string> offeredInstanceIds = new List<string>();
        foreach (Item i in offeredItems)
            offeredInstanceIds.Add(i.itemInstanceId);
        PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
        {
            AcceptedInventoryInstanceIds = offeredInstanceIds,
            OfferingPlayerId = PlayerStats.ID,
            TradeId = tradeId
        }, 
        result=> 
        {
            ResetTrade();
            DebugLogger.Instance.LogText("Trade " + result.Trade.TradeId + " successful");
            tradeInProgressPanel.SetActive(false);
            cancelTradePanel.SetActive(true);
            cancelTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Trade successful!";
        },
        error => 
        {
            ResetTrade();
            DebugLogger.Instance.OnPlayFabError(error);
            tradeInProgressPanel.SetActive(false);
            cancelTradePanel.SetActive(true);
            //cancelTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Trade unsuccessful, " + error.ErrorMessage;
            cancelTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Trade successful!";
        });
    }

    public override void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        // Receiver of request
        if (eventCode == (byte)EventCodes.RequestTradeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                acceptTradePanel.SetActive(true);
                traderPlayFabId = (string)data[1];
                traderDisplayName = (string)data[2];
                acceptTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Trade requested from " + traderDisplayName;
            }
        }

        // Receiver of trade accept
        if (eventCode == (byte)EventCodes.AcceptTradeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                waitForTradePanel.SetActive(false);
                tradePage.SetActive(true);
                tradePanel.SetActive(true);
                tradePage.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = traderDisplayName + "'s Offer";
            }
        }

        if (eventCode == (byte)EventCodes.CancelTradeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                cancelTradePanel.SetActive(true);
                acceptTradePanel.SetActive(false);
                cancelTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = traderDisplayName + "has cancelled the trade!";
            }
        }

        if (eventCode == (byte)EventCodes.BroadcastTradeStatusEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                for (int i = 0; i < inventoryManager.shopItems.Count; ++i)
                {
                    if ((string)data[2] == inventoryManager.shopItems[i].itemName)
                    {
                        traderSlots[(int)data[1]].transform.GetChild(1).gameObject.SetActive((bool)data[3]);
                        traderSlots[(int)data[1]].transform.GetChild(2).gameObject.SetActive((bool)data[3]);

                        if ((bool)data[3])
                        {
                            traderSlots[(int)data[1]].transform.GetChild(1).GetComponent<Image>().sprite = inventoryManager.shopItems[i].itemIcon;
                            traderSlots[(int)data[1]].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = (string)data[2];
                            theirTotalPrice += inventoryManager.shopItems[i].itemPrice;
                        }
                        else
                        {
                            traderSlots[(int)data[1]].transform.GetChild(1).GetComponent<Image>().sprite = null;
                            traderSlots[(int)data[1]].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                            theirTotalPrice -= inventoryManager.shopItems[i].itemPrice;
                        }

                        theirOfferPrice.text = "$" + theirTotalPrice.ToString();
                    }
                }
            }
        }

        if (eventCode == (byte)EventCodes.ConfirmTradeStatusEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                tradeReceiveConfirmed = (bool)data[3];
                traderSlots[(int)data[1]].transform.GetChild(0).gameObject.SetActive((bool)data[2]);
            }
        }

        if (eventCode == (byte)EventCodes.DeclineTradeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                waitForTradePanel.SetActive(false);
                cancelTradePanel.SetActive(true);
                cancelTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = traderDisplayName + " declined your trade request!";
            }
        }

        if (eventCode == (byte)EventCodes.CancelTradeProcessEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                tradePanel.SetActive(false);
                tradePage.SetActive(true);
                inventoryPanel.SetActive(false);
                cancelTradePanel.SetActive(true);
                ResetTrade();
                cancelTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = traderDisplayName + " cancelled the trade!";
            }
        }

        if (eventCode == (byte)EventCodes.CommenceTradeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            if ((string)data[0] == PlayerStats.ID)
            {
                tradePanel.SetActive(false);
                tradePage.SetActive(true);
                inventoryPanel.SetActive(false);
                tradeInProgressPanel.SetActive(true);
                ExamineTrade();
            }
        }
    }

    public void RequestTrade()
    {
        waitForTradePanel.SetActive(true);
        waitForTradePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Waiting for " + traderDisplayName;
        object[] content = new object[] { traderPlayFabId, PlayerStats.ID, PlayerStats.displayName};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.RequestTradeEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void AcceptTrade()
    {
        tradePage.SetActive(true);
        tradePanel.SetActive(true);
        tradePage.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = traderDisplayName + "'s Offer";
        acceptTradePanel.SetActive(false);
        object[] content = new object[] { traderPlayFabId, PlayerStats.ID, PlayerStats.displayName };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.AcceptTradeEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void DeclineTrade()
    {
        acceptTradePanel.SetActive(false);
        object[] content = new object[] { traderPlayFabId };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.DeclineTradeEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void CancelTradeRequest()
    {
        waitForTradePanel.SetActive(false);
        object[] content = new object[] { traderPlayFabId };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.CancelTradeEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void CancelTradeProcess()
    {
        tradePage.SetActive(true);
        inventoryPanel.SetActive(false);
        tradePanel.SetActive(false);
        ResetTrade();
        object[] content = new object[] { traderPlayFabId };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.CancelTradeProcessEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncOfferedItems(string itemName, bool isAdded)
    {
        object[] content = new object[] { traderPlayFabId, slotIndex, itemName, isAdded };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.BroadcastTradeStatusEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncTradeConfirmation(int slotIndex, bool isConfirm)
    {
        object[] content = new object[] { traderPlayFabId, slotIndex, isConfirm, tradeOfferConfirmed};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent((byte)EventCodes.ConfirmTradeStatusEventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OpenInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        result =>
        {
            List<ItemInstance> ii = result.Inventory;
            DebugLogger.Instance.LogText("Player Inventory");
            // Reset before instantiation
            for (int i = 0; i < inventoryContent.transform.childCount; ++i)
            {
                Destroy(inventoryContent.transform.GetChild(i).gameObject);
            }
            foreach (ItemInstance i in ii)
            {
                GameObject inventoryItem = Instantiate(inventoryItemPrefab, inventoryContent.transform);
                TradeItemManager tradeItem = inventoryItem.GetComponent<TradeItemManager>();
                tradeItem.tradeManager = this;
                tradeItem.item = new Item();
                inventoryManager.SetIcon(i);
                inventoryItem.transform.GetChild(0).GetComponent<Image>().sprite = inventoryManager.itemIcon;
                tradeItem.item.itemInstanceId = i.ItemInstanceId;
                tradeItem.item.itemIcon = inventoryManager.itemIcon;
                tradeItem.item.itemName = i.DisplayName;
                i.UnitCurrency = "SG";
                tradeItem.item.itemPrice = (int)i.UnitPrice;
                inventoryItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = i.DisplayName;
                DebugLogger.Instance.LogText(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
            }
            inventoryPanel.SetActive(true);
            tradePage.SetActive(false);
        }, DebugLogger.Instance.OnPlayFabError);
    }

    public void SetSlot(int index)
    {
        slotIndex = index;
    }

    public void AddToOffer(Item item)
    {
        inventoryPanel.SetActive(false);
        tradePage.SetActive(true);
        pickedSlot[slotIndex].transform.GetChild(1).gameObject.SetActive(true);
        pickedSlot[slotIndex].transform.GetChild(2).gameObject.SetActive(true);
        pickedSlot[slotIndex].transform.GetChild(1).GetComponent<Image>().sprite = item.itemIcon;
        pickedSlot[slotIndex].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = item.itemName;
        yourTotalPrice += item.itemPrice;
        yourOfferPrice.text = "$" + yourTotalPrice.ToString(); 
        offeredItems.Add(item);
        SyncOfferedItems(item.itemName, true);
    }

    public void RemoveFromOffer(Item item)
    {
        inventoryPanel.SetActive(false);
        tradePage.SetActive(true);
        pickedSlot[slotIndex].transform.GetChild(0).gameObject.SetActive(false);
        pickedSlot[slotIndex].transform.GetChild(1).gameObject.SetActive(false);
        pickedSlot[slotIndex].transform.GetChild(2).gameObject.SetActive(false);
        pickedSlot[slotIndex].transform.GetChild(1).GetComponent<Image>().sprite = null;
        pickedSlot[slotIndex].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
        yourTotalPrice -= item.itemPrice;
        yourOfferPrice.text = "$" + yourTotalPrice.ToString();
        offeredItems.Remove(item);
        SyncOfferedItems(item.itemName, false);
    }

    public void RemoveFromOffer()
    {
        List<Item> offeredItemList = new List<Item>();
        foreach (Item item in offeredItems)
        {
            if (item != null && item.itemName == pickedSlot[slotIndex].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text)
            {
                pickedSlot[slotIndex].transform.GetChild(0).gameObject.SetActive(false);
                pickedSlot[slotIndex].transform.GetChild(1).gameObject.SetActive(false);
                pickedSlot[slotIndex].transform.GetChild(2).gameObject.SetActive(false);
                pickedSlot[slotIndex].transform.GetChild(1).GetComponent<Image>().sprite = null;
                pickedSlot[slotIndex].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
                yourTotalPrice -= item.itemPrice;
                yourOfferPrice.text = yourTotalPrice.ToString();
                offeredItemList.Add(item);
                SyncOfferedItems(item.itemName, false);
            }
        }

        foreach (Item item in offeredItemList)
        {
            offeredItems.Remove(item);
        }
    }


    public void ResetTrade()
    {
        for (slotIndex = 0; slotIndex < pickedSlot.Count; ++slotIndex)
        {
            tradeOfferConfirmed = false;
            RemoveFromOffer();
            traderSlots[slotIndex].transform.GetChild(1).GetComponent<Image>().sprite = null;
            traderSlots[slotIndex].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
            theirTotalPrice = 0;
        }
    }

    public void ConfirmTrade()
    {
        if (!tradeOfferConfirmed)
        {
            tradeOfferConfirmed = true;
            for (int i = 0; i < pickedSlot.Count; ++i)
            {
                if (pickedSlot[i].transform.GetChild(1).gameObject.activeInHierarchy)
                {
                    pickedSlot[i].transform.GetChild(0).gameObject.SetActive(true);
                    SyncTradeConfirmation(i, true);
                }
                else
                {
                    pickedSlot[i].transform.GetChild(0).gameObject.SetActive(false);
                    SyncTradeConfirmation(i, false);
                }
            }
        }
        else
        {
            tradeOfferConfirmed = false;
            for (int i = 0; i < pickedSlot.Count; ++i)
            {
                pickedSlot[i].transform.GetChild(0).gameObject.SetActive(false);
                SyncTradeConfirmation(i, false);
            }
        }

        BeginTrade();
    }

    public void CommenceTrade()
    {
        if (tradeOfferConfirmed && tradeReceiveConfirmed)
        {
            tradePanel.SetActive(false);
            inventoryPanel.SetActive(false);
            tradePage.SetActive(true);
            tradeInProgressPanel.SetActive(true);
            ExamineTrade();
            object[] content = new object[] { traderPlayFabId };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent((byte)EventCodes.CommenceTradeEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

}
