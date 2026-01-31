using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("基础信息")]
    public int skillID;
    public string skillName;
    public Sprite icon;
    public SkillType skillType;

    [Header("消耗属性")]
    public float cooldown = 5f;
    public int Cost = 20;
    public float castTime = 0f; //前摇

    [Header("目标选择")]
    public TargetingMode targetingMode;
    public float range = 10f;
    public float radius = 2f;
    public LayerMask targetMask;
    public bool requireLineOfSight = true;

    [Header("技能组件配置")]
    public List<SkillComponentConfig> components = new List<SkillComponentConfig>();

    [Header("视觉效果")]
    public GameObject castEffect;
    public GameObject impactEffect;
    public AudioClip castSound;
    public AudioClip impactSound;
    public string animationName;

    [Header("二次操作配置（投掷/闪避类）")]
    public bool requireSecondInput = false;
    public float maxHoldTime = 2f;        // 最大蓄力时间
    public float minHoldTime = 0.5f;      // 最小蓄力时间
    public bool showTrajectory = true;    // 显示轨迹预测
}

[System.Serializable]
public class SkillComponentConfig
{
    public string componentName;  // 对应的组件类名
    public SkillParam[] parameters;
}

[System.Serializable]
public class SkillParam
{
    public string key;
    public string value;
    public ParamType type;
}

public enum ParamType
{
    Int,
    Float,
    String,
    Bool,
    GameObject
}
public enum SkillType
{
    Summon,         // 召唤类
    Projectile,     // 投掷类（需要二次操作）
    TargetLock,     // 直接索敌类
    Buff,           // 强化类
    Heal,           // 治疗类
    Shield,         // 护盾类
    Dodge           // 闪避类（需要二次操作）
}

public enum TargetingMode
{
    None,           // 无需目标（如Buff）
    AutoTarget,     // 自动索敌
    Direction,      // 方向选择
    Area,           // 区域选择
    Unit            // 单位选择
}

public enum SkillState
{
    Ready,
    Casting,
    Targeting,      // 二次操作状态
    Channeling,
    Cooldown
}