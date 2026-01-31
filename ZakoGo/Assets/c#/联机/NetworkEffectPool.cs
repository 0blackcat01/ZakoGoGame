using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkEffectPool : NetworkBehaviour
{
    [System.Serializable]
    public class EffectPool
    {
        public string effectID;          // 特效ID
        public GameObject prefab;        // 特效预制体
        public int initialSize = 10;     // 初始池大小
        public float defaultLifetime = 2f; // 默认生命周期
        public bool autoReturnToPool = true; // 是否自动回收
    }

    [Header("特效池配置")]
    [SerializeField] private List<EffectPool> effectPools = new List<EffectPool>();

    [Header("性能设置")]
    [SerializeField] private bool spawnOnStart = true;  // 启动时生成
    [SerializeField] private int maxTotalEffects = 100; // 最大特效数量

    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, EffectPool> effectConfigDict;
    private Dictionary<GameObject, string> effectTypeDict;  // 记录特效类型

    public static NetworkEffectPool Instance { get; private set; }

    // 特效数据类
    public class EffectData
    {
        public string effectID;
        public Vector3 position;
        public Quaternion rotation;
        public Transform parent = null;
        public Color? color = null;
        public float scale = 1f;
    }

    public override void OnStartServer()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (spawnOnStart)
        {
            InitializeAllPools();
        }
    }

    [Server]
    public void InitializeAllPools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        effectConfigDict = new Dictionary<string, EffectPool>();
        effectTypeDict = new Dictionary<GameObject, string>();

        foreach (var pool in effectPools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning($"特效 {pool.effectID} 的预制体为空，跳过");
                continue;
            }

            // 检查是否已存在
            if (poolDictionary.ContainsKey(pool.effectID))
            {
                Debug.LogError($"重复的特效ID: {pool.effectID}");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();
            poolDictionary.Add(pool.effectID, objectPool);
            effectConfigDict.Add(pool.effectID, pool);

            // 初始化对象池
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject effect = CreateEffect(pool.effectID);
                if (effect != null)
                {
                    effect.SetActive(false);
                    objectPool.Enqueue(effect);
                }
            }

            Debug.Log($"初始化特效池: {pool.effectID}, 大小: {pool.initialSize}");
        }
    }

    [Server]
    private GameObject CreateEffect(string effectID)
    {
        if (!effectConfigDict.TryGetValue(effectID, out EffectPool config))
        {
            Debug.LogError($"未找到特效配置: {effectID}");
            return null;
        }

        GameObject effect = Instantiate(config.prefab);
        effect.SetActive(false);
        effect.name = $"{effectID}_Pooled";

        // 添加网络标识
        NetworkServer.Spawn(effect);

        // 记录特效类型
        effectTypeDict[effect] = effectID;

        return effect;
    }

    [Server]
    public GameObject GetEffect(EffectData data)
    {
        return GetEffect(data.effectID, data.position, data.rotation,
                        data.parent, data.color, data.scale);
    }

    [Server]
    public GameObject GetEffect(string effectID, Vector3 position,
                               Quaternion rotation = default,
                               Transform parent = null,
                               Color? color = null,
                               float scale = 1f)
    {
        if (!poolDictionary.TryGetValue(effectID, out Queue<GameObject> pool))
        {
            Debug.LogError($"未找到类型为 {effectID} 的特效池");
            return null;
        }

        GameObject effect = null;

        // 从池中获取
        if (pool.Count > 0)
        {
            effect = pool.Dequeue();
        }
        else
        {
            // 池为空，创建新的
            effect = CreateEffect(effectID);
            if (effect == null) return null;
        }

        // 设置特效属性
        SetupEffect(effect, effectID, position, rotation, parent, color, scale);

        // 激活并同步
        effect.SetActive(true);

        // 自动回收设置
        if (effectConfigDict.TryGetValue(effectID, out EffectPool config) &&
            config.autoReturnToPool)
        {
            StartCoroutine(AutoReturnEffect(effect, config.defaultLifetime));
        }

        return effect;
    }

    [Server]
    private void SetupEffect(GameObject effect, string effectID,
                            Vector3 position, Quaternion rotation,
                            Transform parent, 
                            Color? color, float scale)
    {
        effect.transform.position = position;
        effect.transform.rotation = rotation;

        if (parent != null)
        {
            effect.transform.SetParent(parent, true);
        }

        effect.transform.localScale = Vector3.one * scale;


    }

    [Server]
    public void ReturnEffect(GameObject effect)
    {
        if (effect == null || !effectTypeDict.TryGetValue(effect, out string effectID))
        {
            Debug.LogWarning("尝试回收未注册的特效，直接销毁");
            NetworkServer.Destroy(effect);
            return;
        }

        // 停用特效
        effect.SetActive(false);

        // 重置父对象
        effect.transform.SetParent(null);

        // 重置位置
        effect.transform.position = Vector3.zero;


        // 回收到对象池
        if (poolDictionary.TryGetValue(effectID, out Queue<GameObject> pool))
        {
            pool.Enqueue(effect);
            Debug.Log($"特效 {effectID} 已回收到对象池，当前数量: {pool.Count}");
        }
        else
        {
            Debug.LogWarning($"特效 {effectID} 的对象池不存在，直接销毁");
            NetworkServer.Destroy(effect);
        }
    }

    [Server]
    private System.Collections.IEnumerator AutoReturnEffect(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnEffect(effect);
    }

    [Server]
    public void PrewarmEffect(string effectID, int count)
    {
        if (!poolDictionary.TryGetValue(effectID, out Queue<GameObject> pool))
        {
            Debug.LogError($"未找到特效池: {effectID}");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject effect = CreateEffect(effectID);
            if (effect != null)
            {
                pool.Enqueue(effect);
            }
        }

        Debug.Log($"预生成 {count} 个 {effectID} 特效");
    }

    [Server]
    public void CleanupAllEffects()
    {
        foreach (var kvp in poolDictionary)
        {
            while (kvp.Value.Count > 0)
            {
                GameObject effect = kvp.Value.Dequeue();
                if (effect != null)
                {
                    NetworkServer.Destroy(effect);
                }
            }
        }

        poolDictionary.Clear();
        effectTypeDict.Clear();

        Debug.Log("已清理所有特效");
    }

    [Server]
    public int GetPoolSize(string effectID)
    {
        if (poolDictionary.TryGetValue(effectID, out Queue<GameObject> pool))
        {
            return pool.Count;
        }
        return 0;
    }

    [Server]
    public List<string> GetAllEffectIDs()
    {
        return new List<string>(poolDictionary.Keys);
    }
}