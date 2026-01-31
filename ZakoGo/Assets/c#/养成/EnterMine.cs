using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterMine : NetworkBehaviour
{
    public int Index = -1;
    public int SignIndex = -1;
    [SyncVar] private bool serverOnce = false;
    [SerializeField] private float renewTime = 60f;

    private void Start()
    {
        if (isServer)
        {
            serverOnce = true;
            InvokeRepeating(nameof(RenewServerOnce), 1f, renewTime);
        }
    }
    [Server]
    private void RenewServerOnce()
    {
        serverOnce = true;
    }
    [Server]
    public void FalseServerOnce()
    {
        serverOnce = false;

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(!serverOnce) return;
        if (!collision.CompareTag("Player")) return;

        var playerControl = collision.GetComponent<NetPlayerControl>();
        if (playerControl == null || !playerControl.isLocalPlayer) return;
        playerControl.OpenMineButton(Index,gameObject.transform.position, SignIndex);

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        
        if (!other.CompareTag("Player")) return;

        var playerControl = other.GetComponent<NetPlayerControl>();
        if (playerControl != null && playerControl.isLocalPlayer && serverOnce)
        {
            playerControl.CloseWhichButton();
            playerControl.StopLoading();
        }
    }
}
