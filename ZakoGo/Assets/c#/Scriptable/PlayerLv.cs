using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Lvs", menuName = "Character/New Lvs")]
[System.Serializable]
public class PlayerLv : ScriptableObject
{
    public List<NumUp> lvs;
}
[System.Serializable]
public class NumUp
{
    [Header("Ã·…˝ Ù–‘")]
    public string Name;
    public int Hp;
    public float Denfense;
    public float MoveSpeed;
    public float CritRate;
    public float SkillNum;
    public float CostRestoreNum;

}
