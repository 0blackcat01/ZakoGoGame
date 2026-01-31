using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum BulletType
{
    子弹,
    榴弹
}

[System.Serializable]
public class BulletPoolConfig
{
    public BulletType type;
    public GameObject prefab;
    public int poolSize = 20;
}

public class PoolBulletNet : NetworkBehaviour
{
    public static PoolBulletNet Instance { get; private set; }

    [Header("子弹池配置")]
    [SerializeField] private List<BulletPoolConfig> bulletConfigs = new List<BulletPoolConfig>();

    private Dictionary<BulletType, Queue<GameObject>> bulletPools = new Dictionary<BulletType, Queue<GameObject>>();
    private Dictionary<BulletType, GameObject> bulletPrefabs = new Dictionary<BulletType, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void OnStartServer()
    {
        InitializeAllPools();
    }

    [Server]
    private void InitializeAllPools()
    {
        foreach (var config in bulletConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning($"子弹类型 {config.type} 的预制体未设置");
                continue;
            }

            bulletPrefabs[config.type] = config.prefab;
            var queue = new Queue<GameObject>();

            for (int i = 0; i < config.poolSize; i++)
            {
                GameObject bullet = Instantiate(config.prefab, transform);
                bullet.SetActive(false);
                ResetBullet(bullet);
                //NetworkServer.Spawn(bullet);
                queue.Enqueue(bullet);
            }

            bulletPools[config.type] = queue;
            Debug.Log($"初始化子弹池: {config.type}, 数量: {config.poolSize}");
        }
    }

    [Server]
    public GameObject GetBullet(BulletType type)
    {
        if (!bulletPools.ContainsKey(type))
        {
            Debug.LogError($"未找到子弹类型: {type}");
            return CreateNewBullet(type);
        }

        var pool = bulletPools[type];

        if (pool.Count > 0)
        {
            GameObject bullet = pool.Dequeue();

            return bullet;
        }

        // 池子空了就新建一个并动态扩展池子
        Debug.Log($"子弹池 {type} 已空，动态扩展");
        return CreateNewBullet(type);
    }

    [Server]
    private GameObject CreateNewBullet(BulletType type)
    {
        if (!bulletPrefabs.ContainsKey(type))
        {
            Debug.LogError($"未找到子弹类型 {type} 的预制体");
            return null;
        }

        GameObject newBullet = Instantiate(bulletPrefabs[type], transform);
        NetworkServer.Spawn(newBullet);
        return newBullet;
    }

    [Server]
    public void ReturnBullet(BulletType type, GameObject bullet)
    {
        if (bullet == null) return;

        if (bulletPools.ContainsKey(type))
        {
            ResetBullet(bullet);

            bulletPools[type].Enqueue(bullet);
        }
        else
        {
            Debug.LogWarning($"返回的子弹类型 {type} 未在池中注册，直接销毁");
            NetworkServer.Destroy(bullet);
        }
    }

    [Server]
    private void ResetBullet(GameObject bullet)
    {
        
        bullet.transform.SetParent(transform);
        bullet.transform.position = new Vector3(0,0,0);
        bullet.transform.rotation = Quaternion.identity;
        bullet.transform.localScale = Vector3.one;

        // 重置物理状态（如果有）
        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        NetworkServer.UnSpawn(bullet);
        bullet.SetActive(false);
        
    }




    [Server]
    public int GetPoolCount(BulletType type)
    {
        return bulletPools.ContainsKey(type) ? bulletPools[type].Count : 0;
    }

    [Server]
    public void ExpandPool(BulletType type, int additionalSize)
    {
        if (!bulletPrefabs.ContainsKey(type))
        {
            Debug.LogError($"无法扩展未注册的子弹类型: {type}");
            return;
        }

        if (!bulletPools.ContainsKey(type))
        {
            bulletPools[type] = new Queue<GameObject>();
        }

        for (int i = 0; i < additionalSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefabs[type], transform);
            bullet.SetActive(false);
            NetworkServer.Spawn(bullet);
            bulletPools[type].Enqueue(bullet);
        }

        Debug.Log($"扩展子弹池 {type}: +{additionalSize}");
    }

    // 清理所有子弹池（换场景时调用）
    [Server]
    public void ClearAllPools()
    {
        foreach (var pool in bulletPools.Values)
        {
            while (pool.Count > 0)
            {
                GameObject bullet = pool.Dequeue();
                if (bullet != null)
                    NetworkServer.Destroy(bullet);
            }
        }

        bulletPools.Clear();
        Debug.Log("清空所有子弹池");
    }
}