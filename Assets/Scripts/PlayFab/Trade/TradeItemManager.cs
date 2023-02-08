using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeItemManager : MonoBehaviour
{
    public Item item;
    public TradeManager tradeManager;

    public void AddToOfferEvent()
    {
        tradeManager.AddToOffer(item);
    }

    public void RemoveFromOfferEvent()
    {
        tradeManager.RemoveFromOffer(item);
    }
}
