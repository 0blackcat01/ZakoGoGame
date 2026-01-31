using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickFood : MonoBehaviour, IPointerClickHandler
{


    public ItemList MyBag;

    // 缓存UI组件
    private NetPlayerControl mPlayerControl;
    private ShowFoodFast FoodFastUI;

    private void Start()
    {
        // 初始化时缓存UI组件
        mPlayerControl = GetComponentInParent<NetPlayerControl>();
        FoodFastUI = GetComponentInParent<ShowFoodFast>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        // 只有本地玩家可以操作
        if (!mPlayerControl.isLocalPlayer) return;

        if (eventData.pointerEnter == null) return;


        // 获取BoxGrid组件只执行一次
        BoxGrid boxGrid = eventData.pointerEnter.GetComponent<BoxGrid>();
        if (boxGrid == null || boxGrid.item == null) return;
        // 处理盒子点击
        if (eventData.pointerEnter.CompareTag("FoodGrid"))
        {
            boxGrid.item.BagItemNum -= 1;
            boxGrid.ItemNumTxt.text = boxGrid.item.BagItemNum.ToString();
            MyBag.CheckBagZeroItem();
            FoodFastUI.UpdateBagItem();
            mPlayerControl.EatingStart(boxGrid.item.HPAddtion[0]);

        }


    }
}
