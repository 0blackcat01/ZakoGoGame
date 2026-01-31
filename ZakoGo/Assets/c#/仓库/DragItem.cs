using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class DragItem : MonoBehaviour, IPointerClickHandler
{
    public ItemList MyBox;
    public ItemList MyBag;

    // 缓存UI组件
    private BoxUIShow boxUI;
    private BagUIShow bagUI;

    private void Start()
    {
        // 初始化时缓存UI组件
        boxUI = GameObject.FindGameObjectWithTag("BoxUI")?.GetComponent<BoxUIShow>();
        bagUI = GameObject.FindGameObjectWithTag("BagUI")?.GetComponent<BagUIShow>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null) return;

        // 获取BoxGrid组件只执行一次
        BoxGrid boxGrid = eventData.pointerEnter.GetComponent<BoxGrid>();
        if (boxGrid == null || boxGrid.item == null) return;

        // 处理盒子点击
        if (eventData.pointerEnter.CompareTag("BoxGrid"))
        {
            if (GameNum.WorldMarket)
            {
                GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().SaleInput.gameObject.SetActive(true);
                GameObject.FindGameObjectWithTag("Market").GetComponent<MarketManager>().SaleInput.GetComponent<SaleInputUI>()
                    .RecvIndex(gameObject.GetComponent<BoxGrid>().item);
                GameObject.FindGameObjectWithTag("BoxUI0")?.gameObject.SetActive(false);
            }
            else
            {
                if (boxGrid.item.itemtype == ItemType.背包)
                {
                    GetBag(boxGrid);
                }
                else if (boxGrid.item.itemtype == ItemType.衣物)
                {
                    
                }
                else if (boxGrid.item.itemtype == ItemType.饰品)
                {
                    GetDecoration(boxGrid);
                }
                else
                {
                    HandleBoxGridClick(boxGrid);
                }
            }

            
        }
        // 处理背包点击
        else if (eventData.pointerEnter.CompareTag("BagGrid"))
        {
            HandleBagGridClick(boxGrid);
        }
    }

    private void HandleBoxGridClick(BoxGrid boxGrid)
    {
        if (GameNum.IsSaled)
        {
            if (!string.IsNullOrEmpty(GameNum.PlayerName))
            {
                // 确保物品数量有效
                if (boxGrid.item.BoxItemNum <= 0) return;

                this.TriggerEvent(EventName.AddMoney, new Item_Args { AddmoneyNum = boxGrid.item.RealMoney });
                boxGrid.item.BoxItemNum -= 1;
                MyBox.CheckBoxZeroItem();

                // 安全更新UI
                boxUI?.UpdateBoxItem();
            }
        }
        else
        {
            // 检查背包容量
            if (MyBag.items.Count >= GameNum.BagCount) return;

            // 确保物品数量有效
            if (boxGrid.item.BoxItemNum <= 0) return;

            if(boxGrid.item.itemtype == ItemType.子弹)
            {
                if(boxGrid.item.BoxItemNum < 10)
                {
                    
                    boxGrid.item.BagItemNum += boxGrid.item.BoxItemNum;
                    boxGrid.item.BoxItemNum = 0;
                }
                else
                {
                    boxGrid.item.BoxItemNum -= 10;
                    boxGrid.item.BagItemNum += 10;
                }
            }
            else
            {
                boxGrid.item.BoxItemNum -= 1;
                boxGrid.item.BagItemNum += 1;
            }

            MyBag.AddItem(boxGrid.item);
            MyBox.CheckBoxZeroItem();

            // 安全更新UI
            bagUI?.UpdateBagItem();
            boxUI?.UpdateBoxItem();
        }
    }

    private void HandleBagGridClick(BoxGrid boxGrid)
    {
        if (!GameNum.CanMoveBagItems) return;
        // 检查盒子容量
        if (MyBox.items.Count >= GameNum.BoxCount) return;

        // 确保物品数量有效
        if (boxGrid.item.BagItemNum <= 0) return;

        if (!GameNum.IsSinglePlayer)
        {

            if (boxGrid.item.itemtype == ItemType.子弹)
            {
                if (boxGrid.item.BagItemNum < 10)
                {
                    boxGrid.item.BagItemNum = 0;
                }
                else
                {
                    boxGrid.item.BagItemNum -= 10;
                }
            }
            else
            {
                boxGrid.item.BagItemNum -= 1;
            }
            MyBag.CheckBagZeroItem();
            GameObject.FindGameObjectWithTag("BagUI")?.GetComponent<NetBagUIShow>()?.UpdateBagItem();
            return;
        }


        if (boxGrid.item.itemtype == ItemType.子弹)
        {
            if (boxGrid.item.BagItemNum < 10)
            {
                boxGrid.item.BoxItemNum += boxGrid.item.BagItemNum;
                boxGrid.item.BagItemNum = 0;
                
            }
            else
            {
                boxGrid.item.BoxItemNum += 10;
                boxGrid.item.BagItemNum -= 10;
            }
        }
        else
        {
            boxGrid.item.BoxItemNum += 1;
            boxGrid.item.BagItemNum -= 1;
        }
        MyBox.AddItem(boxGrid.item);
        MyBag.CheckBagZeroItem();

        // 安全更新UI
        bagUI?.UpdateBagItem();
        boxUI?.UpdateBoxItem();
    }
    public void GetBag(BoxGrid boxGrid)
    {
        // 确保物品数量有效
        if (boxGrid.item.BoxItemNum <= 0) return;
        if (boxGrid.item.BagItemNum != 0) return;
        boxGrid.item.BoxItemNum -= 1;
        boxGrid.item.BagItemNum += 1;
        MyBox.CheckBoxZeroItem();
        boxUI?.UpdateBoxItem();
        GameObject.FindGameObjectWithTag("EquipUI").gameObject.GetComponent<PlayerEquip>().SetBag(boxGrid.item);

    }
    public void GetDecoration(BoxGrid boxGrid)
    {
        // 确保物品数量有效
        if (boxGrid.item.BoxItemNum <= 0) return;
        if (boxGrid.item.BagItemNum != 0) return;
        boxGrid.item.BoxItemNum -= 1;
        boxGrid.item.BagItemNum += 1;
        MyBox.CheckBoxZeroItem();
        boxUI?.UpdateBoxItem();
        GameObject.FindGameObjectWithTag("EquipUI").gameObject.GetComponent<PlayerEquip>().SetDecoration(boxGrid.item);
    }



}
