using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Character",menuName = "Character/New Character")]
[System.Serializable]

public class Character : ScriptableObject
{
    public int NpcID;
    public int HpMax;
    public int Attack;
    public int Denfense;
    public int WalkSpeed;
    public int LvNum;
    [Header("枪械属性")]
    public float FireRate = 1.0f;//发射子弹时间间隔
    public float GunRotation;//枪械稳定性(即随机弹道偏移)
    [Header("战利箱")]
    public GameObject WinBoxPrefab;
}
