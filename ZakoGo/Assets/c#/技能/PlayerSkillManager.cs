using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

// PlayerSkillManager.cs
public class PlayerSkillManager : NetworkBehaviour
{
    [Header("技能配置")]
    [SerializeField] public SkillSlot[] skillSlots = new SkillSlot[2]; // 2个技能槽
    [Header("全部技能列表")]
    [SerializeField] private List<SkillData> allSkills = new List<SkillData>();  // 拖入所有技能资产

    [Header("状态")]
    [SyncVar] private int currentCastingSkillId = -1;
    [SyncVar] private SkillState currentSkillState = SkillState.Ready;
    [SyncVar] private float currentCooldown = 0f;

    [Header("二次操作")]
    private Vector3? secondTargetPosition = null;
    private float holdStartTime = 0f;
    private bool isHolding = false;

    [Header("引用")]
    public ActiveSkill CostPart;
    [SerializeField] private SkillUIManager skillUI;

    private Dictionary<uint, SkillInstance> activeSkills = new Dictionary<uint, SkillInstance>();
    private Camera mainCamera;


    [Client]
    public void TryCastSkill(int slotIndex)
    {
        if (currentSkillState != SkillState.Ready) return;

        var slot = skillSlots[slotIndex];
        if (slot.skillData == null || slot.cooldownRemaining > 0) return;

        // 检查资源
        if (CostPart.currentCharges < slot.skillData.Cost)
        {
            // 显示法力不足提示
            return;
        }

        currentCastingSkillId = slot.skillData.skillID;

        if (slot.skillData.requireSecondInput)
        {
            // 进入二次操作模式
            StartSecondTargeting(slot.skillData);
        }
        else
        {
            // 直接施放
            CmdCastSkill(slot.skillData.skillID, Vector3.zero, Vector2.zero,0);
        }
    }

    [Client]
    private void StartSecondTargeting(SkillData skillData)
    {
        currentSkillState = SkillState.Targeting;
        isHolding = true;
        holdStartTime = Time.time;

        // 显示目标选择器
        skillUI.ShowTargetingUI(skillData);

        // 根据技能类型显示不同的指示器
        switch (skillData.targetingMode)
        {
            case TargetingMode.Direction:
                skillUI.ShowDirectionIndicator(skillData.range);
                break;
            case TargetingMode.Area:
                skillUI.ShowAreaIndicator(skillData.radius);
                break;
            case TargetingMode.Unit:
                skillUI.ShowUnitTargeting();
                break;
        }
    }

    [Client]
    private void OnConfirmTarget()
    {
        if (currentSkillState != SkillState.Targeting || currentCastingSkillId == -1) return;

        var skillData = GetSkillData(currentCastingSkillId);
        if (skillData == null) return;

        // 计算目标位置/方向
        Vector3 targetPosition = Vector3.zero;
        Vector2 direction = Vector2.zero;

        if (skillData.skillType == SkillType.Projectile)
        {
            // 投掷类：获取鼠标方向
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            direction = (mouseWorldPos - transform.position).normalized;
        }
        else if (skillData.skillType == SkillType.Dodge)
        {
            // 闪避类：获取闪避方向
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            direction = input.normalized;
        }

        float holdTime = Time.time - holdStartTime;
        holdTime = Mathf.Clamp(holdTime, skillData.minHoldTime, skillData.maxHoldTime);

        // 发送到服务器
        CmdCastSkill(currentCastingSkillId, targetPosition, direction, holdTime);

        // 结束二次操作
        EndSecondTargeting();
    }

    [Client]
    private void OnCancelTarget()
    {
        if (currentSkillState == SkillState.Targeting)
        {
            EndSecondTargeting();
            currentCastingSkillId = -1;
        }
    }

    [Client]
    private void EndSecondTargeting()
    {
        currentSkillState = SkillState.Ready;
        isHolding = false;
        skillUI.HideTargetingUI();
    }

    [Command]
    private void CmdCastSkill(int skillId, Vector3 targetPosition, Vector2 direction, float holdTime)
    {
        var skillData = GetSkillData(skillId);
        if (skillData == null) return;

        // 服务器验证
        if (!CanCastSkill(skillData)) return;

        CostPart.UseSkill(skillData.Cost);
        // 消耗资源

        // 创建技能实例
        var skillInstance = SkillFactory.CreateSkill(skillData, gameObject, CalculateDefaultTarget(gameObject, skillData));
        skillInstance.Initialize(skillData,netId, targetPosition, direction, holdTime);

        // 同步给所有客户端
        RpcPlaySkillEffects(skillId, transform.position, targetPosition, direction, holdTime);

        // 开始冷却
        StartCoroutine(StartCooldownRoutine(skillData));

        // 添加到活动技能列表
        uint skillNetId = skillInstance.GetComponent<NetworkIdentity>().netId;
        activeSkills[skillNetId] = skillInstance;
    }

    [ClientRpc]
    private void RpcPlaySkillEffects(int skillId, Vector3 casterPosition, Vector3 targetPosition,
                                     Vector2 direction, float holdTime)
    {
        // 如果不是本地玩家，播放技能效果
        if (!isLocalPlayer)
        {
            var skillData = GetSkillData(skillId);
            if (skillData != null)
            {
                //PlayVisualEffects(skillData, casterPosition, targetPosition, direction);
            }
        }
    }
    // 辅助方法：计算默认目标位置
    private Vector3 CalculateDefaultTarget(GameObject casterObj,SkillData skillData)
    {
        UnityEngine.Transform casterTransform = casterObj.transform;

        // 根据技能类型计算默认目标
        if (skillData != null)
        {
            switch (skillData.targetingMode)
            {
                case TargetingMode.Direction:
                    // 默认向前方释放
                    return casterTransform.position + casterTransform.right * skillData.range;

                case TargetingMode.Area:
                    // 默认在施法者脚下
                    return casterTransform.position;

                case TargetingMode.Unit:
                    // 尝试自动选择目标
                    return FindAutoTarget(casterTransform,skillData);

                default:
                    return casterTransform.position;
            }
        }

        return casterTransform.position + casterTransform.right * 5f;
    }
    private Vector3 FindAutoTarget(UnityEngine.Transform casterTransform,SkillData skillData)
    {
        // 简单的自动索敌逻辑
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            casterTransform.position,
            skillData.range,
            skillData.targetMask
        );

        if (colliders.Length > 0)
        {
            return colliders[0].transform.position;
        }

        return casterTransform.position + casterTransform.right * skillData.range;
    }
    private void Update()
    {
        if (!isLocalPlayer) return;

        UpdateSecondTargeting();
        //UpdateUI();
    }

    [Client]
    private void UpdateSecondTargeting()
    {
        if (currentSkillState != SkillState.Targeting) return;

        var skillData = GetSkillData(currentCastingSkillId);
        if (skillData == null) return;

        // 更新目标选择器位置
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        skillUI.UpdateTargetingPosition(mouseWorldPos);

        // 更新蓄力指示
        float holdTime = Time.time - holdStartTime;
        skillUI.UpdateHoldIndicator(holdTime / skillData.maxHoldTime);

        // 自动取消（超时）
        if (holdTime > skillData.maxHoldTime)
        {
            OnCancelTarget();
        }
    }

    [Server]
    private bool CanCastSkill(SkillData skillData)
    {
        // 检查冷却
        var slot = GetSkillSlot(skillData.skillID);
        if (slot == null || slot.cooldownRemaining > 0) return false;

        // 检查资源
        if (CostPart.currentCharges < slot.skillData.Cost) return false;

        // 检查施法条件
        if (skillData.requireLineOfSight)
        {
            // TODO: 检查视线
        }

        return true;
    }

    private IEnumerator StartCooldownRoutine(SkillData skillData)
    {
        var slot = GetSkillSlot(skillData.skillID);
        if (slot != null)
        {
            slot.cooldownRemaining = skillData.cooldown;

            while (slot.cooldownRemaining > 0)
            {
                slot.cooldownRemaining -= Time.deltaTime;
                yield return null;
            }

            slot.cooldownRemaining = 0;
        }
    }

    private SkillData GetSkillData(int skillId)
    {
        // 方案1：使用foreach循环
        foreach (SkillData data in allSkills)
        {
            if (data != null && data.skillID == skillId)
            {
                return data;
            }
        }

        return null;  // 没找到
    }
    private SkillSlot GetSkillSlot(int skillId)
    {
        return skillSlots.FirstOrDefault(slot => slot.skillData?.skillID == skillId);
    }
}

[System.Serializable]
public class SkillSlot
{
    public SkillData skillData;
    [SyncVar] public float cooldownRemaining = 0f;
    public KeyCode hotkey;
}