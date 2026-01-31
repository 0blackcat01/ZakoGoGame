using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BoomBullet : NetworkBehaviour
{
    [Header("榴弹设置")]
    [SerializeField] private float gravity = 9.8f;
    [SerializeField] private GameObject explosionPrefab;

    [SyncVar] private float lifetime;
    [SyncVar] private int damage;
    [SyncVar] private bool isPlayerBullet;
    private Vector2 velocity;
    private float gravityScale;
    private float explosionRadius;

    [Server]
    public void InitializeGrenade(float lifetime, int damage, bool isPlayerBullet,
                                Vector2 initialVelocity, Vector2 spawnPosition,
                                float gravityScale, float explosionRadius)
    {
        this.lifetime = lifetime;
        this.damage = damage;
        this.isPlayerBullet = isPlayerBullet;
        this.velocity = initialVelocity;
        this.gravityScale = gravityScale;
        this.explosionRadius = explosionRadius;

        transform.position = spawnPosition;
        Invoke(nameof(Explode), lifetime);
    }

    private void Update()
    {

        // 应用重力
        velocity += Vector2.down * gravity * gravityScale * Time.deltaTime;

        // 更新位置
        transform.position += (Vector3)velocity * Time.deltaTime;
    }


    [Server]
    private void Explode()
    {
        // 生成爆炸效果
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(explosion);

        // 范围伤害检测
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if(hit.TryGetComponent(out CharacterNum chara))
            {
                chara.Hurt(damage,0,0,0);
            }
        }

        // 销毁榴弹
        PoolBulletNet.Instance.ReturnBullet(BulletType.榴弹,gameObject);
    }
}
