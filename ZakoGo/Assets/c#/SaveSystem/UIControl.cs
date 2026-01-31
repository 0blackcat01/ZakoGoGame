using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIControl : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject MakingTableUI;
    public GameObject KitchenUI;
    public GameObject EnhanceUI;
    public GameObject TipUI;
    public void OpenUI()
    {
        GameNum.IsOpenUI = true;
        gameObject.GetComponent<CanvasGroup>().interactable = false;
        GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
    }
    public void CloseUI()
    {
        GameNum.IsOpenUI = false;
        gameObject.GetComponent<CanvasGroup>().interactable = true;
    }
    public void OpenMakingUI()
    {
        if(BuildingLevelUp.MakingDurability <= 0)
        {
            TipUI.gameObject.SetActive(true);
        }
        else
        {
            MakingTableUI.gameObject.SetActive(true);
        }
        
    }
    public void OpenKitchenUI()
    {
        if (BuildingLevelUp.KitchenDurability <= 0)
        {
            TipUI.gameObject.SetActive(true);
        }
        else
        {
            KitchenUI.gameObject.SetActive(true);
        }
        
    }
    public void OpenEnhanceUI()
    {
        if (BuildingLevelUp.EnhanceDurability <= 0)
        {
            TipUI.gameObject.SetActive(true);
        }
        else
        {
            EnhanceUI.gameObject.SetActive(true);
        }
        
    }
}
