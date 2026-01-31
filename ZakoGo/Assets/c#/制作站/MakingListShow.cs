using Mirror.BouncyCastle.Bcpg.OpenPgp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MakingListShow : MonoBehaviour
{
    public MakingRecipeList MakingGunList;
    public MakingRecipeList MakingBulletList;
    public MakingRecipeList MakingDecorationList;
    public MakingRecipeList MakingClothList;

    private MakingRecipeList MakingList;
    private int PageIndex = 0;
    private void OnEnable()
    {
        OpenGunlist();
    }

    public void UpdateList()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < MakingList.MakingRecipes.Count; i++)
        {
            InsertItem(MakingList.MakingRecipes[i],i);
        }
    }
    public void OpenGunlist()
    {
        PageIndex = 0;
        GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(400,200);
        gridLayoutGroup.spacing = new Vector2(75,0);
        gridLayoutGroup.constraintCount = 2;
        MakingList = MakingGunList;
        makingGrid = makingGunGrid;
        UpdateList();
    }
    public void OpenBulletList()
    {
        PageIndex = 1;
        GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(150, 150);
        gridLayoutGroup.spacing = new Vector2(75, 75);
        gridLayoutGroup.constraintCount = 4;
        MakingList = MakingBulletList;
        makingGrid = makingOtherGrid;
        UpdateList();
    }
    public void OpenDecorationList()
    {
        PageIndex = 2;
        GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(150, 150);
        gridLayoutGroup.spacing = new Vector2(75, 75);
        gridLayoutGroup.constraintCount = 4;
        MakingList = MakingDecorationList;
        makingGrid = makingOtherGrid;
        UpdateList();
    }
    public void OpenClothList()
    {
        PageIndex = 3;
        GridLayoutGroup gridLayoutGroup = gameObject.GetComponent<GridLayoutGroup>();
        gridLayoutGroup.cellSize = new Vector2(150, 150);
        gridLayoutGroup.spacing = new Vector2(75, 75);
        gridLayoutGroup.constraintCount = 4;
        MakingList = MakingClothList;
        makingGrid = makingOtherGrid;
        UpdateList();
    }
    public MakingGrid makingGunGrid;
    public MakingGrid makingOtherGrid;
    private MakingGrid makingGrid;
    public void InsertItem(MakingRecipe recipe,int index)
    {
        MakingGrid grid = Instantiate(makingGrid, gameObject.transform);
        grid.index = index;      
        if(recipe.item != null)
        {
            grid.item = recipe.item;
            if (recipe.item.itemtype == ItemType.子弹)
            {
                grid.NumTxt.text = "60";
            }
            grid.Img.sprite = recipe.item.ItemImg;
        }
        else
        {
            grid.gun = recipe.gun;
            grid.Img.sprite = recipe.gun.GunImg;
        }
        
        grid.MakingRecipe0 = recipe;
        
    }
    public TextMeshProUGUI Tips;
    public GunList MyGunList;
    public ItemList MyBox;
    public void MakingItems()
    {
        int index = gameObject.GetComponent<CheckItemInfo>().ListIndex;
        bool IsSuccess = true;
        if (MyBox.items.Count >= GameNum.BoxCount)
        {
            Tips.gameObject.SetActive(true);
            Invoke("StopTips", 2f);
            Tips.text = "仓库已满";
            Tips.color = Color.white;
            IsSuccess = false;
            return;
        }
        if (PageIndex == 0)
        {
            if (MyGunList.gunList.Contains(MakingList.MakingRecipes[index].gun))
            {
                Tips.gameObject.SetActive(true);
                Invoke("StopTips", 2f);
                Tips.text = "已拥有该枪械";
                Tips.color = Color.white;
                IsSuccess = false;
                return;
            }

        }
        else if(PageIndex == 1)
        {

        }
        else if (PageIndex == 2)
        {

            if (MyBox.items.Contains(MakingList.MakingRecipes[index].item))
            {
                Tips.gameObject.SetActive(true);
                Invoke("StopTips", 2f);
                Tips.text = "已拥有该道具";
                Tips.color = Color.white;
                IsSuccess = false;
                return;
            }
        }
        else if (PageIndex == 3)
        {
            if (MakingList.MakingRecipes[index].item.itemtype == ItemType.衣物)
            {
                if (MyBox.items.Contains(MakingList.MakingRecipes[index].item))
                {
                    Tips.gameObject.SetActive(true);
                    Invoke("StopTips", 2f);
                    Tips.text = "已拥有该道具";
                    Tips.color = Color.white;
                    IsSuccess = false;
                    return;
                }
            }

        }

        if (MakingList.MakingRecipes[index].NeedMoney < GameNum.Money)
        {
            Tips.gameObject.SetActive(true);
            Invoke("StopTips", 2f);
            Tips.text = "青辉石不足";
            Tips.color = Color.white;
            IsSuccess = false;
            return;
        }
        for (int i = 0; i < MakingList.MakingRecipes[index].items.Count; i++)
        {

            if (MakingList.MakingRecipes[index].items[i].BoxItemNum < MakingList.MakingRecipes[index].amount[i])
            {
                Tips.gameObject.SetActive(true);
                Invoke("StopTips", 2f);
                Tips.text = "材料不足";
                Tips.color = Color.white;
                IsSuccess = false;
                break;
            }
            else
            {
                MakingList.MakingRecipes[index].items[i].BoxItemNum -= MakingList.MakingRecipes[index].amount[i];
            }
        }
        if (IsSuccess)
        {
            Tips.gameObject.SetActive(true);
            Invoke("StopTips", 2f);
            Tips.text = "制作成功！";
            Tips.color = Color.green;
            if(PageIndex == 0)
            {
                AddGun(MakingList.MakingRecipes[index].gun);
            }
            else if(PageIndex == 1)
            {
                AddItem(MakingList.MakingRecipes[index].item, 60);
            }
            else if(PageIndex == 2)
            {
                AddItem(MakingList.MakingRecipes[index].item, 1);
            }
            else if(PageIndex == 3)
            {
                AddItem(MakingList.MakingRecipes[index].item, 1);
            }
            MyBox.CheckBoxZeroItem();
        }
    }
    public void AddItem(Item item, int ItemNum)
    {
        if (item.IsStack)
        {
            if (!MyBox.items.Contains(item))
            {
                MyBox.items.Add(item);
            }
        }
        else
        {
            MyBox.items.Add(item);
        }
        item.BoxItemNum += ItemNum;

    }
    public void AddGun(Gun gun)
    {
        MyGunList.gunList.Add(gun);
    }
    public void StopTips()
    {
        Tips.gameObject.SetActive(false);
    }
}
