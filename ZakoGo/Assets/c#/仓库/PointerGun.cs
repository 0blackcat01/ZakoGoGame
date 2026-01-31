using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PointerGun : MonoBehaviour, IPointerClickHandler
{
    public GunList MyGun;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerEnter == null) return;
        GameObject obj = eventData.pointerEnter.gameObject;
        if(!obj.CompareTag("BoxGrid")) return;
        int index = MyGun.gunList.IndexOf(obj.GetComponent<BoxGrid>().gun);
        if(index == 0) return;
        MyGun.gunList[index] = MyGun.gunList[0];
        MyGun.gunList[0] = obj.GetComponent<BoxGrid>().gun;
        GameObject.FindGameObjectWithTag("BagUI").GetComponent<BagUIShow>().UpdateGun();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControl>().RefreshGunNum();
    }
}
