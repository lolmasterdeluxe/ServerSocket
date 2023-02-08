using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerOptionsManager : MonoBehaviour
{
    public string playFabId;
    public Player player;
    public TradeManager tradeManager;
    public FriendManager friendManager;
    // Start is called before the first frame update
    void Start()
    {
        tradeManager = FindObjectOfType<TradeManager>();
        friendManager = FindObjectOfType<FriendManager>();
        player = GetComponent<Player>();
    }

    public void OpenOptionsMenu()
    {
        tradeManager.playerOptionsMenu.SetActive(true);
        tradeManager.playerOptionsMenu.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = player.displayName + "'s Options";
        tradeManager.traderPlayFabId = playFabId;
        tradeManager.traderDisplayName = player.displayName;
        friendManager.recipientDisplayName = player.displayName;
        friendManager.recipientID = playFabId;
    }
}
