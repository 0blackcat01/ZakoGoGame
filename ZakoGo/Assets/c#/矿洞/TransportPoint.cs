using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportPoint : NetworkBehaviour
{
    [Header("传送设置")]
    [SerializeField] private Vector3 targetTeleportPosition;  // 目标位置
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isServer) return;
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                // 获取玩家网络标识
                NetworkIdentity playerIdentity = collision.GetComponent<NetworkIdentity>();
                if (playerIdentity == null) return;
                TeleportPlayer(playerIdentity);
            }
        }
    }

    [Server]
    private void TeleportPlayer(NetworkIdentity playerIdentity)
    {
        if (targetTeleportPosition == null)
        {
            Debug.LogWarning("没有设置目标传送位置！");
            return;
        }

        // 传送玩家
        playerIdentity.transform.position = targetTeleportPosition;

    }

}
