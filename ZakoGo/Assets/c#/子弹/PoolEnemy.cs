using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoolEnemy : NetworkBehaviour
{

    
    [System.Serializable]
    public class EnemyPool
    {
        public int EnemyID;
        public GameObject prefab;
        public int initialSize;
    }


    [SerializeField] private List<EnemyPool> enemyPools;
    private Dictionary<int, Queue<GameObject>> poolDictionary;


    public override void OnStartServer()
    {
        InitializeAllPools();
        Debug.Log($"2D区域管理器启动，配置了 {areas.Count} 个区域");
    }

    [Server]
    private void InitializeAllPools()
    {
        poolDictionary = new Dictionary<int, Queue<GameObject>>();

        foreach (var pool in enemyPools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = Instantiate(pool.prefab);            
                obj.SetActive(false);
                objectPool.Enqueue(obj);
                //NetworkServer.Spawn(obj);
            }
            poolDictionary.Add(pool.EnemyID, objectPool);
        }
    }

    [Server]
    public GameObject GetEnemy(int EnemyId, Vector2 position)
    {
        if (!poolDictionary.ContainsKey(EnemyId))
        {
            Debug.LogError($"未找到类型为 {EnemyId} 的对象池");
            return InstantiateNewEnemy(EnemyId, position);
        }

        Queue<GameObject> pool = poolDictionary[EnemyId];

        GameObject enemy = pool.Count > 0 ?
            pool.Dequeue() :
            InstantiateNewEnemy(EnemyId, position);

        enemy.transform.position = position;
        Debug.Log(enemy.transform.position);
        RegisterEnemyToArea(enemy, position);
        NetworkServer.Spawn(enemy);
        enemy.SetActive(true);

        return enemy;
    }


    [Server]
    private GameObject InstantiateNewEnemy(int EnemyId, Vector2 position)
    {
        GameObject prefab = GetPrefabByType(EnemyId);
        if (prefab == null)
        {
            Debug.LogError($"找不到类型 {EnemyId} 的预制体");
            return null;
        }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        RegisterEnemyToArea(enemy,position);
        enemy.name = $"{EnemyId}_NewInstance";
        NetworkServer.Spawn(enemy);
        return enemy;
    }
    [Server]
    public void ReturnEnemy(int EnemyId, GameObject enemy)
    {
        if (enemy == null) return;

        enemy.SetActive(false);

        if (poolDictionary.TryGetValue(EnemyId, out var pool))
        {
            pool.Enqueue(enemy);
            UnregisterEnemyFromArea(enemy);
            NetworkServer.UnSpawn(enemy);
            Debug.Log("???");
        }
        else
        {
            Debug.LogWarning($"返回的敌人类型 {EnemyId} 未注册，直接销毁");
            UnregisterEnemyFromArea(enemy);
            NetworkServer.Destroy(enemy);
            Debug.Log("??");
        }
    }

    private GameObject GetPrefabByType(int EnemyId)
    {
        foreach (var pool in enemyPools)
        {
            if (pool.EnemyID == EnemyId)
                return pool.prefab;
        }
        return null;
    }

    [Server]
    public void CleanupAllEnemies()
    {
        foreach (var pool in poolDictionary.Values)
        {
            while (pool.Count > 0)
            {
                GameObject enemy = pool.Dequeue();
                if (enemy != null)
                {
                    NetworkServer.Destroy(enemy);
                }
            }
        }
        poolDictionary.Clear();
    }
    #region 区域检测回收
    [System.Serializable]
    public class GameArea
    {
        [Header("区域名称")]
        public string areaName = "区域1";

        [Header("X轴范围")]
        public float centerX = 0f;
        public float width = 20f;

        [Header("Y轴范围")]
        public float centerY = 0f;
        public float height = 10f;

        [Header("检测设置")]
        public bool checkYAxis = true;  // 是否检测Y轴

        // 运行时数据
        [System.NonSerialized] public bool hasPlayer = false;
        [System.NonSerialized] public List<GameObject> enemies = new List<GameObject>();

        // 计算边界
        public float Left => centerX - width / 2;
        public float Right => centerX + width / 2;
        public float Bottom => centerY - height / 2;
        public float Top => centerY + height / 2;

        // 检测点是否在区域内
        public bool Contains(Vector2 point)
        {
            bool inX = point.x >= Left && point.x <= Right;

            if (!checkYAxis)
                return inX;  // 不检查Y轴，只检查X轴

            bool inY = point.y >= Bottom && point.y <= Top;
            return inX && inY;
        }

        // 绘制区域Gizmo
        public void DrawGizmo(Color color)
        {
            Gizmos.color = color;
            Vector3 center = new Vector3(centerX, centerY, 0);
            Vector3 size = new Vector3(width, height, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
    [Header("区域配置")]
    [SerializeField] private List<GameArea> areas = new List<GameArea>();

    [Header("检测设置")]
    [SerializeField] private float checkInterval = 3f;
    [SerializeField] private bool drawGizmos = true;

    private float timer = 0f;


    [Server]
    private void Update()
    {
        if (!isServer) return;

        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            CheckAllAreas();
            timer = 0f;
        }
    }

    [Server]
    private void CheckAllAreas()
    {
        // 1. 重置所有区域状态
        foreach (var area in areas)
        {
            area.hasPlayer = false;
        }

        // 2. 检测玩家位置
        CheckPlayersInAreas();

        // 3. 处理空区域
        ProcessEmptyAreas();
    }

    [Server]
    private void CheckPlayersInAreas()
    {
        var allPlayers = GameObject.FindGameObjectsWithTag("Player");

        foreach (var player in allPlayers)
        {

            Vector2 playerPos = player.transform.position;

            // 检查玩家在哪个区域
            foreach (var area in areas)
            {
                if (area.Contains(playerPos))
                {
                    area.hasPlayer = true;
                    break; // 一个玩家只在一个区域
                }
            }
        }
    }

    [Server]
    private void ProcessEmptyAreas()
    {
        foreach (var area in areas)
        {
            if (!area.hasPlayer)
            {
                RecycleEnemiesInArea(area);
            }
        }
    }

    [Server]
    private void RecycleEnemiesInArea(GameArea area)
    {
        if (area.enemies.Count == 0) return;

        int count = area.enemies.Count;
        Debug.Log($"区域 '{area.areaName}' 没有玩家，回收 {count} 个敌人");

        foreach (var enemy in area.enemies.ToArray())
        {
            if (enemy != null)
            {
                ReturnEnemy(enemy.GetComponent<CharacterNum>().EnemyID, enemy);
            }
        }

        area.enemies.Clear();
    }


    [Server]
    private void RegisterEnemyToArea(GameObject enemy, Vector3 position)
    {
        GameArea area = FindAreaByPosition(position);
        if (area != null && !area.enemies.Contains(enemy))
        {
            area.enemies.Add(enemy);
        }
    }

    [Server]
    private void UnregisterEnemyFromArea(GameObject enemy)
    {
        // 简单方法：遍历所有区域，移除这个敌人
        foreach (var area in areas)
        {
            if (area.enemies.Remove(enemy))
            {
                break; // 找到一个就停止
            }
        }
    }

    [Server]
    private GameArea FindAreaByPosition(Vector3 position)
    {
        Vector2 pos2D = position;

        foreach (var area in areas)
        {
            if (area.Contains(pos2D))
            {
                return area;
            }
        }

        return null;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos || areas == null) return;

        foreach (var area in areas)
        {
            Color color = area.hasPlayer ? Color.green : Color.red;
            Gizmos.color = color;

            Vector3 center = new Vector3(area.centerX, area.centerY, 0);
            Vector3 size = new Vector3(area.width, area.height, 0);
            Gizmos.DrawWireCube(center, size);

            // 显示区域名称和敌人数
            Vector3 labelPos = new Vector3(area.centerX, area.Top + 1, 0);
            UnityEditor.Handles.Label(labelPos, $"{area.areaName}\n敌人:{area.enemies.Count}");
        }
    }
#endif
    #endregion
}
