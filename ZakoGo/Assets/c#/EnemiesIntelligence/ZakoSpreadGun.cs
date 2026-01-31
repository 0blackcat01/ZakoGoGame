using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZakoSpreadGun : EnemyAI
{
    protected override void Update()
    {
        base.Update();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    [Header("射击模式")]
    [SerializeField] private FireMode fireMode = FireMode.Shotgun;  // 射击模式
    [SerializeField] private int burstCount = 3;                     // 连发次数
    [SerializeField] private float burstInterval = 0.1f;            // 连发间隔

    [Header("霰弹枪设置")]
    [SerializeField] private int pelletCount = 6;          // 弹丸数量
    [SerializeField] private float spreadAngle = 30f;     // 散布角度
    [SerializeField] private Transform gunBarrel;          // 枪口位置
    [SerializeField] private GameObject pelletPrefab;      // 弹丸预制体
    [SerializeField] private float BulletDieTime = 3f;   // 子弹存续时间
    
    private float lastTargetUpdateTime = 0;
    [Header("延时索敌")]
    [SerializeField] private float targetUpdateInterval = 0;//索敌间隔
    private Vector2 toPlayer;

    // 连发相关变量
    private int currentBurstCount = 0;
    private float lastBurstTime = 0f;
    private float lastAttackTime = 0f;
    private bool isBursting = false;

    // 射击模式枚举
    public enum FireMode
    {
        Shotgun,    // 霰弹模式
        Burst       // 连发模式
    }

    protected override void UpdateAttackState()
    {
        base.UpdateAttackState();

        // 如果在连发中，处理连发逻辑
        if (isBursting)
        {
            HandleBurst();
            return;
        }

        // 检查冷却时间
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            switch (fireMode)
            {
                case FireMode.Shotgun:
                    ShootShotgun();
                    lastAttackTime = Time.time;
                    break;

                case FireMode.Burst:
                    StartBurst();
                    break;
            }
        }
    }

    // 开始连发
    private void StartBurst()
    {
        isBursting = true;
        currentBurstCount = 0;
        lastBurstTime = Time.time;
        lastAttackTime = Time.time; // 重置攻击冷却时间

        // 发射第一发
        FireSingleShot();
        currentBurstCount++;
    }

    // 处理连发逻辑
    private void HandleBurst()
    {
        if (currentBurstCount >= burstCount)
        {
            isBursting = false;
            return;
        }

        if (Time.time - lastBurstTime >= burstInterval)
        {
            FireSingleShot();
            currentBurstCount++;
            lastBurstTime = Time.time;

            // 如果连发结束，重置状态
            if (currentBurstCount >= burstCount)
            {
                isBursting = false;
            }
        }
    }

    // 发射单发散弹
    private void ShootShotgun()
    {      
        if (Time.time - lastTargetUpdateTime >= targetUpdateInterval)
        {
            toPlayer = (player.position - gunBarrel.position).normalized;
            lastTargetUpdateTime = Time.time;
        }
        audio0.Play();

        // 发射多发弹丸
        for (int i = 0; i < pelletCount; i++)
        {
            CmdShoot(toPlayer, i);
        }
    }

    // 发射单发子弹（用于连发模式）
    private void FireSingleShot()
    {
        Vector2 toPlayer = (player.position - gunBarrel.position).normalized;
        audio0.Play();
        CmdShoot(toPlayer, 0);
    }

    [Server]
    private void CmdShoot(Vector2 mousePosition, int k)
    {
        if (!isServer) return;
        FirePellet(mousePosition, k);
    }

    [Server]
    private void FirePellet(Vector2 baseDirection, int k)
    {
        GameObject bullet = PoolBulletNet.Instance.GetBullet(BulletType.子弹);

        if (bullet == null)
        {
            Debug.LogWarning("子弹池已空，无法获取子弹");
            return;
        }

        float gunRotationZ = Gun.rotation.eulerAngles.z;
        float randomSpread = Random.Range(-spreadAngle, spreadAngle);
        float finalAngle = gunRotationZ + randomSpread;
        Quaternion pelletRotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);

        bool isPlayerFacingRight = gameObject.transform.localScale.x > 0;

        bullet.GetComponent<ZakoBulletNet>().SetDirection(isPlayerFacingRight);

        bullet.GetComponent<ZakoBulletNet>().Initialize(
            bulletLifetime: BulletDieTime,
            bulletDamage: AttackNum,
            playerBullet: false,
            pelletrotation: pelletRotation,
            Pos: Gun.transform.position,
            Spriteindex: 0,
            Isinblock: m_CharacterNum.IsInBlcok,
            critrate: 1,
            critnum: 1,
            playerId: 0
         );

        bullet.gameObject.SetActive(true);
    }


}
