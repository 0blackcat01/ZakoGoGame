using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCropData", menuName = "Farming/Crop Data")]
public class CropData : ScriptableObject
{
    public int cropid = 0;
    public string cropName;
    public Sprite[] growthStageSprites; // 各阶段显示图片
    public float[] growthStageTimes;    // 各阶段所需时间(小时)
    public float totalGrowthTime;       // 总生长时间(自动计算)
    public Item HarvestPlant;
    public int HarvestMaxNum = 0;

    private void OnValidate()
    {
        // 自动计算总生长时间
        totalGrowthTime = 0;
        foreach (var time in growthStageTimes)
        {
            totalGrowthTime += time;
        }
    }
}