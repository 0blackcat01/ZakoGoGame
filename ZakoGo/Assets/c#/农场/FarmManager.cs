using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
[System.Serializable]
public class FarmPlot
{
    public int plotID;
    public CropData plantedCrop;
    public DateTime plantTime;
    public int currentStage;
    public bool isReadyForHarvest;

    // 计算当前生长进度(0-1)
    public float GetGrowthProgress()
    {
        if (plantedCrop == null) return 0;

        TimeSpan growthTime = DateTime.Now - plantTime;
        return Mathf.Clamp01((float)(growthTime.TotalHours / plantedCrop.totalGrowthTime));
    }

    // 检查是否需要更新生长阶段
    public bool CheckGrowthStageUpdate()
    {
        if (plantedCrop == null || currentStage >= plantedCrop.growthStageSprites.Length - 1)
            return false;

        TimeSpan stageTime = DateTime.Now - plantTime;
        float accumulatedTime = 0;

        for (int i = 0; i <= currentStage; i++)
        {
            accumulatedTime += plantedCrop.growthStageTimes[i];
        }

        if (stageTime.TotalHours >= accumulatedTime)
        {
            currentStage++;
            if (currentStage == plantedCrop.growthStageSprites.Length - 1)
            {
                isReadyForHarvest = true;
            }
            return true;
        }

        return false;
    }
}
public class FarmManager : MonoBehaviour
{


    [Header("田地设置")]
    public int plotCount = 1;
    public CropData[] availableCrops;

    [Header("可视化")]
    public SpriteRenderer[] plotRenderers; // 对应田地的SpriteRenderer
    public GameObject[] FarmGrids;

    [SerializeField]private List<FarmPlot> farmPlots = new List<FarmPlot>();
    private DateTime lastQuitTime;

    public Recipe Farm;
    public ItemList MyBox;

    private void Awake()
    {
        plotCount = Farm.level;
        InitializePlots();

    }
    private void Update()
    {
        CheckHarvest();
    }
    public void CheckHarvest()
    {     
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (GameNum.FarmIndex == -1) return;
            if (farmPlots[GameNum.FarmIndex].plantedCrop == null) return;
            HarvestCrop(GameNum.FarmIndex);
        }
    }

    private void InitializePlots()
    {
        if(farmPlots.Count == 0)
        {
            for (int i = 0; i < plotCount; i++)
            {
                farmPlots.Add(new FarmPlot { plotID = i });
            }
        }
        else
        {
            farmPlots = GameNum.farmPlots;
        }

    }

    private void Start()
    {
        UpdateAllPlotsVisuals();
        InvokeRepeating("CheckUpdate", 1,5);
    }


    // 种植作物
    public void PlantCrop(int plotIndex, int cropIndex)
    {
        if (plotIndex < 0 || plotIndex >= farmPlots.Count) return;
        if (cropIndex < 0 || cropIndex >= availableCrops.Length) return;

        farmPlots[plotIndex].plantedCrop = availableCrops[cropIndex];
        farmPlots[plotIndex].plantTime = DateTime.Now;
        farmPlots[plotIndex].currentStage = 0;
        farmPlots[plotIndex].isReadyForHarvest = false;
        UpdatePlotVisual(plotIndex);
        GameNum.farmPlots = farmPlots;

    }

    // 收获作物
    public void HarvestCrop(int plotIndex)
    {
        if (plotIndex < 0 || plotIndex >= farmPlots.Count) return;
        if (!farmPlots[plotIndex].isReadyForHarvest) return;

        // 这里可以添加收获奖励逻辑
        Debug.Log($"收获 {farmPlots[plotIndex].plantedCrop.cropName}!");
        MyBox.AddItem(farmPlots[plotIndex].plantedCrop.HarvestPlant);
        int ran_num = UnityEngine.Random.Range(1,
            farmPlots[plotIndex].plantedCrop.HarvestMaxNum);
        farmPlots[plotIndex].plantedCrop.HarvestPlant.BoxItemNum += ran_num;
        ShowHarvestText(Camera.main.transform.position, farmPlots[plotIndex].plantedCrop.HarvestPlant.ItemName, ran_num);
        farmPlots[plotIndex].plantedCrop = null;
        UpdatePlotVisual(plotIndex);
        GameNum.farmPlots = farmPlots;
        



    }

    // 更新所有田地视觉效果
    private void UpdateAllPlotsVisuals()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if(i+1 <= farmPlots.Count)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(true);
            }
            else
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        CheckUpdate();
    }
    public void CheckUpdate()
    {
        for (int i = 0; i < farmPlots.Count; i++)
        {
            UpdatePlotVisual(i);
        }
    }
    // 更新单个田地视觉效果
    private void UpdatePlotVisual(int plotIndex)
    {
        var plot = farmPlots[plotIndex];
        if (plot.plantedCrop == null)
        {
            plotRenderers[plotIndex].sprite = null; // 显示空田地
            plotRenderers[plotIndex].color = new Color32(255,255, 255, 0);
            return;
        }
        plotRenderers[plotIndex].color = new Color32(255, 255, 255, 255);
        // 检查是否需要更新生长阶段
        if (plot.CheckGrowthStageUpdate())
        {
            Debug.Log($"田地 {plotIndex} 的作物进入阶段 {plot.currentStage}");
        }

        // 设置对应阶段的图片
        plotRenderers[plotIndex].sprite = plot.plantedCrop.growthStageSprites[plot.currentStage];
    }
    [SerializeField] private TextMeshProUGUI harvestTextPrefab;
    [SerializeField] private Canvas gameCanvas;

    public void ShowHarvestText(Vector3 worldPosition, string itemName, int amount)
    {
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        TextMeshProUGUI text = Instantiate(harvestTextPrefab, gameCanvas.transform);
        text.transform.position = screenPosition;
        text.text = "获得"+ itemName + "*" + amount;

        // 动画序列
        Sequence sequence = DOTween.Sequence();
        sequence.Append(text.transform.DOMoveY(screenPosition.y + 100f, 2f));
        sequence.Join(text.DOFade(0f, 2f));
        sequence.OnComplete(() => Destroy(text.gameObject));
    }
}