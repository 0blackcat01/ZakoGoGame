using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// SkillUIManager.cs
public class SkillUIManager : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private RectTransform skillPanel;
    [SerializeField] private SkillSlotUI[] skillSlots;
    [SerializeField] private GameObject targetingUI;
    [SerializeField] private Image holdIndicator;
    [SerializeField] private LineRenderer trajectoryRenderer;

    [Header("指示器")]
    [SerializeField] private GameObject directionIndicator;
    [SerializeField] private GameObject areaIndicator;
    [SerializeField] private GameObject unitIndicator;

    [SerializeField] private PlayerSkillManager skillManager;
    private SkillData currentTargetingSkill;
    private void Start()
    {
        Initialize();
    }
    public void Initialize()
    {

        for (int i = 0; i < skillSlots.Length; i++)
        {
            int index = i; // 闭包捕获
            skillSlots[i].Initialize(skillManager.skillSlots[i], () => skillManager.TryCastSkill(index));
        }
    }

    public void ShowTargetingUI(SkillData skillData)
    {
        currentTargetingSkill = skillData;
        targetingUI.SetActive(true);

        // 隐藏所有指示器
        directionIndicator.SetActive(false);
        areaIndicator.SetActive(false);
        unitIndicator.SetActive(false);
    }

    public void HideTargetingUI()
    {
        targetingUI.SetActive(false);
        currentTargetingSkill = null;
    }

    public void ShowDirectionIndicator(float range)
    {
        directionIndicator.SetActive(true);
        directionIndicator.transform.localScale = new Vector3(range, 1, 1);
    }

    public void ShowAreaIndicator(float radius)
    {
        areaIndicator.SetActive(true);
        areaIndicator.transform.localScale = Vector3.one * radius * 2;
    }

    public void ShowUnitTargeting()
    {
        unitIndicator.SetActive(true);
    }

    public void UpdateTargetingPosition(Vector3 worldPosition)
    {
        if (currentTargetingSkill == null) return;

        switch (currentTargetingSkill.targetingMode)
        {
            case TargetingMode.Direction:
                UpdateDirectionIndicator(worldPosition);
                break;
            case TargetingMode.Area:
                areaIndicator.transform.position = worldPosition;
                break;
            case TargetingMode.Unit:
                UpdateUnitIndicator(worldPosition);
                break;
        }
    }

    private void UpdateDirectionIndicator(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        directionIndicator.transform.position = transform.position;
        directionIndicator.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void UpdateUnitIndicator(Vector3 targetPosition)
    {
        // 寻找最近的单位
        // TODO: 实现单位高亮
    }

    public void UpdateHoldIndicator(float percentage)
    {
        if (holdIndicator != null)
        {
            holdIndicator.fillAmount = percentage;

            // 颜色变化：绿->黄->红
            Color color = Color.Lerp(Color.green, Color.red, percentage);
            holdIndicator.color = color;
        }
    }

    public void DrawTrajectory(Vector3 start, Vector3 end, float speed)
    {
        trajectoryRenderer.positionCount = 20;

        for (int i = 0; i < 20; i++)
        {
            float t = i / 19f;
            Vector3 point = CalculateTrajectoryPoint(start, end, speed, t);
            trajectoryRenderer.SetPosition(i, point);
        }
    }

    private Vector3 CalculateTrajectoryPoint(Vector3 start, Vector3 end, float speed, float t)
    {
        // 简单的抛物线轨迹
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        float time = distance / speed;

        return start + direction * speed * t * time + Physics.gravity * 0.5f * Mathf.Pow(t * time, 2);
    }
}

