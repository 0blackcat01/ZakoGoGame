using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.VisualScripting;

public class GetWinBox : NetworkBehaviour
{
    public Item WinboxItem;
    [SyncVar]
    private bool isCollected = false;
    public float DestoryTime = 60f;
    private void Start()
    {
        if(!isServer) return;
        Invoke("ServerDestroyBox", DestoryTime);
    }
    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCollected || !isServer) return;
        if (collision == null) return;
        if (collision.gameObject.CompareTag("Player"))
        {
            uint playerID = collision.gameObject.GetComponent<NetworkIdentity>().netId;
            NetworkIdentity playerIdentity;
            if (NetworkServer.spawned.TryGetValue(playerID, out playerIdentity))
            {
                TargetGetWinBox(playerIdentity.connectionToClient, (ushort)WinboxItem.ItemId, (byte)1);
            }
            Debug.Log("Winbox");
            Invoke("ServerDestroyBox", 0.2f);

        }
    }
    [TargetRpc]
    private void TargetGetWinBox(NetworkConnectionToClient target, ushort itemId, byte count)
    {
        GameObject localPlayer = NetworkClient.localPlayer?.gameObject;
        if (localPlayer == null) return;
        if(localPlayer.GetComponent<NetPlayerControl>()?.MyBag.items.Count >= GameNum.BagCount) return;
        isCollected = true;
        // Ìí¼Óµ½±³°ü
        localPlayer.GetComponent<NetPlayerControl>()?.UpdateBagItem(itemId, count, NetworkClient.localPlayer.netId);
    }
    [Server]
    private void ServerDestroyBox()
    {
        NetworkServer.Destroy(gameObject);
    }

}
