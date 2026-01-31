using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerEquip : MonoBehaviour,IPointerClickHandler
{
    public ItemList MyBox;
    public ItemList MyBag;
    private BagUIShow bagUI;
    private BoxUIShow boxUI;

    [Header("背包栏位")]
    public UnityEngine.UI.Image BagImg;
    public Item BagSpace;
    [Header("饰品栏位")]
    public UnityEngine.UI.Image DecorationImg;
    public Item DecorationSpace;
    [Header("鞋子栏位")]
    public UnityEngine.UI.Image ClothImg;
    public Item ClothSpace;
    // Start is called before the first frame update
    void Start()
    {
        bagUI = GameObject.FindGameObjectWithTag("BagUI")?.GetComponent<BagUIShow>();
        boxUI = GameObject.FindGameObjectWithTag("BoxUI")?.GetComponent<BoxUIShow>();
        BagSpace = GameNum.BagSpaceItem;
        DecorationSpace = GameNum.DecorationSpaceItem;
        ClothSpace = GameNum.ClothSpaceItem;

           
    }
    public void SetBag(Item bag)
    {
        BagImg.sprite = bag.ItemImg;
        BagSpace = bag;
        GameNum.BagSpaceItem = bag;
        BagImg.color = new Color32(255, 255, 255, 255);
        GameNum.BagCount = 4 + bag.BagAddtion;
        bagUI.UpdateBagCountTxt();
    }
    public void SetDecoration(Item decoration)
    {
        if (decoration == null) return;

        // 1. 移除当前装饰品的加成
        if (GameNum.DecorationSpaceItem != null)
        {
            RemoveDecorationEffects(DecorationSpace);
            GameNum.DecorationSpaceItem.BoxItemNum += 1;
            GameNum.DecorationSpaceItem.BagItemNum -= 1;
            if (!MyBox.items.Contains(GameNum.DecorationSpaceItem))
            {
                MyBox.items.Add(GameNum.DecorationSpaceItem);
                boxUI?.UpdateBoxItem();
            }
        }

        // 2. 应用新装饰品的加成
        ApplyDecorationEffects(decoration);

        // 3. 更新UI和引用
        UpdateDecorationVisuals(decoration);
    }

    private void RemoveDecorationEffects(Item decoration)
    {
        if (decoration.HPAddtion.Count > 0)
        {
            GameNum.HpAddtion -= decoration.HPAddtion[decoration.ItemValueNum];
        }
        if (decoration.SpeedAddtion.Count > 0)
        {
            GameNum.MoveSpeedAddtion -= decoration.SpeedAddtion[decoration.ItemValueNum];
        }
        if (decoration.DenfenseAddtion.Count > 0)
        {
            GameNum.DenfenseAddtion -= decoration.DenfenseAddtion[decoration.ItemValueNum];
        }
        if (decoration.CritRateAddtion.Count > 0)
        {
            GameNum.CritRateAddtion -= decoration.CritRateAddtion[decoration.ItemValueNum];
        }
        if (decoration.HatedAddtion.Count > 0)
        {
            GameNum.HateAddtion -= decoration.HatedAddtion[decoration.ItemValueNum];
        }
    }

    private void ApplyDecorationEffects(Item decoration)
    {
        if (decoration.HPAddtion.Count > 0)
        {
            GameNum.HpAddtion += decoration.HPAddtion[decoration.ItemValueNum];
        }
        if (decoration.SpeedAddtion.Count > 0)
        {
            GameNum.MoveSpeedAddtion += decoration.SpeedAddtion[decoration.ItemValueNum];
        }
        if (decoration.DenfenseAddtion.Count > 0)
        {
            GameNum.DenfenseAddtion += decoration.DenfenseAddtion[decoration.ItemValueNum];
        }
        if (decoration.CritRateAddtion.Count > 0)
        {
            GameNum.CritRateAddtion += decoration.CritRateAddtion[decoration.ItemValueNum];
        }
        if (decoration.HatedAddtion.Count > 0)
        {
            GameNum.HateAddtion += decoration.HatedAddtion[decoration.ItemValueNum];
        }
    }
    private void UpdateDecorationVisuals(Item decoration)
    {
        DecorationImg.sprite = decoration.ItemImg;
        DecorationImg.color = Color.white; // 等同于 new Color32(255, 255, 255, 255)
        DecorationSpace = decoration;
        GameNum.DecorationSpaceItem = decoration;
    }
    public void ReSetBag()
    {
        if(GameNum.BoxCount - MyBox.items.Count < MyBag.items.Count - 4) return;
        if(MyBag.items.Count > 4)
        {
            for (int i = 3; i < MyBag.items.Count; i++)
            {
                if (!MyBox.items.Contains(MyBag.items[i]))
                {
                    MyBox.items.Add(MyBag.items[i]);
                }
                MyBag.items[i].BoxItemNum += MyBag.items[i].BagItemNum;
                MyBag.items[i].BagItemNum = 0;
                MyBag.items.Remove(MyBag.items[i]);
            }
        }
        GameNum.BagCount = 4;
        BagSpace.BoxItemNum += 1;
        BagSpace.BagItemNum -= 1;
        if (!MyBox.items.Contains(BagSpace))
        {
            MyBox.items.Add(BagSpace);
        }
        bagUI.UpdateBagCountTxt();
        boxUI?.UpdateBoxItem();
        bagUI?.UpdateBagItem();
        BagImg.color = new Color32(255,255, 255, 0);
        BagImg.sprite = null;
        BagSpace = null;
        GameNum.BagSpaceItem = null;
    }
    public void ReSetDecoration()
    {
        if (MyBox.items.Count >= GameNum.BoxCount) return;
        RemoveDecorationEffects(DecorationSpace);
        DecorationSpace.BoxItemNum += 1;
        DecorationSpace.BagItemNum -= 1;
        if (!MyBox.items.Contains(DecorationSpace))
        {
            MyBox.items.Add(DecorationSpace);
        }
        bagUI.UpdateBagCountTxt();
        boxUI?.UpdateBoxItem();
        bagUI?.UpdateBagItem();
        DecorationImg.color = new Color32(255, 255, 255, 0);
        DecorationImg.sprite = null;
        DecorationSpace = null;
        GameNum.DecorationSpaceItem = null;
    }
    public void ReSetFoot()
    {
        if (MyBox.items.Count >= GameNum.BoxCount) return;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null) return;
        if (eventData.pointerEnter.CompareTag("EquipGrid"))
        {
            int id = eventData.pointerEnter.GetComponent<EquipID>().Euqip_id;
            if (id == 0)
            {
                ReSetBag();
            }
            else if(id == 1)
            {
                ReSetDecoration();
            }
            else if(id == 2)
            {
                ReSetFoot();
            }
        }
    }
}
