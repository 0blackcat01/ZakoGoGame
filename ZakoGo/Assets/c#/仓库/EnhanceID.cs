using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceID : MonoBehaviour
{
    // Start is called before the first frame update
    public int index;
    public int ThingGridIndex;
    public TextMeshProUGUI TxtNum;
    public Image Img;
    public EnhanceList MyEnhanceList;

    public void InitGrid()
    {
        TxtNum.text = "0";
        Img.sprite = null;
        Img.color = new Color32(255,255, 255,0);  
    }
    public void ShowGrid(Item item,int index)
    {

        TxtNum.text = MyEnhanceList.tiles[index].Num.ToString();
        Img.sprite = item.ItemImg;
        Img.color = new Color32(255, 255, 255, 255);
    }
}
