using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class MarketGrid : MonoBehaviour
{
    public UnityEngine.UI.Image gridImage;
    public TextMeshProUGUI gridNum;
    public TextMeshProUGUI gridName;
    public TextMeshProUGUI SalerName;
    public TextMeshProUGUI PriceTxt;
    public int ItemID;
    public int SalerID;
    public TextMeshProUGUI itemInfo;
    public int ItemNum;

    private string Name;
    public ItemList MarketBag;


    public TextMeshProUGUI Txt;
    public Item Find_Item(int id)
    {
        Item item = null;
        for (int i = 0; i < MarketBag.items.Count; i++)
        {
            if (id == MarketBag.items[i].ItemId)
            {
                item = MarketBag.items[i];
            }
        }

        return item;
    }

    public void Buy()
    {
        if(GameNum.Money < int.Parse(PriceTxt.text)) return;
        if (!GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().IsCheckMyItems)
        {
            Name = GameNum.PlayerName;
            GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().PurchaseItem0(Name, ItemID, SalerID, ItemNum);
        }
        else
        {
            Name = GameNum.PlayerName;
            GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().RemoveItem0(ItemID, Name, ItemNum);
        }


    }
    public void TextFly()
    {
        Txt.gameObject.SetActive(false);
    }
}
