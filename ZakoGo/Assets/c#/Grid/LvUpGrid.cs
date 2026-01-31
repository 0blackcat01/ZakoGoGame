using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LvUpGrid : MonoBehaviour
{
    public TextMeshProUGUI LvUpName;
    public TextMeshProUGUI Addtion;
    public int index;
    public void MakeChoice()
    {
        GameObject.FindGameObjectWithTag("LvUpUI").GetComponent<ShowCharacterNum>().AddCharacterNum(index);
    }
}
