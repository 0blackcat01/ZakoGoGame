using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New Gun", menuName = "Gun/New Gun")]
[System.Serializable]
public class Gun : ScriptableObject
{
    public Sprite GunImg;
    public Sprite GunChangeBulletImg;
    public Sprite BulletImg;
    public int BulletSpriteIndex;
    public AudioClip clip01;
    public AudioClip clip02;
    public bool Isbreak_audio = true;
    public int GunID;
    public string GunName;
    public int TolBulletNum;
    public int BulletNum;//一个弹夹里的现有子弹数
    public int BulletBoxNum;//弹夹子弹数
    public float reloadTime = 2f;            // 装弹时间
    public float BulletDieTime;//子弹消亡时间
    public float GunCritRate = 0;//枪械基础暴击率
    public float GunCritNum = 0;//枪械基础暴伤值
    public Vector3 GunAddPos;
    public Item Bullet;

    public float AttackNum;
    public float FireRate = 1.0f;
    public float GunRotation;//枪械稳定性(即随机弹道偏移)
    public float moveSpeedMultiplier = 1f;     // 移动速度倍率
    public float ScreenShakeTime = 0.3f;
    public float ScreenShakePower = 1.0f;
    public int PerParticleNum = 0;
    public int DurabilityNum = 100;//耐久

    public int ValueMoney;
    [TextArea]
    public string GunInfo;//枪械介绍

}
