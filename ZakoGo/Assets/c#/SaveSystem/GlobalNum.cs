using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalNum : MonoBehaviour
{
    // Start is called before the first frame update
}
//用于管理游戏里最重要的数值类
public static class GameNum
{
    //需要保存的
    public static int Money;
    public static int HpMax = 100;
    public static int Level = 1;
    public static int LevelNum = 0;
    public static float MoveSpeed = 5;
    public static float DenfenseNum = 0;
    public static float CritRateNum = 0;
    public static float CostRestore = 0;
    public static float CostTolNum = 0;


    public static float HpAddtion = 0;
    public static float MoveSpeedAddtion = 0;
    public static float JumpForceAddtion = 0;
    public static float CritRateAddtion = 0;
    public static float DenfenseAddtion = 0;
    public static float HateAddtion = 0;
    public static float LuckyAddtion = 0;
    public static float CostRestoreAddtion = 0;
    public static float CostTolNumAddtion = 0;


    public static int BulletNum = 20;
    public static int TolBulletNum = 100;
    public static int BagCount = 4;
    public static int BoxCount = 10;
 
    public static bool FirstLoad = true;
   
    public static string PlayerName;

    public static KitchenData Kitchendata;
    public static List<FarmPlot> farmPlots;

    public static Item BagSpaceItem;
    public static Item DecorationSpaceItem;
    public static Item ClothSpaceItem;

    
    //不需要保存的
    public static string Sercer_Ip = "47.115.143.68";//47.115.143.68
    public static string Server_Api = "http://47.115.143.68:3001/api/";

    public static int FarmIndex = -1;//不需要存储，中间变量

    public static bool CanMoveBagItems = true;

    public static bool WorldMarket = false;
    public static bool IsCleanItem = false;
    public static bool IsOpenUI = false;
    public static bool IsSaled = false;

    public static bool IsSinglePlayer = true;
}
public static class BuildingLevelUp
{
    public static int BoxLevel = 1;
    public static int MakingLevel = 1;
    public static int MakingDurability = 0;   
    public static int TrashCyLevel = 1;
    public static int GardenLevel = 1;
    public static int KitchenLevel = 1;
    public static int KitchenDurability = 0;
    
    public static int EnhanceDurability = 0;

}

