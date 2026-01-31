using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class KitchenData
{
    public List<CookingSlot> cookingSlots = new List<CookingSlot>();
}

[System.Serializable]
public class CookingSlot
{
    public int recipeId = -1; // 当前烹饪的配方ID
    public DateTime startTime; // 开始烹饪时间
    public bool isActive = false; // 是否正在使用
    public bool isHarvest = false;//是否可以收获
    public KitchenRecipe recipe;


}
public class ShowKitchenGrid : MonoBehaviour,IPointerClickHandler
{
    public KitchenData kitchenData;
    public Recipe KitchenLv;
    public List<KitchenRecipe> kitchenRecipes;
    public ItemList MyBox;
    public GameObject conten;
    public TextMeshProUGUI MakeFoodTips;
    public TextMeshProUGUI Tips;
    private KitchenRecipe NeedRecipe;
    public GameObject ThingGrids;
    public GameObject ThingGridsPrefab;
    public void OnEnable()
    {
        UpdateKitchenItems();
        CheckKitchenData();
        InvokeRepeating("CheckHarvestableItems", 1f, 30f);
        

    }
    public void CheckKitchenData()
    {

        if(GameNum.Kitchendata != null)
        {
            kitchenData = GameNum.Kitchendata;
        }
        else
        {
            kitchenData = new KitchenData();
            for (int i = 0; i < KitchenLv.level; i++)
            {
                kitchenData.cookingSlots.Add(new CookingSlot());
            }
        }
        RefreshGrid();


    }
    public void RefreshGrid()
    {
        for (int i = 0; i < ThingGrids.transform.childCount; i++)
        {
            Destroy(ThingGrids.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < KitchenLv.level; i++)
        {
            GameObject grid = Instantiate(ThingGridsPrefab, ThingGrids.transform);
            grid.GetComponent<EnhanceID>().index = i;
            if (kitchenData.cookingSlots[i] != null)
            {
                if (kitchenData.cookingSlots[i].isActive)
                {
                    grid.GetComponent<EnhanceID>().Img.sprite = kitchenData.cookingSlots[i].recipe.Food.ItemImg;
                    grid.GetComponent<EnhanceID>().Img.color = new Color32(255, 255, 255, 255);
                    grid.GetComponent<EnhanceID>().TxtNum.gameObject.SetActive(true);
                    if (!kitchenData.cookingSlots[i].isHarvest)
                    {
                        TimeSpan elapsed = DateTime.Now - kitchenData.cookingSlots[i].startTime;
                        float remainingSeconds = kitchenData.cookingSlots[i].recipe.cookingTime - (float)elapsed.TotalSeconds;
                        TimeSpan remainingTime = TimeSpan.FromSeconds(remainingSeconds);
                        // 四舍五入到分钟
                        int roundedMinutes = (int)Math.Round(remainingTime.TotalMinutes);
                        TimeSpan roundedTime = TimeSpan.FromMinutes(roundedMinutes);

                        grid.GetComponent<EnhanceID>().TxtNum.text = roundedTime.ToString(@"hh\:mm");

                    }
                    else
                    {
                        grid.GetComponent<EnhanceID>().TxtNum.text = "可收获";
                    }

                }
            }
        }
    }
    public void CheckHarvestableItems()
    {
        DateTime currentTime = DateTime.Now; // 只获取一次当前时间

        for (int i = 0; i < kitchenData.cookingSlots.Count; i++)
        {
            var slot = kitchenData.cookingSlots[i];

            // 跳过非活动或已可收获的槽位
            if (!slot.isActive || slot.isHarvest)
                continue;

            // 检查配方是否存在
            if (slot.recipe == null)
            {
                Debug.LogWarning($"槽位 {i} 有活动但没有配方!");
                continue;
            }

            // 计算经过的时间
            TimeSpan elapsed = currentTime - slot.startTime;

            // 检查是否完成烹饪
            if (elapsed.TotalSeconds >= slot.recipe.cookingTime)
            {
                slot.isHarvest = true;
                ThingGrids.transform.GetChild(i).GetComponent<EnhanceID>().TxtNum.text = "可收获";
            }
            else
            {
                float remainingSeconds = kitchenData.cookingSlots[i].recipe.cookingTime - (float)elapsed.TotalSeconds;
                TimeSpan remainingTime = TimeSpan.FromSeconds(remainingSeconds);
                //ThingGrids.transform.GetChild(i).GetComponent<EnhanceID>().TxtNum.text = remainingTime.ToString(@"hh\:mm");
                // 四舍五入到分钟
                int roundedMinutes = (int)Math.Round(remainingTime.TotalMinutes);
                TimeSpan roundedTime = TimeSpan.FromMinutes(roundedMinutes);

                ThingGrids.transform.GetChild(i).GetComponent<EnhanceID>().TxtNum.text = roundedTime.ToString(@"hh\:mm");
            }

        }

    }

    public void UpdateKitchenItems()
    {
        for(int i = 0; i < conten.transform.childCount; i++)
        {
            Destroy(conten.transform.GetChild(i).gameObject);
        }
        for(int i = 0; i < kitchenRecipes.Count; i++)
        {
            if(kitchenRecipes[i].requiredKitchenLevel <= KitchenLv.level)
            {
                InstertKitchenItems(kitchenRecipes[i].Food,i);
            }
            
        }
    }
    public BoxGrid boxGrid;
    public void InstertKitchenItems(Item item,int index)
    {
        BoxGrid grid = Instantiate(boxGrid, gameObject.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNameTxt.text = item.ItemName.ToString();
        grid.item = item;
        grid.recipe = kitchenRecipes[index];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null) return;
        if (eventData.pointerEnter.CompareTag("FoodGrid"))
        {
            BoxGrid grid = eventData.pointerEnter.GetComponent<BoxGrid>();
            MakeFoodTips.text = "";
            for (int i = 0;i < grid.recipe.ingredients.Count; i++)
            {
                MakeFoodTips.text += grid.recipe.ingredients[i].Item.ItemName + "*" + grid.recipe.ingredients[i].amount + ",";
            }
            NeedRecipe = grid.recipe;
            
        }
    }
    public void MakeFood()
    {
        if (NeedRecipe == null) return;
        bool IsSuccess = true;
        for(int i = 0;i < NeedRecipe.ingredients.Count; i++)
        {
            if (NeedRecipe.ingredients[i].Item.BoxItemNum < NeedRecipe.ingredients[i].amount)
            {
                Tips.gameObject.SetActive(true);
                Invoke("StopTips", 2f);
                Tips.text = "材料不足";
                Tips.color = Color.white;
                IsSuccess = false;
                break;
            }
        }
        if(IsSuccess)
        {
            for(int i = 0; i < kitchenData.cookingSlots.Count; i++)
            {
                if (!kitchenData.cookingSlots[i].isActive)
                {
                    kitchenData.cookingSlots[i].recipeId = NeedRecipe.recipeId;
                    kitchenData.cookingSlots[i].startTime = DateTime.Now;
                    kitchenData.cookingSlots[i].isActive = true;
                    kitchenData.cookingSlots[i].recipe = NeedRecipe;
                    
                    break;
                }
            }
            for (int i = 0; i < NeedRecipe.ingredients.Count; i++)
            {
                NeedRecipe.ingredients[i].Item.BoxItemNum -= NeedRecipe.ingredients[i].amount;
            }
            RefreshGrid();
            GameNum.Kitchendata = kitchenData;
            
        }
    }
    public void HarvesFood(int index)
    {
        if (!kitchenData.cookingSlots[index].isActive) return;
        if (!kitchenData.cookingSlots[index].isHarvest) return;
        if (kitchenData.cookingSlots[index].recipe.Food.BoxItemNum == 0)
        {
            MyBox.items.Add(kitchenData.cookingSlots[index].recipe.Food);
        }       
        kitchenData.cookingSlots[index].recipe.Food.BoxItemNum += 1;
        InitKitchenData(index);
        RefreshGrid();

    }
    public void InitKitchenData(int index)
    {
        kitchenData.cookingSlots[index].recipeId = -1;
        kitchenData.cookingSlots[index].isActive = false;
        kitchenData.cookingSlots[index].isHarvest = false;
        kitchenData.cookingSlots[index].recipe = null;
    }
    public void StopTips()
    {
        Tips.gameObject.SetActive(false);
    }
    public void ExitEvent()
    {
        MakeFoodTips.text = "";
        NeedRecipe = null;
    }
}
