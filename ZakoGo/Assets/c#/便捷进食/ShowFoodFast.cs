using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowFoodFast : MonoBehaviour
{
    public ItemList MyBag;
    public GameObject Conten;


    private void OnEnable()
    {
        UpdateBagItem();
    }

    public void UpdateBagItem()
    {
        for (int i = 0; i < Conten.transform.childCount; i++)
        {
            Destroy(Conten.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < MyBag.items.Count; i++)
        {
            if(MyBag.items[i].itemtype == ItemType.Ê³Îï)
            {
                InsertItem(MyBag.items[i]);
            }
            
        }
        
    }
    public BoxGrid boxGrid;

    public void InsertItem(Item item)
    {
        BoxGrid grid = Instantiate(boxGrid, Conten.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.text = item.BagItemNum.ToString();
        grid.item = item;

    }
}
