using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnhanceTable : MonoBehaviour
{
    public Recipe Box_recipe;
    public Recipe Making_recipe;
    public Recipe Kitchen_recipe;
    public Recipe Vegetable_recipe;
    private Recipe recipe;

    public TextMeshProUGUI Boxlv;
    public TextMeshProUGUI Makinglv;
    public TextMeshProUGUI Kitchenlv;
    public TextMeshProUGUI Vegetablelv;
    public TextMeshProUGUI NeedTip;
    public TextMeshProUGUI Tips;

    public TextMeshProUGUI MakingDurabilityTxt;
    public TextMeshProUGUI KitchenDurabilityTxt;
    public TextMeshProUGUI EnhanceDurabilityTxt;


    public int EnhanceIndex = 0;
    public ItemList MyBox;
    private void OnEnable()
    {
        ClickLvUp();
        EnhanceIndex = 0;
        Boxlv.text = "lv." + BuildingLevelUp.BoxLevel;
        Makinglv.text = "lv." + BuildingLevelUp.MakingLevel;
        Kitchenlv.text = "lv." + BuildingLevelUp.KitchenLevel;
        Vegetablelv.text = "lv." + Vegetable_recipe.level;
        MakingDurabilityTxt.text = BuildingLevelUp.MakingDurability + "/" + "100";
        KitchenDurabilityTxt.text = BuildingLevelUp.KitchenDurability + "/" + "100";
        EnhanceDurabilityTxt.text = BuildingLevelUp.EnhanceDurability + "/" + "100";
    }
    public void ShowBoxEnhanceNeed()
    {

        EnhanceIndex = 1;
        NeedTip.text = "";
        if (IsFix) return;
        for (int i = 0; i < Box_recipe.ingredients[Box_recipe.level - 1].item.Count; i++)
        {
            NeedTip.text += Box_recipe.ingredients[Box_recipe.level - 1].item[i].ItemName + "*" +
                Box_recipe.ingredients[Box_recipe.level - 1].amount[i] + " ";
        }
        NeedTip.text += ("青辉石" + Box_recipe.ingredients[Box_recipe.level - 1].NeedMoney);
    }
    public void ShowMakingEnhanceNeed()
    {

        EnhanceIndex = 2;
        NeedTip.text = "";
        if (IsFix) return;
        for (int i = 0; i < Making_recipe.ingredients[Making_recipe.level - 1].item.Count; i++)
        {
            NeedTip.text += Making_recipe.ingredients[Making_recipe.level - 1].item[i].ItemName + "*" +
                Making_recipe.ingredients[Making_recipe.level - 1].amount[i] + " ";
        }
        NeedTip.text += ("青辉石" + Making_recipe.ingredients[Making_recipe.level - 1].NeedMoney);
    }
    public void ShowTrashEnhanceNeed()
    {
        EnhanceIndex = 3;
        NeedTip.text = "";
        if (IsFix) return;
        for (int i = 0; i < Kitchen_recipe.ingredients[Kitchen_recipe.level - 1].item.Count; i++)
        {
            NeedTip.text += Kitchen_recipe.ingredients[Kitchen_recipe.level - 1].item[i].ItemName + "*" +
                Kitchen_recipe.ingredients[Kitchen_recipe.level - 1].amount[i] + " ";
        }
        NeedTip.text += ("青辉石" + Kitchen_recipe.ingredients[Kitchen_recipe.level - 1].NeedMoney);
    }
    public void ShowVegeEnhanceNeed()
    {
        EnhanceIndex = 4;
        NeedTip.text = "";
        if (IsFix) return;
        for (int i = 0; i < Vegetable_recipe.ingredients[Vegetable_recipe.level - 1].item.Count; i++)
        {
            NeedTip.text += Vegetable_recipe.ingredients[Vegetable_recipe.level - 1].item[i].ItemName + "*" +
                Vegetable_recipe.ingredients[Vegetable_recipe.level - 1].amount[i] + " ";
        }
        NeedTip.text += ("青辉石" + Vegetable_recipe.ingredients[Vegetable_recipe.level - 1].NeedMoney);
    }
    public void ShowEnhanceNeed()
    {
        EnhanceIndex = 5;
        NeedTip.text = "";
        if (IsFix) return;
    }
    public void Enhance()
    {
        
        if(EnhanceIndex == 1)
        {
            recipe = Box_recipe;
        }
        else if(EnhanceIndex == 2)
        {
            recipe = Making_recipe;
        }
        else if(EnhanceIndex == 3)
        {
            recipe = Kitchen_recipe;
        }
        else if(EnhanceIndex == 4)
        {
            recipe = Vegetable_recipe;
        }
        if (Box_recipe.level > Box_recipe.ingredients.Count) return;
        bool IsSuccess = true;
        for (int i = 0; i < Box_recipe.ingredients[Box_recipe.level - 1].item.Count; i++)
        {
            if (recipe.ingredients[recipe.level - 1].NeedMoney < GameNum.Money)
            {
                Tips.gameObject.SetActive(true);
                Invoke("StopTips", 2f);
                Tips.text = "青辉石不足";
                Tips.color = Color.white;
                IsSuccess = false;
                break;
            }
            if (recipe.ingredients[recipe.level - 1].item[i].BoxItemNum < recipe.ingredients[recipe.level - 1].amount[i])
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
                recipe.ingredients[recipe.level - 1].item[i].BoxItemNum -= recipe.ingredients[recipe.level - 1].amount[i];
            }
        }
        if (IsSuccess)
        {
            if (EnhanceIndex == 1)
            {
                SuccussBox();
            }
            else if (EnhanceIndex == 2)
            {
                recipe = Making_recipe;
            }
            else if (EnhanceIndex == 3)
            {
                recipe = Kitchen_recipe;
            }
            else if (EnhanceIndex == 4)
            {
                recipe = Vegetable_recipe;
            }
        }



    }
    public void StopTips()
    {
        Tips.gameObject.SetActive(false);
    }
    public void ExpendItems()
    {
        for (int i = 0; i < Box_recipe.ingredients[Box_recipe.level - 1].item.Count; i++)
        {
            Box_recipe.ingredients[Box_recipe.level - 1].item[i].BoxItemNum -= Box_recipe.ingredients[Box_recipe.level - 1].amount[i];
        }
        GameObject.FindGameObjectWithTag("FindButton").GetComponent<CreatName>().CheckMyMoeny0();
    }
    public void SuccussBox()
    {
        Tips.gameObject.SetActive(true);
        Invoke("StopTips", 2f);
        Tips.text = "升级成功！";
        Tips.color = Color.green;
        Box_recipe.level += 1;
        ExpendItems();
        if (Box_recipe.level == 2)
        {
            GameNum.BoxCount = 13;
        }
        else if (Box_recipe.level == 3)
        {
            GameNum.BoxCount = 15;
        }
        else if (Box_recipe.level == 4)
        {
            GameNum.BoxCount = 20;
        }
        else if (Box_recipe.level == 5)
        {
            GameNum.BoxCount = 23;
        }
        else if (Box_recipe.level == 6)
        {
            GameNum.BoxCount = 25;
        }
        else if (Box_recipe.level == 7)
        {
            GameNum.BoxCount = 28;
        }
        MyBox.CheckBoxZeroItem();
    }
    #region 修复
    [Header("修复相关")]
    public TextMeshProUGUI Txt;
    public TextMeshProUGUI LvTxt;
    private bool IsFix = false;
    public GameObject FixedUI;
    public Item Wood;
    public Item Stone;
    public TMP_InputField InputWood;
    public TMP_InputField InputStone;


    public void ClickFix()
    {
        Txt.text = "修复";
        IsFix = true;
    }
    public void ClickLvUp()
    {
        Txt.text = "升级";
        IsFix = false;
    }
    public void SubmitButtonEvent()
    {
        if (IsFix)
        {
            FixedUI.gameObject.SetActive(true);
            LvTxt.text = "";
        }
        else
        {
            Enhance();
        }
    }
    public void FixedUp()
    {
        EndEditWood();
        EndEditStone();
        Wood.BoxItemNum -= int.Parse(InputWood.text);
        Stone.BoxItemNum -= int.Parse(InputStone.text);
        if (EnhanceIndex == 2)
        {
            BuildingLevelUp.MakingDurability += int.Parse(InputWood.text) * 2 + int.Parse(InputStone.text) * 4;
        }
        else if (EnhanceIndex == 3)
        {
            BuildingLevelUp.KitchenDurability += int.Parse(InputWood.text) * 2 + int.Parse(InputStone.text) * 4;
        }
        else if (EnhanceIndex == 5)
        {
            BuildingLevelUp.EnhanceDurability += int.Parse(InputWood.text) * 2 + int.Parse(InputStone.text) * 4;
        }
        MakingDurabilityTxt.text = BuildingLevelUp.MakingDurability + "/" + "100";
        KitchenDurabilityTxt.text = BuildingLevelUp.KitchenDurability + "/" + "100";
        EnhanceDurabilityTxt.text = BuildingLevelUp.EnhanceDurability + "/" + "100";
        InputWood.text = "0";
        InputStone.text = "0";
        FixedUI.gameObject.SetActive(false);
    }
    public void EndEditWood()
    {
        if(int.Parse(InputWood.text) < 0)
        {
            InputWood.text = "0";
        }
        if(int.Parse(InputWood.text) > Wood.BoxItemNum)
        {
            InputWood.text = Wood.BoxItemNum.ToString();
        }
        LvTxt.text = "获得" + (int.Parse(InputWood.text) * 2 + int.Parse(InputStone.text) * 4) + "耐久";
    }
    public void EndEditStone()
    {
        if (int.Parse(InputStone.text) < 0)
        {
            InputStone.text = "0";
        }
        if (int.Parse(InputStone.text) > Stone.BoxItemNum)
        {
            InputStone.text = Stone.BoxItemNum.ToString();
        }
        LvTxt.text = "获得" + (int.Parse(InputWood.text) * 2 + int.Parse(InputStone.text) * 4) + "耐久";
    }
    #endregion
}
