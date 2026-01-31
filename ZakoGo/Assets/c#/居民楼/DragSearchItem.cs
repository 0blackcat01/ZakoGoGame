using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;
public class DragSearchItem : MonoBehaviour,IPointerClickHandler
{
    public ItemList MyBox;
    public ItemList MyBag;

    // 缓存UI组件
    private NetPlayerControl mPlayerControl;
    private BagUIShow bagUI;

    private void Start()
    {
        // 初始化时缓存UI组件
        mPlayerControl = GetComponentInParent<NetPlayerControl>();
        //bagUI = GameObject.FindGameObjectWithTag("BagUI")?.GetComponent<BagUIShow>();
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
        if (eventData.pointerEnter.CompareTag("BoxGrid"))
        {
            HandleBoxGridClick(boxGrid);
        }
        // 处理背包点击
        else if (eventData.pointerEnter.CompareTag("BagGrid"))
        {
            HandleBagGridClick(boxGrid);
        }
    }

    private void HandleBoxGridClick(BoxGrid boxGrid)
    {
        // 检查背包容量
        if (MyBag.items.Count >= GameNum.BagCount && !MyBag.items.Contains(boxGrid.item)) return;

        mPlayerControl.TakeItemFromBox(boxGrid.item.ItemId, 1, gameObject.GetComponent<BoxGrid>().index);

    }

    private void HandleBagGridClick(BoxGrid boxGrid)
    {

        // 确保物品数量有效
        if (boxGrid.item.BagItemNum <= 0) return;


    }
}
