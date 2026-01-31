using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppStart : MonoBehaviour
{
    public bool IsServer = false;
    private NetworkManager MyNet;
    private void Start()
    {
        MyNet = GetComponent<NetworkManager>();
        if (IsServer)
        {
            MyNet.networkAddress = GameNum.Sercer_Ip;
            MyNet.StartServer();
            GameNum.IsSinglePlayer = false;
        }

    }
    //  nohup ./ZakoGo.x86_64 -batchmode -nographics > server.log 2>&1 &

}
