using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickKitchenGrid : MonoBehaviour,IPointerClickHandler
{
    public GameObject Conten;
    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerEnter != null)
        {
            if (eventData.pointerEnter.CompareTag("StoreGrid"))
            {
                Conten.GetComponent<ShowKitchenGrid>().HarvesFood(
                    eventData.pointerEnter.gameObject.GetComponent<EnhanceID>().index);
            }
        }
    }


}
