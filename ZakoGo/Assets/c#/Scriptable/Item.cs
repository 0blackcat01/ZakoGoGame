using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Item/New Item")]
[System.Serializable]
public class Item : ScriptableObject
{
    public int ItemId;
    public int BoxItemNum;
    public int BagItemNum;
    public int OtherItemNum;
    public string ItemName;
    public Sprite ItemImg;
    public ItemType itemtype;
    public bool IsStack = false;
    [Header("商店设置")]
    public int RealMoney;
    [Header("品质")]
    public ItemValueType ItemValue;
    public int ItemValueNum;
    [TextArea] public string ItemInfo;
    [Header("道具数值加成")]
    public List<int> HPAddtion;
    public List<float> SpeedAddtion;
    public List<int> DenfenseAddtion;
    public List<int> CritRateAddtion;
    public List<int> HatedAddtion;
    public bool IsOpenDecoration = false;
    [Header("出货概率")]
    public int FindRate;
    [Header("出货地点")]
    public List<int> FindPoint;
    [Header("背包格数")]
    public int BagAddtion;
    [Header("食物属性")]
    public float EatingTime;//食用时间
    [Header("田地属性")]
    public CropData CropData;

}
public enum ItemValueType
{
    无,劣质,普通,精良,优质,史诗,传说  //灰色，白色，绿色，蓝色，紫色，橙色       

}
public enum ItemType
{
    子弹,物品,衣物,饰品,背包,食物,启动石,强化石,种子,宝箱
}
