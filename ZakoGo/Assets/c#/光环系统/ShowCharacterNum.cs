using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ShowCharacterNum : MonoBehaviour
{
    public TextMeshProUGUI LevelTxt;
    public TextMeshProUGUI LevelNumTxt;
    public TextMeshProUGUI HpTxt;
    public TextMeshProUGUI AttackTxt;
    public TextMeshProUGUI DenfenseTxt;
    public TextMeshProUGUI CritRateTxt;
    public TextMeshProUGUI CritNumTxt;
    public TextMeshProUGUI HatedNumTxt;
    public TextMeshProUGUI LuckyNumTxt;
    public TextMeshProUGUI MoveSpeedTxt;
    public TextMeshProUGUI CostRestoreTxt;
    public TextMeshProUGUI CostNumTxt;


    public GunList MyGunList;
    private void OnEnable()
    {
        GameNum.LevelNum = 20;
        UpdateNumShow();

    }
    public void UpdateNumShow()
    {

        LevelNumTxt.text = GameNum.LevelNum + "/" + LvUpNeed[GameNum.Level];

        LevelTxt.text = "等级：" + GameNum.Level;
        HpTxt.text = "血量：" + GameNum.HpMax;
        AttackTxt.text = "攻击力：" + MyGunList.gunList[0].AttackNum;
        DenfenseTxt.text = "防御力：" + (GameNum.DenfenseNum + GameNum.DenfenseAddtion);
        CritRateTxt.text = "暴击率：" + (GameNum.CritRateAddtion + GameNum.CritRateAddtion + MyGunList.gunList[0].GunCritRate) + "%";
        CritNumTxt.text = "暴击伤害：" + MyGunList.gunList[0].GunCritNum;
        HatedNumTxt.text = "仇恨值：" + GameNum.HateAddtion;
        LuckyNumTxt.text = "幸运值：" + GameNum.LuckyAddtion;
        MoveSpeedTxt.text = "移速：" + (GameNum.MoveSpeed + GameNum.MoveSpeedAddtion);
        CostRestoreTxt.text = "Cost恢复：" + (GameNum.CostRestore + GameNum.CostRestoreAddtion);
        CostNumTxt.text = "光环能量:" + (GameNum.CostTolNum + GameNum.CostTolNumAddtion);
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            Destroy(panel.transform.GetChild(i).gameObject);
        }
        panel.gameObject.GetComponent<Image>().raycastTarget = false;
    }
    [Header("等级提升")]
    public List<PlayerLv> PlayerLvs;
    public List<int> LvUpNeed;
    private PlayerLv lv;
    public void LevelUp()
    {
        CheckLv();
        panel.gameObject.GetComponent<Image>().raycastTarget = true;
        for (int i = 0; i < LvUpNeed.Count; i++)
        {
            if(GameNum.Level == i)
            {
                if(GameNum.LevelNum >= LvUpNeed[i])
                {
                    GameNum.LevelNum -= LvUpNeed[i];
                    GameNum.Level += 1;
                    ShowUpUI();
                    break;
                }
            }
        }
        
    }
    public void ShowUpUI()
    {
        // 洗牌算法实现
        List<int> numbers = new List<int> { 0 ,1, 2 ,3 ,4 };

        // 洗牌
        for (int i = numbers.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int temp = numbers[i];
            numbers[i] = numbers[j];
            numbers[j] = temp;
        }
        for (int i = 0; i < panel.transform.childCount; i++)
        {
            Destroy(panel.transform.GetChild(i));
        }
        for (int i = 0;i < 3; i++)
        {
            InsertGird(numbers[i]);
        }
    }
    public LvUpGrid lvUpGrid;
    public GameObject panel;
    public void CheckLv()
    {
        if (GameNum.Level < 10)
        {
            lv = PlayerLvs[0];
        }
        else if (GameNum.Level >= 10 && GameNum.Level < 20)
        {
            lv = PlayerLvs[1];
        }
        else if (GameNum.Level >= 20 && GameNum.Level < 30)
        {
            lv = PlayerLvs[2];
        }
    }
    public void InsertGird(int index)
    {
        Debug.Log(lv.name);
        LvUpGrid grid = Instantiate(lvUpGrid, panel.transform);
        grid.LvUpName.text = lv.lvs[index].Name.ToString();
        if(index == 0)
        {
            grid.LvUpName.color = new Color32(255,80,80,255);
        }
        else if(index == 1)
        {
            grid.LvUpName.color = new Color32(176, 176, 176, 255);
        }
        else if( index == 2)
        {
            grid.LvUpName.color = new Color32(0, 250, 15, 255);
        }
        else if(index == 3)
        {
            grid.LvUpName.color = new Color32(255, 0, 168, 255);
        }
        else if(index == 4)
        {
            grid.LvUpName.color = new Color32(155, 0, 255, 255);
        }
        else if(index == 5)
        {
            grid.LvUpName.color = new Color32(0, 140, 255, 255);
        }
        grid.index = index;

        if(lv.lvs[index].Hp != 0)
        {
            grid.Addtion.text += "+" + lv.lvs[index].Hp + "生命值";
        }
        if (lv.lvs[index].Denfense != 0)
        {
            if (grid.Addtion.text != null)
            {
                grid.Addtion.text += System.Environment.NewLine;
            }
            grid.Addtion.text += "+" + lv.lvs[index].Denfense + "防御值";
            
        }
        if (lv.lvs[index].MoveSpeed != 0)
        {
            if (grid.Addtion.text != null)
            {
                grid.Addtion.text += System.Environment.NewLine;
            }
            grid.Addtion.text = "+" + lv.lvs[index].MoveSpeed + "移速";
        }
        if (lv.lvs[index].CritRate != 0)
        {
            if (grid.Addtion.text != null)
            {
                grid.Addtion.text += System.Environment.NewLine;
            }
            grid.Addtion.text = "+" + lv.lvs[index].CritRate + "暴击率";
        }
        if (lv.lvs[index].SkillNum != 0)
        {
            if (grid.Addtion.text != null)
            {
                grid.Addtion.text += System.Environment.NewLine;
            }
            grid.Addtion.text = "+" + lv.lvs[index].SkillNum + "光环能量";
        }
        if (lv.lvs[index].CostRestoreNum != 0)
        {
            if (grid.Addtion.text != null)
            {
                grid.Addtion.text += System.Environment.NewLine;
            }
            grid.Addtion.text = "+" + lv.lvs[index].CostRestoreNum + "Cost恢复";
        }


    }
    public void AddCharacterNum(int index)
    {

        if (lv.lvs[index].Hp != 0)
        {
            GameNum.HpMax += lv.lvs[index].Hp;
        }
        if (lv.lvs[index].Denfense != 0)
        {
            GameNum.DenfenseNum += lv.lvs[index].Denfense;
        }
        if (lv.lvs[index].MoveSpeed != 0)
        {
            GameNum.MoveSpeed += lv.lvs[index].MoveSpeed;
        }
        if (lv.lvs[index].CritRate != 0)
        {
            GameNum.CritRateNum += lv.lvs[index].CritRate;
        }
        if (lv.lvs[index].SkillNum != 0)
        {
            GameNum.CostTolNum += lv.lvs[index].SkillNum;
        }
        if (lv.lvs[index].CostRestoreNum != 0)
        {
            GameNum.CostRestore += lv.lvs[index].CostRestoreNum;
        }
        UpdateNumShow();
    }
}
