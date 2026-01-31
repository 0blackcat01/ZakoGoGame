using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoxGrid : MonoBehaviour
{
    public int index;
    public Image ItemColorImage;
    public Image ItemImage;
    public TextMeshProUGUI ItemNumTxt;
    public TextMeshProUGUI ItemNameTxt;
    public TextMeshProUGUI ItemValueTxt;
    public Item item;
    public Gun gun;

    [Header("食物配方")]
    public KitchenRecipe recipe;
    public void ChangeItemColor(ItemValueType type)
    {
        if(type == ItemValueType.劣质)
        {
            ItemColorImage.color = new Color32(120, 120, 120, 65);
        }
        else if(type == ItemValueType.普通)
        {
            ItemColorImage.color = new Color32(255, 255, 255, 65);
        }
        else if(type == ItemValueType.精良)
        {
            ItemColorImage.color = new Color32(0, 255, 60, 65);
        }
        else if(type == ItemValueType.优质)
        {
            ItemColorImage.color = new Color32(0, 136, 255, 65);

        }
        else if(type == ItemValueType.史诗)
        {
            ItemColorImage.color = new Color32(210, 0, 255, 65);
            
        }
        else if(type == ItemValueType.传说)
        {
            ItemColorImage.color = new Color32(255, 150, 0, 65);
        }
        else
        {
            ItemColorImage.color = new Color32(0,0, 0, 0);
        }
    }

}
