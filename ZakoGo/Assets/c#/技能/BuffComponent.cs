using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 强化类组件
public class BuffComponent : SkillComponent
{
    [Header("Buff设置")]
    public float valueIncrease;
    public float duration = 10f;
    public GameObject buffEffect;

    public override void ExecuteServer()
    {
        var caster = skillInstance.Caster;
        if (caster == null) return;

        var stats = caster.GetComponent<CharacterNum>();
        if (stats != null)
        {
            stats.AddBuff(new BuffInfo
            {
                //type = statType,
                value = valueIncrease,
                duration = duration,
                sourceSkillId = skillData.skillID
            });
        }
    }

    public override void ExecuteClient()
    {
        var caster = skillInstance.Caster;
        if (caster != null && buffEffect != null)
        {
            Instantiate(buffEffect, caster.transform);
        }
    }
}
