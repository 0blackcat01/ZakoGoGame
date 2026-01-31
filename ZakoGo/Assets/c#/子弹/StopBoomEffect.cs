using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopBoomEffect : NetworkBehaviour
{
    // Start is called before the first frame update
    public void StopAnim()
    {
        NetworkServer.UnSpawn(gameObject);
    }
}
