using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSeedPlant : MonoBehaviour,IPointerClickHandler
{
    public ItemList MyBox;
    public GameObject Farm;
    private void OnEnable()
    {
        UpdateSeedItems();
    }
    public void UpdateSeedItems()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0;i < MyBox.items.Count; i++)
        {
            if (MyBox.items[i] != null)
            {
                if(MyBox.items[i].itemtype == ItemType.ÖÖ×Ó)
                {
                    InstertItem(MyBox.items[i]);
                }
            }
            
        }
    }
    public BoxGrid boxGrid;
    public void InstertItem(Item item)
    {
        BoxGrid grid = Instantiate(boxGrid, gameObject.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.gameObject.SetActive(true);
        grid.ItemNameTxt.gameObject.SetActive(false);
        grid.ItemNumTxt.text = item.BoxItemNum.ToString();
        grid.item = item;

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerEnter == null) return;
        if (GameNum.FarmIndex == -1) return;
        if (eventData.pointerEnter.CompareTag("FoodGrid"))
        {
            if(eventData.pointerEnter.gameObject.GetComponent<BoxGrid>().item.BoxItemNum >= 0)
            {
                
                eventData.pointerEnter.gameObject.GetComponent<BoxGrid>().item.BoxItemNum -= 1;
                MyBox.CheckBoxZeroItem();
                Farm.GetComponent<FarmManager>().PlantCrop(GameNum.FarmIndex, 
                    eventData.pointerEnter.gameObject.GetComponent<BoxGrid>().item.CropData.cropid);
                UpdateSeedItems();
            }
        }
    }


}
