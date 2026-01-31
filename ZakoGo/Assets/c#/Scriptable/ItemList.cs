using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[CreateAssetMenu(fileName = "New ItemList", menuName = "Item/New ItemList")]
[System.Serializable]
public class ItemList : ScriptableObject
{
    [Header("容器名称")]
    public string BoxName;

    [Tooltip("资源点标识")]
    public int FindPoint;

    [Space]
    public List<Item> items = new List<Item>();
    public List<int> OtherNums = new List<int>();

    public void CheckBagZeroItem()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].BagItemNum <= 0)
            {
                items.Remove(items[i]);
            }
        }
    }
    public void CheckBoxZeroItem()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].BoxItemNum <= 0)
            {
                items.Remove(items[i]);
            }
        }
    }
    public void CheckOtherZeroItem()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].OtherItemNum <= 0)
            {
                items.Remove(items[i]);
            }
        }
    }
    public void AddItem(Item item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
        }
    }
    public void ResetList()
    {
        for(int i = 0;i < items.Count; i++)
        {
            items[i].BoxItemNum = 0;
            items[i].BagItemNum = 0;
            items[i].OtherItemNum = 0;

        }
    }
}
