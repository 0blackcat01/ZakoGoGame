using Mirror.BouncyCastle.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class KeepNum : MonoBehaviour
{
    // Start is called before the first frame update
    const string PLAYER_DATA_KEY = "PlayerData";
    const string PLAYER_DATA_FILE_NAME = "PlayerData.game";

    private void Start()
    {
        if (GameNum.FirstLoad)
        {
            GameNum.FirstLoad = false;
            Load();
        }
        
    }
    private void OnEnable()
    {
        if (GameNum.IsCleanItem)
        {
            AllItems.ResetList();
        }
    }

    void SaveByJson()
    {
        SaveSystem.SaveByJson(PLAYER_DATA_FILE_NAME, SavingData());

    }
    void LoadFromJson()
    {
        var saveData = SaveSystem.LoadFromJson<SaveData>(PLAYER_DATA_FILE_NAME);
        if (saveData == null) return;
        LoadData(saveData);
    }
    [System.Serializable]
    class SaveData
    {
        public int BagCount;
        public int BoxCount;
        public int[] BagItemIds = new int[50];
        public int[] BoxItemIds = new int[100];

        public int Money;
        public int HpMax;
        public bool FirstLoad;
        //³ø·¿Êý×é
        public long[] lastSaveTimeTicks;
        public int[] recipeids;
        public bool[] IsActives;
    }
    public ItemList MyBag;
    public ItemList MyBox;
    public ItemList AllItems;
    SaveData SavingData()
    {
        var data = new SaveData();
        data.BagCount = GameNum.BagCount;
        data.BoxCount = GameNum.BoxCount;
        for (int i = 0;i < MyBag.items.Count; i++)
        {
            data.BagItemIds[i] = MyBag.items[i].ItemId;
        }
        for (int i = 0; i < MyBox.items.Count; i++)
        {
            data.BoxItemIds[i] = MyBox.items[i].ItemId;
        }
        
        data.Money = GameNum.Money;
        data.HpMax = GameNum.HpMax;
        data.FirstLoad = GameNum.FirstLoad;

        if(GameNum.Kitchendata != null)
        {
            DateTime[] times = new DateTime[GameNum.Kitchendata.cookingSlots.Count];
            data.recipeids = new int[GameNum.Kitchendata.cookingSlots.Count];
            data.IsActives = new bool[GameNum.Kitchendata.cookingSlots.Count];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = GameNum.Kitchendata.cookingSlots[i].startTime;
                data.recipeids[i] = GameNum.Kitchendata.cookingSlots[i].recipeId;
                data.IsActives[i] = GameNum.Kitchendata.cookingSlots[i].isActive;

            }
            data.lastSaveTimeTicks = SetLastSaveTime(times);
        }

        return data;
    }
    void LoadData(SaveData data)
    {
        GameNum.BagCount = data.BagCount;
        GameNum.BoxCount = data.BoxCount;
        MyBag.items.Clear();
        MyBox.items.Clear();
        int index = 0;
        for (int i = 0; i < GameNum.BagCount; i++)
        {
            if (data.BagItemIds[i] != 0)
            {
                index = data.BagItemIds[i];
                MyBag.items.Add(AllItems.items.Find(i => i.ItemId == index));
            }
        }
        for (int i = 0; i < GameNum.BoxCount; i++)
        {
            if (data.BoxItemIds[i] != 0)
            {
                index = data.BoxItemIds[i];
                MyBox.items.Add(AllItems.items.Find(i => i.ItemId == index));
            }
        }

        GameNum.Money = data.Money;
        GameNum.HpMax = data.HpMax;
        if(data.lastSaveTimeTicks != null)
        {
            DateTime[] times = GetLastSaveTime(data.lastSaveTimeTicks);
            for (int i = 0; i < times.Length; i++)
            {
                GameNum.Kitchendata.cookingSlots[i].startTime = times[i];
                GameNum.Kitchendata.cookingSlots[i].recipeId = data.recipeids[i];
                GameNum.Kitchendata.cookingSlots[i].isActive = data.IsActives[i];
            }
        }


    }
    public TextMeshProUGUI SaveTip;
    public void Save()
    {
        SaveByJson();
        SaveTip.gameObject.SetActive(true);
        Invoke("StopTip",2f);
    }
    public void Load()
    {
        LoadFromJson();

    }
    public void StopTip()
    {
        SaveTip.gameObject.SetActive(false);
    }
    
    public long[] SetLastSaveTime(DateTime[] time)
    {
        long[] TimeTicks = new long[time.Length];
        for (int i = 0;i < time.Length; i++)
        {
            TimeTicks[i] = time[i].Ticks;
        }
        
        return TimeTicks;
    }

    public DateTime[] GetLastSaveTime(long[] longs)
    {
        DateTime[] Times = new DateTime[longs.Length];
        for (int i = 0; i < Times.Length; i++)
        {
            Times[i] = new DateTime(longs[i]);
        }
        return Times;
    }
    public void CleanItem()
    {
        if (GameNum.IsCleanItem)
        {
            AllItems.ResetList();
            MyBag.items.Clear();
            MyBox.items.Clear();
        }
    }
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Developer/Delete ItemNum")]
    public static void DeletePlayerDataPrefs()
    {
        GameNum.IsCleanItem = !GameNum.IsCleanItem;
    }
#endif
}
