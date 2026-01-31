using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class HiedUINet : NetworkBehaviour
{
    public GameObject UI;
    public override void OnStartLocalPlayer()
    {
        if(!isLocalPlayer)
        // 只有本地玩家才显示UI
        UI.gameObject.SetActive(false);
    }


}
