using DG.Tweening;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class CharacterNum : NetworkBehaviour
{
    public Character MyCharacterNum;
    public float Hpmax;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float Hp;
    public float Defense;
    public TextMeshProUGUI HpTxt;
    private bool IsDead = false;
    public float HatredNum = 0; //玩家特有
    private int LvNum = 0;

    [Header("私有Buff数据（不同步）")]
    private List<BuffInfo> activeBuffs = new List<BuffInfo>();
    private Dictionary<StatType, float> buffAdditions = new Dictionary<StatType, float>();

    public bool IsInBlcok = false;
    public int EnemyID = -1;
    private NetPlayerControl PlayerControl;
    private void OnEnable()
    {
        if (!gameObject.CompareTag("Player"))
        {
            Hpmax = MyCharacterNum.HpMax;
            Hp = MyCharacterNum.HpMax;
            Defense = MyCharacterNum.Denfense;
            LvNum = MyCharacterNum.LvNum;
            EnemyID = MyCharacterNum.NpcID;

        }
        else
        {
            Hpmax = 100 + GameNum.HpAddtion;
            Hp = Hpmax;
            HpTxt.text = Hp.ToString();
            Defense = GameNum.DenfenseNum + GameNum.DenfenseAddtion;
            HatredNum = 0 + GameNum.HateAddtion;
            
        }
        IsDead = false;
        
        HurtNumCanvas = GameObject.FindGameObjectWithTag("HurtNumCanvas").transform;
        HurtNumpool = HurtNumCanvas.gameObject.GetComponent<PoolBullet>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalScale = transform.localScale;
        PlayerControl = gameObject.GetComponent<NetPlayerControl>();
        //GetComponent<SpriteRenderer>().material = Instantiate(mt);

    }
    public void ResetHp()
    {
        Hp = Hpmax;
    }

    [Server]
    public void Hurt(int AttackNumFromAttacker,float critRate,float critNum,uint PlayerID)
    {
        if (!isServer) return; // 仅服务器能修改血量
        int minDamage = Mathf.Max(1, AttackNumFromAttacker / 10);
        float defenseFactor = Mathf.Clamp01(Defense / (Defense + 100f)); // 防御越高收益递减
        int CritRandomNum = UnityEngine.Random.Range(1,101);
        float AttackAddtion = 0;
        if (CritRandomNum <= critRate)
        {
            AttackAddtion = critNum;
        }
        else
        {
            AttackAddtion = 0;
        }       
        int finalDamage = Mathf.FloorToInt(AttackNumFromAttacker * (1f - defenseFactor) + AttackAddtion);
        Hp -= Mathf.Max(minDamage, finalDamage);

        RpcPlayHurtEffects(Mathf.Max(minDamage, finalDamage));
        if (Hp <= 0)
        {
            Die(PlayerID);
        }
    }
    [ClientRpc]
    private void RpcPlayHurtEffects(int damage)
    {
        if (!isServer) // 只在客户端执行
        {
            ShowDamageText(damage, 0);
            Flash(flashColor);

                
        }
    }
    [ClientRpc]
    private void RpcAddHpEffects(int num)
    {
        if (!isServer) // 只在客户端执行
        {

            ShowDamageText(num,1);
            Flash(Color.green);
            HpTxt.text = Hp.ToString();
        }
    }

    [Server]
    public void AddHP(int AddtionNum)
    {
        if (!isServer) return;
        Hp += AddtionNum;
        if(Hp > 100)
        {
            Hp =100;
        }
        RpcAddHpEffects(AddtionNum);

    }
    [Server]
    public void Die(uint PlayerID)
    {
        if (!isServer) return;
        if (gameObject.CompareTag("Enemy"))
        {
            IsDead = true;
            NetworkIdentity playerIdentity;
            if (NetworkServer.spawned.TryGetValue(PlayerID, out playerIdentity))
            {
                TargetGiveItemToPlayer(playerIdentity.connectionToClient, LvNum);
            }
            GameObject WinboxNet = Instantiate(MyCharacterNum.WinBoxPrefab,gameObject.transform.position,Quaternion.identity);
            NetworkServer.Spawn(WinboxNet);
            GameObject.FindGameObjectWithTag("Pool").GetComponent<PoolEnemy>().ReturnEnemy(EnemyID, gameObject);
            
        }
        else if (gameObject.CompareTag("Player"))
        {
            
        }
        else if (gameObject.CompareTag("Block"))
        {
            RoadBlock roadBlock = gameObject.GetComponent<RoadBlock>();
            if(roadBlock != null)
            {
                roadBlock.RefreshBlock0();
            }
        }
    }
    private void OnDestroy()
    {
        // 确保网络销毁时动画也被终止
        if (flashSequence != null && flashSequence.IsActive())
        {
            flashSequence.Kill();
        }
    }
    // 显示伤害数字
    public GameObject damageTextPrefab; // 伤害数字预制体
    public Transform damageTextSpawnPoint; // 伤害数字生成位置（可选）
    private Transform HurtNumCanvas;
    private PoolBullet HurtNumpool;
    private void ShowDamageText(int damage,int colorNum)
    {
        if (damageTextPrefab == null) return;

        Vector3 worldPosition = transform.position;

        // 3D 世界坐标 → 屏幕坐标
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        // 实例化伤害数字

        GameObject damageText = HurtNumpool.GetBullet();
        if(colorNum == 0)
        {
            damageText.GetComponent<TextMeshProUGUI>().color = Color.red;
        }
        else
        {
            damageText.GetComponent<TextMeshProUGUI>().color = Color.green;
        }
        damageText.transform.SetParent(HurtNumCanvas, false);
        damageText.transform.position = screenPosition;
        TextMeshProUGUI textMesh = damageText.GetComponent<TextMeshProUGUI>();

        if (textMesh != null)
        {
            textMesh.text = damage.ToString();

            // 使用DOTween实现动画效果
            Sequence sequence = DOTween.Sequence();

            // 初始设置
            damageText.transform.localScale = Vector3.one * 0.5f;
            Color originalColor = textMesh.color;
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);

            // 动画序列
            sequence.Append(damageText.transform.DOMoveY(screenPosition.y + 150f, 0.5f).SetEase(Ease.OutQuad));
            sequence.Join(damageText.transform.DOScale(Vector3.one * 1.2f, 0.3f).SetEase(Ease.OutBack));
            sequence.Join(textMesh.DOFade(1f, 0.2f));

            // 稍后缩小并淡出
            sequence.AppendInterval(0.2f);
            sequence.Append(damageText.transform.DOScale(Vector3.one * 0.8f, 0.3f));
            sequence.Join(textMesh.DOFade(0f, 0.3f));

            // 动画完成后销毁对象
            sequence.OnComplete(() => HurtNumpool.ReturnBullet(damageText)) ;
        }
    }
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private Color flashColor = new Color(1, 0.3f, 0.3f, 1);
    private Color originalColor;
    private Vector3 originalScale;
    private Sequence flashSequence;
    private SpriteRenderer spriteRenderer; 
    public void Flash(Color color)
    {
        
        // 如果已有动画在运行，先杀死它
        if (flashSequence != null && flashSequence.IsActive())
        {
            flashSequence.Kill();
            ResetToOriginalState();
        }

        flashSequence = DOTween.Sequence();

        // 变红
        flashSequence.Append(spriteRenderer.DOColor(color, flashDuration * 0.3f));

        // 恢复
        flashSequence.Append(spriteRenderer.DOColor(originalColor, flashDuration * 0.7f));

        // 可选：添加一些缩放效果增强打击感
        flashSequence.Join(transform.DOPunchScale(new Vector3(0.05f, 0.05f, 0), flashDuration, 5, 0.5f));

    }
    private void ResetToOriginalState()
    {
        transform.localScale = originalScale;
    }
    private void OnHealthChanged(float oldValue, float newValue)
    {
        // 客户端血量变化时的反馈（如UI更新、受击特效）

        if (gameObject.CompareTag("Player"))
        {
            HpTxt.text = Hp.ToString();
        }

        Debug.Log($"血量变化: {oldValue} -> {newValue}");
    }

    [TargetRpc]
    private void TargetGiveItemToPlayer(NetworkConnectionToClient target, int Levelnum0)
    {
        GameObject localPlayer = NetworkClient.localPlayer?.gameObject;
        if (localPlayer == null) return;

        GameNum.LevelNum += Levelnum0;
        Debug.Log(GameNum.LevelNum);
    }

    private void InitializeBuffAdditions()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            buffAdditions[type] = 0f;
        }
    }
    [Server]
    private void UpdateBuffs()
    {
        // 检查过期的Buff
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];
            if (Time.time - buff.startTime >= buff.duration)
            {
                RemoveBuffAt(i);
            }
        }
    }
    [Server]
    public void AddBuff(BuffInfo buff)
    {
        buff.startTime = Time.time;

        // 检查是否可叠加
        int existingIndex = -1;
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            if (activeBuffs[i].type == buff.type &&
                activeBuffs[i].sourceSkillId == buff.sourceSkillId)
            {
                existingIndex = i;
                break;
            }
        }

        if (existingIndex >= 0 && buff.stackType != BuffStackType.None)
        {
            // 叠加Buff
            var existingBuff = activeBuffs[existingIndex];
            existingBuff.value += buff.value;
            existingBuff.duration = Mathf.Max(existingBuff.duration, buff.duration);
            existingBuff.startTime = Time.time;
            activeBuffs[existingIndex] = existingBuff;

            // 重新计算加成
            buffAdditions[buff.type] = CalculateTotalBuffValue(buff.type);
        }
        else
        {
            // 添加新Buff
            activeBuffs.Add(buff);
            buffAdditions[buff.type] += buff.value;
        }

        // 应用Buff效果
        ApplyBuffEffect(buff);

        //Debug.Log($"[服务器] {gameObject.name} 获得 {buff.type} +{buff.value}");
    }

    [Server]
    private float CalculateTotalBuffValue(StatType type)
    {
        float total = 0f;
        foreach (var buff in activeBuffs)
        {
            if (buff.type == type)
            {
                total += buff.value;
            }
        }
        return total;
    }

    [Server]
    private void RemoveBuffAt(int index)
    {
        if (index >= 0 && index < activeBuffs.Count)
        {
            var buff = activeBuffs[index];
            buffAdditions[buff.type] -= buff.value;
            RemoveBuffEffect(buff);
            activeBuffs.RemoveAt(index);

        }
    }

    [Server]
    public void RemoveBuffsBySource(int skillId)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].sourceSkillId == skillId)
            {
                RemoveBuffAt(i);
            }
        }
    }
    private int attackDamageAddtion = 0;
    private float moveSpeedAddtion = 0;
    [Server]
    private void ApplyBuffEffect(BuffInfo buff)
    {
        switch (buff.type)
        {
            case StatType.Health:
                Hp += buff.value;
                Hp = Mathf.Min(Hp, Hpmax);
                break;

            case StatType.MaxHealth:
                Hpmax += buff.value;
                Hp = Mathf.Min(Hp, Hpmax);
                break;

            case StatType.Defense:
                Defense += buff.value;
                break;

            case StatType.AttackDamage:
                attackDamageAddtion += (int)buff.value;
                if (PlayerControl != null)
                {
                    PlayerControl.SetDamage(attackDamageAddtion);
                }
                break;

            case StatType.MoveSpeed:
                moveSpeedAddtion += buff.value;
                // 通知移动组件
                if (PlayerControl != null)
                {
                    PlayerControl.SetMoveSpeed(moveSpeedAddtion);
                }
                break;
        }
    }

    [Server]
    private void RemoveBuffEffect(BuffInfo buff)
    {
        
        switch (buff.type)
        {
            case StatType.MaxHealth:
                Hpmax -= buff.value;
                Hp = Mathf.Min(Hp, Hpmax);
                break;

            case StatType.Defense:
                Defense -= buff.value;
                break;

            case StatType.AttackDamage:
                attackDamageAddtion += (int)buff.value;
                if (PlayerControl != null)
                {
                    PlayerControl.SetDamage(attackDamageAddtion);
                }
                break;

            case StatType.MoveSpeed:
                if (PlayerControl != null)
                {
                    PlayerControl.SetMoveSpeed(-moveSpeedAddtion);
                }
                moveSpeedAddtion -= buff.value;
                break;
        }
    }

}
#region Buff
[System.Serializable]
public enum StatType
{
    Health,
    MaxHealth,
    Cost,
    MaxCost,
    AttackDamage,
    Defense,        // 防御力
    MoveSpeed,      // 移动速度
    AttackSpeed,
    CritChance,     // 暴击率
    CritDamage,     // 暴击伤害
    LifeSteal,
    CooldownReduction
}

public enum BuffStackType
{
    None,           // 不可叠加
    StackDuration,  // 叠加持续时间
    StackValue,     // 叠加数值
    StackBoth       // 叠加持续时间和数值
}

[System.Serializable]
public struct BuffInfo
{
    public StatType type;
    public float value;
    public float duration;
    public int sourceSkillId;
    public float startTime;
    public BuffStackType stackType;
    public int maxStacks;

    public float RemainingTime => Mathf.Max(0, duration - (Time.time - startTime));
    public bool IsExpired => Time.time - startTime >= duration;
}
#endregion