using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SkillComponent基类
public abstract class SkillComponent : MonoBehaviour
{
    protected SkillInstance skillInstance;
    protected SkillData skillData;

    public virtual void Initialize(SkillInstance instance, SkillData data)
    {
        skillInstance = instance;
        skillData = data;
    }

    // 服务器逻辑
    public abstract void ExecuteServer();

    // 客户端表现
    public abstract void ExecuteClient();

    // 二次操作相关
    public virtual void OnSecondInput(Vector3 target, Vector2 direction, float holdTime) { }
    public virtual void UpdateSecondTargeting(Vector3 currentTarget) { }
}

/*
// 召唤类组件
public class SummonComponent : SkillComponent
{
    [Header("召唤设置")]
    public GameObject summonPrefab;
    public int summonCount = 1;
    public float summonRadius = 2f;
    public float summonDuration = 30f;

    public override void ExecuteServer()
    {
        for (int i = 0; i < summonCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * summonRadius;
            Vector3 spawnPos = skillInstance.transform.position + (Vector3)offset;

            var summon = Instantiate(summonPrefab, spawnPos, Quaternion.identity);
            var networkSummon = summon.GetComponent<NetworkSummon>();

            if (networkSummon != null)
            {
                networkSummon.Initialize(skillInstance.CasterNetId, summonDuration);
            }

            NetworkServer.Spawn(summon);
        }
    }

    public override void ExecuteClient()
    {
        // 播放召唤特效
        PlaySummonEffects();
    }
}

// 投掷类组件（需要二次操作）
public class ProjectileComponent : SkillComponent
{
    [Header("投掷设置")]
    public GameObject projectilePrefab;
    public float baseSpeed = 10f;
    public float maxSpeed = 20f;
    public float minDamage = 10f;
    public float maxDamage = 30f;
    public bool homing = false;

    private Vector2 throwDirection;
    private float holdTime;

    public override void OnSecondInput(Vector3 target, Vector2 direction, float holdTime)
    {
        this.throwDirection = direction;
        this.holdTime = holdTime;
    }

    public override void ExecuteServer()
    {
        float speed = Mathf.Lerp(baseSpeed, maxSpeed, holdTime);
        float damage = Mathf.Lerp(minDamage, maxDamage, holdTime);

        var projectile = Instantiate(projectilePrefab,
            skillInstance.transform.position,
            Quaternion.identity);

        var netProjectile = projectile.GetComponent<NetworkProjectile>();
        netProjectile.Initialize(skillInstance.CasterNetId, throwDirection, speed, damage, homing);

        NetworkServer.Spawn(projectile);
    }

    public override void ExecuteClient()
    {
        // 客户端预测轨迹
        if (skillInstance.IsOwner)
        {
            DrawTrajectoryPrediction();
        }

        // 播放投掷动画
        PlayThrowAnimation();
    }

    public override void UpdateSecondTargeting(Vector3 currentTarget)
    {
        // 更新轨迹预测显示
        UpdateTrajectory(currentTarget);
    }
}

// 直接索敌类组件
public class TargetLockComponent : SkillComponent
{
    [Header("索敌设置")]
    public float lockRange = 15f;
    public int maxTargets = 1;
    public LayerMask targetLayer;
    public bool requireLineOfSight = true;

    private List<uint> lockedTargets = new List<uint>();

    public override void ExecuteServer()
    {
        var caster = skillInstance.Caster;
        if (caster == null) return;

        // 查找目标
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            caster.transform.position,
            lockRange,
            targetLayer);

        int targetsFound = 0;
        foreach (var collider in colliders)
        {
            if (targetsFound >= maxTargets) break;

            var networkIdentity = collider.GetComponent<NetworkIdentity>();
            if (networkIdentity != null && networkIdentity.netId != skillInstance.CasterNetId)
            {
                if (!requireLineOfSight || HasLineOfSight(caster.transform, collider.transform))
                {
                    lockedTargets.Add(networkIdentity.netId);
                    targetsFound++;

                    // 应用效果
                    ApplyEffectToTarget(networkIdentity.netId);
                }
            }
        }
    }

    private bool HasLineOfSight(Transform from, Transform to)
    {
        Vector2 direction = to.position - from.position;
        RaycastHit2D hit = Physics2D.Raycast(from.position, direction, direction.magnitude,
            LayerMask.GetMask("Obstacle"));

        return hit.collider == null;
    }
}


// 治疗类组件
public class HealComponent : SkillComponent
{
    [Header("治疗设置")]
    public float healAmount = 50f;
    public float healRadius = 5f;
    public bool includeSelf = true;
    public GameObject healEffect;

    public override void ExecuteServer()
    {
        var caster = skillInstance.Caster;
        if (caster == null) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            caster.transform.position,
            healRadius,
            LayerMask.GetMask("Player"));

        foreach (var collider in colliders)
        {
            var health = collider.GetComponent<Health>();
            if (health != null)
            {
                if (includeSelf || collider.gameObject != caster)
                {
                    health.Heal(healAmount);
                }
            }
        }
    }
}

// 护盾类组件
public class ShieldComponent : SkillComponent
{
    [Header("护盾设置")]
    public float shieldAmount = 100f;
    public float duration = 5f;
    public GameObject shieldEffect;

    public override void ExecuteServer()
    {
        var caster = skillInstance.Caster;
        if (caster == null) return;

        var shield = caster.GetComponent<PlayerShield>();
        if (shield != null)
        {
            shield.AddShield(shieldAmount, duration);
        }
    }
}

// 闪避类组件（需要二次操作）
public class DodgeComponent : SkillComponent
{
    [Header("闪避设置")]
    public float baseDistance = 3f;
    public float maxDistance = 6f;
    public float invincibilityDuration = 0.5f;
    public GameObject dodgeEffect;

    private Vector2 dodgeDirection;
    private float holdTime;

    public override void OnSecondInput(Vector3 target, Vector2 direction, float holdTime)
    {
        this.dodgeDirection = direction;
        this.holdTime = holdTime;
    }

    public override void ExecuteServer()
    {
        var caster = skillInstance.Caster;
        if (caster == null) return;

        float distance = Mathf.Lerp(baseDistance, maxDistance, holdTime);
        Vector2 dodgeVector = dodgeDirection * distance;

        // 服务器移动
        var movement = caster.GetComponent<CharacterMovement>();
        if (movement != null)
        {
            movement.ServerDodge(dodgeVector, invincibilityDuration);
        }
    }

    public override void ExecuteClient()
    {
        var caster = skillInstance.Caster;
        if (caster != null && caster == skillInstance.LocalPlayer)
        {
            // 本地预测闪避
            float distance = Mathf.Lerp(baseDistance, maxDistance, holdTime);
            Vector2 dodgeVector = dodgeDirection * distance;

            var movement = caster.GetComponent<CharacterMovement>();
            if (movement != null)
            {
                movement.DodgePredicted(dodgeVector);
            }

            // 播放特效
            if (dodgeEffect != null)
            {
                Instantiate(dodgeEffect, caster.transform.position, Quaternion.identity);
            }
        }
    }

    public override void UpdateSecondTargeting(Vector3 currentTarget)
    {
        // 更新闪避方向指示
        UpdateDodgeDirectionIndicator();
    }
    */
