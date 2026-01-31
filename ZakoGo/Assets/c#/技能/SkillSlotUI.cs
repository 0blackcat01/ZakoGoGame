using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private Button skillButton;

    private SkillSlot skillSlot;
    private Action onClickCallback;

    public void Initialize(SkillSlot slot, Action callback)
    {
        skillSlot = slot;
        onClickCallback = callback;

        if (skillSlot.skillData != null)
        {
            iconImage.sprite = skillSlot.skillData.icon;
        }
        Debug.Log($"回调函数设置: {callback != null}");
        Debug.Log($"回调方法: {callback?.Method.Name}");
        skillButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        if (skillSlot == null) return;
        if (skillSlot.skillData != null)
        {
            // 更新冷却显示
            if (skillSlot.cooldownRemaining > 0)
            {
                cooldownOverlay.fillAmount = skillSlot.cooldownRemaining / skillSlot.skillData.cooldown;

            }
            else
            {
                cooldownOverlay.fillAmount = 0;

            }

            // 更新按钮状态
            skillButton.interactable = skillSlot.cooldownRemaining <= 0;
        }
    }

    private void OnClick()
    {
        Debug.Log("按钮被点击！");
        if (onClickCallback == null)
        {
            Debug.LogError("回调函数为空！");
            return;
        }
        onClickCallback?.Invoke();
    }
}