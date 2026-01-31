using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;


public class EnhanceItem : MonoBehaviour,IPointerClickHandler
{


    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.pointerEnter == null) return;
        if (eventData.pointerEnter.CompareTag("BagGrid"))
        {
            if(gameObject.GetComponent<BoxGrid>().item.BoxItemNum <= 0) return;
            GameObject.FindGameObjectWithTag("EnhanceUI").GetComponent<EnhanceUIShow>().Update_EnhanceUI(gameObject.GetComponent<BoxGrid>().item,gameObject);

        }
    }
}
