using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TolResourcePoint : NetworkBehaviour
{
    [Header("全局资源列表")]
    public ItemLists globalResourceList;
    //public List<ItemList> globalResourceList; // 全局资源列表
    public ItemList AllglobalResource;

    public float DelayTime = 20000f;
    public static TolResourcePoint Instance { get; private set; }
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

        Debug.Log("服务器资源系统初始化启动");


        InitializeResources();
        InvokeRepeating("RefreshResources", 1f, DelayTime);
        InstanceTest();
    }

    #region 资源管理逻辑
    [Server]
    private void InitializeResources()
    {
        for (int i = 0; i < globalResourceList.itemLists.Count; i++)
        {
            globalResourceList.itemLists[i].items.Clear();
            globalResourceList.itemLists[i].OtherNums.Clear();
        }
    }

    [Server]
    public void RefreshResources()
    {
        if (AllglobalResource == null || AllglobalResource.items.Count == 0)
        {
            Debug.LogError("基础资源库为空!");
            return;
        }

        // 清空所有资源列表
        for (int i = 0; i < globalResourceList.itemLists.Count; i++)
        {
            globalResourceList.itemLists[i].items.Clear();
            globalResourceList.itemLists[i].OtherNums.Clear();
        }

        // 改进的随机生成逻辑
        for (int i = 0; i < globalResourceList.itemLists.Count; i++)
        {
            var resourceList = globalResourceList.itemLists[i];
            int itemsToGenerate = UnityEngine.Random.Range(1, 4);

            HashSet<int> generatedIndices = new HashSet<int>();
            List<Item> CopyList = new List<Item>();

            for (int j = 0; j < AllglobalResource.items.Count; j++)
            {
                if (AllglobalResource.items[j].FindPoint.Contains(resourceList.FindPoint))
                {
                    CopyList.Add(AllglobalResource.items[j]);
                }
            }

            for (int j = 0; j < itemsToGenerate && j < CopyList.Count; j++)
            {
                int randomIndex;
                do
                {
                    randomIndex = UnityEngine.Random.Range(0, CopyList.Count);
                } while (generatedIndices.Contains(randomIndex));

                generatedIndices.Add(randomIndex);
                if (UnityEngine.Random.Range(1, 100) <= CopyList[randomIndex].FindRate)
                {
                    resourceList.OtherNums.Add(1);
                    resourceList.items.Add(CopyList[randomIndex]);
                }
            }

            SyncResourcePointToClients(i);
        }
    }

    [Server]
    private void SyncResourcePointToClients(int boxIndex)
    {
        var resourceList = globalResourceList.itemLists[boxIndex];
        ushort[] itemIds = new ushort[resourceList.items.Count];

        for (int i = 0; i < resourceList.items.Count; i++)
        {
            itemIds[i] = (ushort)resourceList.items[i].ItemId;
        }

        RpcSyncResourcePoint(boxIndex, itemIds);
    }

    [ClientRpc]
    private void RpcSyncResourcePoint(int boxIndex, ushort[] itemIds)
    {

        var clientList = globalResourceList.itemLists[boxIndex].items;
        var clientOtherNumList = globalResourceList.itemLists[boxIndex].OtherNums;

        clientList.Clear();
        clientOtherNumList.Clear();

        foreach (var itemId in itemIds)
        {
            Item configItem = AllglobalResource.items.Find(x => x.ItemId == itemId);
            if (configItem != null)
            {
                clientList.Add(configItem);
                clientOtherNumList.Add(1);
            }
        }

        //Debug.Log($"资源点 {boxIndex} 已刷新!");
    }

    [Server]
    public void SyncResourcesToNewPlayer(NetworkConnectionToClient conn)
    {


        // 验证资源列表初始化状态
        if (globalResourceList == null || globalResourceList.itemLists.Count == 0)
        {
            Debug.LogError("全局资源列表未初始化");
            return;
        }

        // 验证基础资源库
        if (AllglobalResource == null || AllglobalResource.items == null)
        {
            Debug.LogError("基础资源库未配置");
            return;
        }

        // 分块同步资源点
        for (int i = 0; i < globalResourceList.itemLists.Count; i++)
        {
            // 检查资源点有效性
            if (globalResourceList.itemLists[i] == null)
            {
                Debug.LogWarning($"资源点 {i} 未初始化，跳过同步");
                continue;
            }

            // 准备传输数据
            ushort[] itemIds = new ushort[globalResourceList.itemLists[i].items.Count];
            for (int j = 0; j < globalResourceList.itemLists[i].items.Count; j++)
            {
                if (globalResourceList.itemLists[i].items[j] == null)
                {
                    Debug.LogWarning($"资源点 {i} 的物品 {j} 为null");
                    continue;
                }
                itemIds[j] = (ushort)globalResourceList.itemLists[i].items[j].ItemId;
            }

            // 同步单个资源点
            TargetSyncResourcePoint(conn, i, itemIds);
        }

    }

    [TargetRpc]
    private void TargetSyncResourcePoint(NetworkConnectionToClient target, int boxIndex, ushort[] itemIds)
    {
        // 1. 检查基础资源库是否初始化
        if (AllglobalResource == null || AllglobalResource.items == null)
        {
            Debug.LogError("基础资源库未初始化!");
            return;
        }

        // 2. 检查资源点索引范围
        if (boxIndex < 0 || boxIndex >= globalResourceList.itemLists.Count)
        {
            Debug.LogWarning($"无效的资源点索引: {boxIndex}");
            return;
        }



        // 4. 检查itemIds数组
        if (itemIds == null)
        {
            Debug.LogWarning("收到空的物品ID数组");
            return;
        }

        // 5. 获取目标资源点
        var resourcePoint = globalResourceList.itemLists[boxIndex];
        if (resourcePoint == null || resourcePoint.items == null || resourcePoint.OtherNums == null)
        {
            Debug.LogError($"资源点 {boxIndex} 未正确初始化");
            return;
        }



        // 清空现有数据
        resourcePoint.items.Clear();
        resourcePoint.OtherNums.Clear();

        // 填充新数据
        foreach (var itemId in itemIds)
        {
            Item configItem = AllglobalResource.items.Find(x => x.ItemId == itemId);
            if (configItem != null)
            {
                resourcePoint.items.Add(configItem);
                resourcePoint.OtherNums.Add(1);
            }
            else
            {
                Debug.LogWarning($"未找到物品配置: {itemId}");
            }
        }

        Debug.Log($"资源点 {boxIndex} 同步完成，物品数量: {resourcePoint.items.Count}");
    }
    #endregion

    #region 物品交互逻辑
    [Command(requiresAuthority = false)]
    public void CmdUpdateSearchUI(int itemID, int count, int index, uint playerID, int BoxIndex)
    {
        // 验证BoxIndex范围
        if (BoxIndex < 0 || BoxIndex >= globalResourceList.itemLists.Count)
        {
            Debug.LogWarning($"无效的BoxIndex: {BoxIndex}");
            return;
        }

        var resourceBox = globalResourceList.itemLists[BoxIndex];

        // 验证index范围
        if (index < 0 || index >= resourceBox.items.Count)
        {
            Debug.LogWarning($"无效的index: {index} (当前items数量: {resourceBox.items.Count})");
            return;
        }

        // 验证物品存在
        Item item = resourceBox.items.Find(i => i.ItemId == itemID);
        if (item == null)
        {
            Debug.LogWarning($"未找到物品ID: {itemID}");
            return;
        }

        // 执行安全移除
        resourceBox.items.RemoveAt(index);
        resourceBox.OtherNums.RemoveAt(index);

        SyncResourcePointToClients(BoxIndex);

        // 通知特定玩家
        NetworkIdentity playerIdentity;
        if (NetworkServer.spawned.TryGetValue(playerID, out playerIdentity))
        {
            TargetGiveItemToPlayer(playerIdentity.connectionToClient, (ushort)itemID, (byte)count,BoxIndex);
        }
        else
        {
            Debug.LogWarning($"找不到玩家 {playerID} 的网络身份");
        }
    }

    [TargetRpc]
    private void TargetGiveItemToPlayer(NetworkConnectionToClient target, ushort itemId, byte count,int BoxIndex)
    {
        GameObject localPlayer = NetworkClient.localPlayer?.gameObject;
        if (localPlayer == null) return;

        // 更新UI
        var searchUIContent = localPlayer.GetComponent<NetPlayerControl>().SearchUIConten;
        if (searchUIContent != null && searchUIContent.activeSelf)
        {
            searchUIContent.GetComponent<SearchShowUI>()?.UpdateBoxItem(globalResourceList.itemLists[BoxIndex]);
        }

        // 添加到背包
        localPlayer.GetComponent<NetPlayerControl>()?.UpdateBagItem(itemId, count, NetworkClient.localPlayer.netId);
    }
    #endregion

}