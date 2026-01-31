using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZakoBoom : EnemyAI
{
    // Start is called before the first frame update
    protected override void Update()
    {
        base.Update();
    }
    protected override void Awake()
    {
        base.Awake();
    }
    [Header("步枪设置")]
    [SerializeField] private float spreadAngle = 1f;     // 散布角度
    [SerializeField] private Transform gunBarrel;          // 枪口位置
    [SerializeField] private GameObject pelletPrefab;      // 弹丸预制体
    [SerializeField] private float BulletDieTime = 4f;   // 子弹存续时间

    private float lastAttackTime;

    protected override void UpdateAttackState()
    {
        if (!isServer) return;
        base.UpdateAttackState();

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            ShootShotgun();
            lastAttackTime = Time.time;
        }
    }

    private void ShootShotgun()
    {
        Vector2 toPlayer = (player.position - gunBarrel.position).normalized;
        Cmdshoot(toPlayer);
    }
    [Server]
    private void Cmdshoot(Vector2 mousePosition)
    {
        if (!isServer) return;
        FirePellet(mousePosition);
    }
    [Server]
    private void FirePellet(Vector2 baseDirection)
    {
        GameObject bullet = GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolBulletNet>().GetBullet(BulletType.子弹);
        //audio0.Play();
        if (bullet == null)
        {
            Debug.LogWarning("子弹池已空，无法获取子弹");
            return;
        }

        // 获取枪械的当前Z轴旋转角度（以度为单位）
        float gunRotationZ = Gun.rotation.eulerAngles.z;

        // 计算随机散布角度（基于枪械当前朝向）
        float randomSpread = Random.Range(-spreadAngle, spreadAngle);

        // 计算最终角度（枪械旋转 + 随机散布）
        float finalAngle = gunRotationZ + randomSpread;

        // 创建旋转四元数
        Quaternion pelletRotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
        Vector2 pelletDirection = pelletRotation * Vector2.right;

        bool isPlayerFacingRight = gameObject.transform.localScale.x > 0;
        bullet.GetComponent<ZakoBulletNet>().SetDirection(isPlayerFacingRight);

        bullet.GetComponent<ZakoBulletNet>().Initialize(
            bulletLifetime: BulletDieTime,
            bulletDamage: 10,
            playerBullet: false,
            pelletrotation: pelletRotation,
            Pos: Gun.transform.position,
            Spriteindex: 0,
            Isinblock: m_CharacterNum.IsInBlcok,
            critrate: 1,
            critnum: 1,
            playerId: 0
         );
        NetworkServer.Spawn(bullet);


    }
}
