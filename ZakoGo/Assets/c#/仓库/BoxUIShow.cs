using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class BoxUIShow : MonoBehaviour
{
    public ItemList MyBox;
    public GunList MyGun;
    public TextMeshProUGUI ContainsNumTxt;
    public TextMeshProUGUI MoneyTxt;

    private void OnEnable()
    {
        UpdateBoxItem();
        MoneyTxt.text = GameNum.Money.ToString();
    }

    public void UpdateBoxItem()
    {
        MyBox.CheckBoxZeroItem();
        for(int i = 0;i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < MyBox.items.Count; i++)
        {
            InsertItem(MyBox.items[i]);
        }
        ContainsNumTxt.text = MyBox.items.Count + "/" + GameNum.BoxCount;
    }
    public void UpdateGun()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < MyGun.gunList.Count; i++)
        {
            InsertGun(MyGun.gunList[i]);
        }
    }
    public BoxGrid boxGrid;
    public BoxGrid boxGrid1;
    public void InsertItem(Item item)
    {
        GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(150, 150);
        gridLayoutGroup.spacing = new Vector2(75, 75);
        gridLayoutGroup.constraintCount = 4;
        BoxGrid grid = Instantiate(boxGrid,gameObject.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.text = item.BoxItemNum.ToString();
        grid.ItemNameTxt.text = item.ItemName.ToString();
        grid.ItemValueTxt.text = item.RealMoney.ToString();
        grid.item = item;
        grid.ChangeItemColor(item.ItemValue);
        

    }
    public void InsertGun(Gun gun)
    {
        GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(400, 200);
        gridLayoutGroup.spacing = new Vector2(75, 0);
        gridLayoutGroup.constraintCount = 2;
        BoxGrid grid = Instantiate(boxGrid1, gameObject.transform);
        grid.ItemImage.sprite = gun.GunImg;
        grid.ItemNumTxt.text = "ÄÍ¾Ã:" + gun.DurabilityNum.ToString();
        grid.gun = gun;

    }
}
