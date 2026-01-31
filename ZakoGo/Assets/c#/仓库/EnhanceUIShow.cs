using Mirror.BouncyCastle.Utilities.Collections;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class EnhanceUIShow : MonoBehaviour,IPointerClickHandler
{

    public ItemList MyBox;
    public int TypeID = 0;
    public GameObject UI;
    private void OnEnable()
    {
        RefreshGrid();
        UpdateStoreItem();

    }

    #region 按需求更新仓库列表
    public void UpdateStoreItem()
    {
        for (int i = 0; i < UI.transform.childCount; i++)
        {
            Destroy(UI.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < MyBox.items.Count; i++)
        {
            if(TypeID == 0)
            {
                if (MyBox.items[i].itemtype == ItemType.饰品)
                {
                    InsertItem(MyBox.items[i]);
                }
            }
            else if (TypeID == 1)
            {
                if (MyBox.items[i].itemtype == ItemType.物品)
                {
                    InsertItem(MyBox.items[i]);
                }
            }
            else if(TypeID == 2)
            {
                if (MyBox.items[i].itemtype == ItemType.启动石)
                {
                    InsertItem(MyBox.items[i]);
                }
            }
            else if(TypeID == 3)
            {
                if (MyBox.items[i].itemtype == ItemType.强化石)
                {
                    InsertItem(MyBox.items[i]);
                }
            }
            
        }

    }
    public BoxGrid boxGrid;
    public void InsertItem(Item item)
    {
        BoxGrid grid = Instantiate(boxGrid, UI.transform);
        grid.ItemImage.sprite = item.ItemImg;
        grid.ItemNumTxt.text = item.BoxItemNum.ToString();
        grid.ItemNameTxt.text = item.ItemName.ToString();
        grid.ItemValueTxt.text = item.RealMoney.ToString();
        grid.item = item;
        grid.ChangeItemColor(item.ItemValue);


    }
    #endregion
    #region 将强化台材料移回仓库
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerEnter == null) return;
        if (eventData.pointerEnter.CompareTag("EnhanceGrid"))
        {
            TypeID = eventData.pointerEnter.GetComponent<EnhanceID>().index;
            if (TypeID == 0)
            {
                if (MyEnhanceList.Decoraion != null)
                {
                    MyEnhanceList.Decoraion.BoxItemNum += 1;
                    MyEnhanceList.Decoraion = null;
                    decorationGridImg.sprite = null;
                    decorationGridImg.color = new Color32(255, 255, 255, 0);
                    NeedItemsTxt.text = "";
                }

            }
            else if (TypeID == 1)
            {
                int index = eventData.pointerEnter.GetComponent<EnhanceID>().ThingGridIndex;
                if (MyEnhanceList.tiles[index].id != -1)
                {
                    MyEnhanceList.tiles[index].EnhanceItem.BoxItemNum += MyEnhanceList.tiles[index].Num;
                    MyEnhanceList.ResetATiles(index);
                    ThingGrid[index].GetComponent<EnhanceID>().InitGrid();
                }
                
            }
            else if (TypeID == 2)
            {
                if (MyEnhanceList.StartStone != null)
                {
                    MyEnhanceList.StartStone.BoxItemNum += 1;
                    MyEnhanceList.StartStone = null;
                    StartStoneGridImg.sprite = null;
                    StartStoneGridImg.color = new Color32(255, 255, 255, 0);
                }
            }
            else if (TypeID == 3)
            {
                if (MyEnhanceList.EnhanceStone != null)
                {
                    SuccessfulRate -= 20;
                    SuccessfulRateTxt.text = "成功率：" + SuccessfulRate.ToString();
                    MyEnhanceList.EnhanceStone.BoxItemNum += 1;
                    MyEnhanceList.EnhanceStone = null;
                    EnhanceStoneGridImg.sprite = null;
                    EnhanceStoneGridImg.color = new Color32(255, 255, 255, 0);
                }
            }
        }
        UpdateStoreItem();
    }
    #endregion
    #region 更新强化台UI
    public EnhanceList MyEnhanceList;
    public Image decorationGridImg;
    public Image StartStoneGridImg;
    public Image EnhanceStoneGridImg;
    public List<GameObject> ThingGrid;
    public RecipeList MyRecipeList;
    public TextMeshProUGUI NeedItemsTxt;
    public TextMeshProUGUI EnhanceTip;
    public TextMeshProUGUI SuccessfulRateTxt;
    public float SuccessfulRate;
    //移动物品到强化台UI更新逻辑
    public void Update_EnhanceUI(Item item,GameObject boxgrid)
    {
        if (TypeID == 0)
        {
            if(MyEnhanceList.Decoraion == null)
            {
                MyEnhanceList.Decoraion = item;
                decorationGridImg.sprite = item.ItemImg;
                item.BoxItemNum -= 1;
                boxgrid.GetComponent<BoxGrid>().ItemNumTxt.text = item.BoxItemNum.ToString();
                decorationGridImg.color = new Color32(255, 255, 255, 255);
                
                string ShowInfo = "需要";
                for (int i = 0;i < MyRecipeList.recipes.Count;i++)
                {
                    if(MyRecipeList.recipes[i].EnhanceItemID == item.ItemId)
                    {
                        if(item.ItemValueNum >= MyRecipeList.recipes[i].ingredients.Count)
                        {
                            ShowInfo = "已达到最高品质";
                        }
                        else
                        {
                            SuccessfulRate = MyRecipeList.recipes[i].ingredients[item.ItemValueNum].SuccessfulRate;
                            SuccessfulRateTxt.text = "成功率：" + SuccessfulRate.ToString();
                            for (int j = 0; j < MyRecipeList.recipes[i].ingredients[item.ItemValueNum].item.Count; j++)
                            {
                                ShowInfo += MyRecipeList.recipes[i].ingredients[item.ItemValueNum].item[j].ItemName;
                                ShowInfo += "*" + MyRecipeList.recipes[i].ingredients[item.ItemValueNum].amount[j].ToString() + "、";
                            }
                        }

                        
                    }
                    
                }
                NeedItemsTxt.text = ShowInfo;
            }

        }
        else if (TypeID == 1)
        {
            int index = MyEnhanceList.AddtionEnhanction(item.ItemId,item);
            Debug.Log(index);
            if (index < 0) return;
            item.BoxItemNum -= 1;
            boxgrid.GetComponent<BoxGrid>().ItemNumTxt.text = item.BoxItemNum.ToString();
            ThingGrid[index].GetComponent<EnhanceID>().ShowGrid(item,index);
        }
        else if (TypeID == 2)
        {
            if(MyEnhanceList.StartStone == null)
            {
                MyEnhanceList.StartStone = item;
                StartStoneGridImg.sprite = item.ItemImg;
                item.BoxItemNum -= 1;
                boxgrid.GetComponent<BoxGrid>().ItemNumTxt.text = item.BoxItemNum.ToString();
                StartStoneGridImg.color = new Color32(255, 255, 255, 255);
            }

        }
        else if(TypeID == 3)
        {
            if (MyEnhanceList.EnhanceStone == null)
            {
                MyEnhanceList.EnhanceStone = item;
                EnhanceStoneGridImg.sprite = item.ItemImg;
                item.BoxItemNum -= 1;
                boxgrid.GetComponent<BoxGrid>().ItemNumTxt.text = item.BoxItemNum.ToString();
                EnhanceStoneGridImg.color = new Color32(255, 255, 255, 255);
                SuccessfulRate += item.ItemValueNum;
                if(SuccessfulRate > 100)
                {
                    SuccessfulRate = 100;
                }
                SuccessfulRateTxt.text = "成功率：" + SuccessfulRate.ToString();

            }
        }

    }
    public void RefreshGrid()
    {
        for (int i = 0; i < ThingGrid.Count; i++)
        {
            ThingGrid[i].GetComponent<EnhanceID>().InitGrid();
        }
        MyEnhanceList.ResetAllTiles();
        decorationGridImg.color = new Color32(255, 255, 255, 0);
        StartStoneGridImg.color = new Color32(255, 255, 255, 0);
        EnhanceStoneGridImg.color = new Color32(255, 255, 255, 0);
        SuccessfulRate = 0;
        SuccessfulRateTxt.text = "成功率：" + SuccessfulRate.ToString();
        NeedItemsTxt.text = "";
    }
    #endregion
    public void EnhanceItemButtonEvent()
    {

        if(MyEnhanceList.StartStone == null)
        {
            EnhanceTip.gameObject.SetActive(true);
            EnhanceTip.text = "缺少启动石";
            Invoke("StopTxt",2f);
            return;
        }
        if (MyEnhanceList.Decoraion == null)
        {
            EnhanceTip.gameObject.SetActive(true);
            EnhanceTip.text = "缺少要强化的饰品";
            Invoke("StopTxt", 2f);
            return;
        }
        
        for (int i = 0; i < MyRecipeList.recipes.Count; i++)
        {
            if (MyRecipeList.recipes[i].EnhanceItemID == MyEnhanceList.Decoraion.ItemId)
            {
                if(MyEnhanceList.Decoraion.ItemValueNum >= MyRecipeList.recipes[i].ingredients.Count)
                {
                    return;
                }
                if (MyEnhanceList.CanEnhanceWithRecipeStrict(MyRecipeList.recipes[i], MyEnhanceList.Decoraion.ItemValueNum))
                {
                    EnhanceTip.gameObject.SetActive(true);
                    int ranNum = Random.Range(1, 101);
                    if(ranNum <= SuccessfulRate)
                    {
                        EnhanceTip.text = "强化成功!";
                        EnhanceTip.color = Color.green;
                        MyEnhanceList.Decoraion.ItemValueNum += 1;
                        UpdateItemValue(MyEnhanceList.Decoraion, MyEnhanceList.Decoraion.ItemValue);
                        Debug.Log("强化成功！");
                    }
                    else
                    {
                        EnhanceTip.text = "强化失败";
                        EnhanceTip.color = Color.red;
                        Debug.Log("强化失败");
                    } 
                    Invoke("StopTxt", 2f);
                    MyEnhanceList.Decoraion.BoxItemNum += 1;
                    RefreshGrid();
                                   
                }
                else
                {
                    EnhanceTip.gameObject.SetActive(true);
                    EnhanceTip.text = "材料不足";
                    Invoke("StopTxt", 2f);
                    Debug.Log("材料不足");
                }
                break;


            }

        }


    }
    public void StopTxt()
    {
        EnhanceTip.color = Color.white;
        EnhanceTip.gameObject.SetActive(false);
    }
    public void UpdateItemValue(Item item,ItemValueType type)
    {
        
        if (type == ItemValueType.劣质)
        {
            item.ItemValue = ItemValueType.普通;
        }
        else if (type == ItemValueType.普通)
        {
            item.ItemValue = ItemValueType.精良;
        }
        else if (type == ItemValueType.精良)
        {
            item.ItemValue = ItemValueType.优质;
        }
        else if (type == ItemValueType.优质)
        {
            item.ItemValue = ItemValueType.史诗;

        }
        else if (type == ItemValueType.史诗)
        {
            item.ItemValue = ItemValueType.传说;

        }


    }
    public void ExitBackItems()
    {
        if (MyEnhanceList.StartStone != null)
        {
            MyEnhanceList.StartStone.BoxItemNum += 1;
        }
        if (MyEnhanceList.Decoraion != null)
        {
            MyEnhanceList.Decoraion.BoxItemNum += 1;
        }
        if(MyEnhanceList.EnhanceStone != null)
        {
            MyEnhanceList.EnhanceStone.BoxItemNum += 1;
            SuccessfulRate = 0;
            SuccessfulRateTxt.text = "成功率：" + SuccessfulRate.ToString();

        }
        for(int i = 0;i < MyEnhanceList.tiles.Length; i++)
        {
            if(MyEnhanceList.tiles[i].id != -1)
            {
                MyEnhanceList.tiles[i].EnhanceItem.BoxItemNum += MyEnhanceList.tiles[i].Num;
            }
            
        }
        RefreshGrid();
        SuccessfulRateTxt.text = "";
        NeedItemsTxt.text = "";
        gameObject.SetActive(false);
    }
}
