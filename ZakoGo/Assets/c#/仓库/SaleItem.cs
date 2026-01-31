using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaleItem : MonoBehaviour
{
    public GameObject saleTxt;
    public void OpenSaleMod()
    {
        if (GameNum.IsSaled)
        {
            GameNum.IsSaled = false;
            saleTxt.SetActive(false);
            gameObject.GetComponent<Image>().color = new Color32(245, 217, 179, 255);
        }
        else
        {
            GameNum.IsSaled = true;
            saleTxt.SetActive(true);
            gameObject.GetComponent<Image>().color = new Color32(255, 200, 140, 255);
        }
    }
}
