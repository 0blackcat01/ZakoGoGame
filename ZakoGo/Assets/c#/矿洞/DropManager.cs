using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DropManager : NetworkBehaviour
{
    public static DropManager Instance { get; private set; }
    private void InstanceTest()
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
        if (!isServer) return;
        base.OnStartServer();
        InstanceTest();
    }
    [System.Serializable]
    public class DropItem
    {
        public GameObject prefab;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [SerializeField] private DropItem[] dropTable;

    [Server]
    public void SpawnDrops(Vector3 position, int index)
    {
        int amount = Random.Range(dropTable[index].minAmount, dropTable[index].maxAmount + 1);
        for (int i = 0; i < amount; i++)
        {
            Vector2 dropPos = position + Random.insideUnitSphere * 1.5f;
            GameObject drop = Instantiate(dropTable[index].prefab, dropPos, Quaternion.identity);
            NetworkServer.Spawn(drop);

            // 如果需要，可以添加掉落动画或效果
            //StartCoroutine(AnimateDrop(drop));
        }

    }


    private IEnumerator AnimateDrop(GameObject drop)
    {
        // 掉落动画逻辑
        yield break;
    }
    #region 矿石开采
    [SerializeField] private List<GameObject> MineLists;
    [Command(requiresAuthority = false)]
    public void CmdSignMine(int index)
    {
        ServerSignMine(index);
    }
    [Server]
    public void ServerSignMine(int index)
    {
        MineLists[index].gameObject.GetComponent<EnterMine>().FalseServerOnce();
    }
    #endregion
}
