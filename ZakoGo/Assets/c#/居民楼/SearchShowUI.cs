using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Unity.VisualScripting;


public class SearchShowUI : NetworkBehaviour
{
    public TolResourcePoint tolResourcePoint;

    public void UpdateBoxItem(ItemList SearchBox)
    {
        //Debug.Log(isClient);
        //if (!isClient) return;

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < SearchBox.items.Count; i++)
        {
            InsertItem(SearchBox.items[i],i);
        }
    }
    public BoxGrid boxGrid;
    public void InsertItem(Item item,int index)
    {
        BoxGrid grid = Instantiate(boxGrid, gameObject.transform);
        grid.index = index;
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.text = "1";      
        grid.ItemNameTxt.text = item.ItemName.ToString();
        grid.ItemValueTxt.text = item.RealMoney.ToString();
        grid.item = item;
        grid.ChangeItemColor(item.ItemValue);

    }

}
