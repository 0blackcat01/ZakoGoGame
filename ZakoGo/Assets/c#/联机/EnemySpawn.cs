using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : NetworkBehaviour
{
    public List<Character> characters = new List<Character>();
    public List<int> EnenmyNums = new List<int>();
    [SerializeField] private float renewTime = 120f;
    [SerializeField] private Transform spawnPoint; // 敌人生成点
    [SyncVar] private bool serverOnce = false;

    private void Start()
    {
        if (isServer)
        {
            serverOnce = true;
            InvokeRepeating(nameof(RenewServerOnce), renewTime, renewTime);
        }
    }
    [Server]
    private void RenewServerOnce()
    {
        serverOnce = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        var playerControl = collision.GetComponent<NetPlayerControl>();
        if (playerControl == null || !playerControl.isLocalPlayer) return;

        if (serverOnce)
        {
            CmdRequestSpawnEnemy();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var playerControl = other.GetComponent<NetPlayerControl>();

    }

    [Command(requiresAuthority = false)]
    private void CmdRequestSpawnEnemy()
    {
        if (!serverOnce) return;

        serverOnce = false;
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        for (int i = 0; i < characters.Count; i++)
        {
            for (int j = 0; j < EnenmyNums[i]; j++)
            {
                GameObject enemy = GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolEnemy>()
                    .GetEnemy(characters[i].NpcID, spawnPosition);
            }

        }



    }
}
