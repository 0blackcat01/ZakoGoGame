using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class NetBagUIShow : NetworkBehaviour
{
    public ItemList MyBag;
    public GunList MyGunList;
    public GameObject GunImg;
    public TextMeshProUGUI ContainsNumTxt;
    private void OnEnable()
    {
        UpdateBagItem();
        UpdateGun();
    }
    public void UpdateGun()
    {
        GunImg.GetComponent<UnityEngine.UI.Image>().color = new Color32(255, 255, 255, 255);
        GunImg.GetComponent<UnityEngine.UI.Image>().sprite = MyGunList.gunList[0].GunImg;
    }
    public void UpdateBagItem()
    {
        Debug.Log(isLocalPlayer);
        if (!isLocalPlayer) return;    
        MyBag.CheckBagZeroItem();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < MyBag.items.Count; i++)
        {
            InsertItem(MyBag.items[i]);
        }
        ContainsNumTxt.text = MyBag.items.Count + "/" + GameNum.BagCount;
    }
    public BoxGrid boxGrid;


    public void InsertItem(Item item)
    {
        BoxGrid grid = Instantiate(boxGrid, gameObject.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.text = item.BagItemNum.ToString();
        grid.ItemNameTxt.text = item.ItemName.ToString();
        grid.ItemValueTxt.text = item.RealMoney.ToString();
        grid.item = item;
        grid.ChangeItemColor(item.ItemValue);
    }
}
