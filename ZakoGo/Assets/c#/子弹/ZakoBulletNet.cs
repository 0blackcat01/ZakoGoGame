using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
public class ZakoBulletNet : NetworkBehaviour
{
    private Vector3 direction;  // 子弹的方向
    [Header("子弹属性")]
    [SyncVar] public float speed = 10f;
    [SyncVar] public float lifetime = 3f;
    [SyncVar] public int damage = 10;
    [SyncVar] public float critRate = 5;
    [SyncVar] public float critNum = 1;
    [SyncVar] public bool isPlayerBullet = false;
    [SyncVar] public bool IsInBlock = false;
    [SyncVar] public uint playerID;

    private float spawnTime;
    public List<Sprite> BulletSprites;

    [SyncVar(hook = nameof(OnSpriteIndexChanged))]
    private int spriteIndex;

    [Server]
    public void SetDirection(bool IsLeft)
    {


        if (IsLeft)
        {
            if (transform.rotation.z <= -90)
            {
                direction = -transform.right;
            }
            else
            {
                direction = transform.right;
            }


        }
        else
        {
            direction = transform.right;

        }
        gameObject.transform.localScale = new Vector3(1, 1, 1);


    }
    [Server]
    public void SetPlayerDirection(bool IsLeft)
    {
        if (IsLeft)
        {
            direction = -transform.right;
            gameObject.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            direction = transform.right;
            gameObject.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    [Server]
    public void Initialize(float bulletLifetime, int bulletDamage, bool playerBullet,
        Quaternion pelletrotation,Vector3 Pos,int Spriteindex,bool Isinblock, float critrate,float critnum, uint playerId)
    {
        //Debug.Log(pelletrotation);
        lifetime = bulletLifetime;
        damage = bulletDamage;
        isPlayerBullet = playerBullet;
        spawnTime = Time.time;
        gameObject.transform.rotation = pelletrotation;
        gameObject.transform.position = Pos;
        // 设置SyncVar，会自动同步到所有客户端
        spriteIndex = Spriteindex;
        IsInBlock = Isinblock;
        critRate = critrate;
        critNum = critnum;
        playerID = playerId;
    }
    // SyncVar变化时的回调
    private void OnSpriteIndexChanged(int oldIndex, int newIndex)
    {
        if (newIndex >= 0 && newIndex < BulletSprites.Count)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = BulletSprites[newIndex];
        }
    }


    private void Update()
    {
        // 移动逻辑在所有客户端执行
        transform.Translate(direction * speed * Time.deltaTime);

        // 服务器处理生命周期
        if (isServer && Time.time - spawnTime > lifetime)
        {
            PoolBulletNet.Instance.ReturnBullet(BulletType.子弹,gameObject);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPlayerBullet && other.CompareTag("Enemy"))
        {
            other.GetComponent<CharacterNum>().Hurt(damage,critRate,critNum,playerID);
            PoolBulletNet.Instance.ReturnBullet(BulletType.子弹, gameObject);
        }
        else if (!isPlayerBullet && other.CompareTag("Player"))
        {
            other.GetComponent<CharacterNum>().Hurt(damage, critRate, critNum,0);
            PoolBulletNet.Instance.ReturnBullet(BulletType.子弹, gameObject);
        }
        else if(!other.CompareTag("Player") && !other.CompareTag("Enemy"))
        {
            if (other.gameObject.CompareTag("Block"))
            {
                if (!IsInBlock)
                {
                    other.GetComponent<CharacterNum>().Hurt(damage, critRate, critNum, 0);
                    PoolBulletNet.Instance.ReturnBullet(BulletType.子弹, gameObject);
                }
                
            }
            else
            {
                PoolBulletNet.Instance.ReturnBullet(BulletType.子弹, gameObject);
            }
            
        }

        
    }

}
