using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEnter : MonoBehaviour
{
    public GameObject MyNet;
    public GameObject TipUI;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                if (!GameNum.IsSinglePlayer)
                {
                    if(!collision.GetComponent<NetPlayerControl>().isLocalPlayer) return;
                }
                TipUI.SetActive(true);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.CompareTag("Player"))
            {
                TipUI.SetActive(false);
            }
        }
    }
    public void EnterGame()
    {
        MyNet.GetComponent<NetworkManager>().networkAddress = GameNum.Sercer_Ip;
        GameNum.IsSinglePlayer = false;
        MyNet.GetComponent<NetworkManager>().StartClient();
    }
    public void ExitGame()
    {
        if(!NetworkClient.localPlayer.isLocalPlayer) return ;
        MyNet = GameObject.FindGameObjectWithTag("MyNet");
        GameNum.IsSinglePlayer = true;
        MyNet.GetComponent<NetworkManager>().StopClient();
    }
}
