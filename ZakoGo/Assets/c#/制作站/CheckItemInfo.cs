using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheckItemInfo : MonoBehaviour, IPointerClickHandler
{
    public GameObject InfoConten;
    public int ListIndex;
    public TextMeshProUGUI InfoTipTxt;
    private void OnEnable()
    {
        InfoTipTxt.text = "";
        for (int i = 0; i < InfoConten.transform.childCount; i++)
        {
            Destroy(InfoConten.transform.GetChild(i).gameObject);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null) return;
        if (eventData.pointerEnter.gameObject ==  null) return;
        GameObject obj = eventData.pointerEnter.gameObject;
        if (obj.CompareTag("MakingGrid"))
        {
            ResetColor();
            obj.GetComponent<Image>().color = Color.red;
            UpdateList(obj.GetComponent<MakingGrid>().MakingRecipe0);
            if(obj.GetComponent<MakingGrid>().MakingRecipe0.gun != null)
            {
                InfoTipTxt.text = obj.GetComponent<MakingGrid>().MakingRecipe0.gun.GunInfo;
            }
            else
            {
                InfoTipTxt.text = obj.GetComponent<MakingGrid>().MakingRecipe0.item.ItemInfo;
            }
            
            ListIndex = obj.GetComponent<MakingGrid>().index;
        }
    }
    public void UpdateList(MakingRecipe recipe)
    {
        for (int i = 0; i < InfoConten.transform.childCount; i++)
        {
            Destroy(InfoConten.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < recipe.items.Count; i++)
        {
            InsertItem(recipe.items[i], recipe.amount[i]);
        }
    }
    public MakingGrid makingGrid;
    public void InsertItem(Item item,int num)
    {
        MakingGrid grid = Instantiate(makingGrid, InfoConten.transform);
        grid.Img.sprite = item.ItemImg;
        grid.NumTxt.text = "*" + num.ToString();

    }
    public GameObject UIConten;
    public void ResetColor()
    {
        for(int i = 0; i < UIConten.transform.childCount; i++)
        {
            UIConten.transform.GetChild(i).gameObject.GetComponent<Image>().color = Color.white;
        }
      

    }
}
