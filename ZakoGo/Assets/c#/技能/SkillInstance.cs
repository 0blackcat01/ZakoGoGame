// SkillFactory.cs
using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class SkillFactory
{
    public static SkillInstance CreateSkill(SkillData skillData, GameObject caster,Vector3 targetPos)
    {
        GameObject skillObj = new GameObject($"Skill_{skillData.skillName}_{Time.time}");
        var networkIdentity = skillObj.AddComponent<NetworkIdentity>();

        // 添加技能实例组件
        var skillInstance = skillObj.AddComponent<SkillInstance>();
        // 自动生成参数
        uint casterId = caster.GetComponent<NetworkIdentity>().netId;
        Vector2 dir = Vector2.right; // 默认向右
        float hold = 0f;
        skillInstance.Initialize(skillData, casterId, targetPos, dir, hold);

        // 根据配置添加组件
        foreach (var config in skillData.components)
        {
            Type componentType = Type.GetType(config.componentName);
            if (componentType != null && typeof(SkillComponent).IsAssignableFrom(componentType))
            {
                var component = skillObj.AddComponent(componentType) as SkillComponent;
                component.Initialize(skillInstance, skillData);
            }
        }

        return skillInstance;
    }

}

// SkillInstance.cs
public class SkillInstance : NetworkBehaviour
{
    [SyncVar] private int skillId;
    [SyncVar] private uint casterNetId;
    [SyncVar] private Vector3 targetPosition;
    [SyncVar] private Vector2 direction;
    [SyncVar] private float holdTime;

    private SkillData skillData;
    private GameObject caster;
    private List<SkillComponent> components = new List<SkillComponent>();

    public uint CasterNetId => casterNetId;
    public GameObject Caster => caster;
    public bool IsOwner => caster != null && caster.GetComponent<NetworkIdentity>().isLocalPlayer;

    public uint InstanceNetId => GetComponent<NetworkIdentity>().netId;
    [Server]
    public void Initialize(SkillData data,uint casterId, Vector3 targetPos, Vector2 dir, float hold)
    {
        skillData = data;
        skillId = skillData.skillID;
        casterNetId = casterId;
        targetPosition = targetPos;
        direction = dir;
        holdTime = hold;
        // 执行服务器逻辑
        ExecuteServer();
    }

    [Server]
    private void ExecuteServer()
    {
        foreach (var component in components)
        {
            component.ExecuteServer();
        }

        // 如果是瞬发技能，立即销毁
        if (skillData.castTime <= 0)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        NetworkIdentity casterIdentity = NetworkClient.spawned.GetValueOrDefault(casterNetId);

        if (casterIdentity != null)
        {
            caster = casterIdentity.gameObject;
            Debug.Log($"Found caster: {caster.name} (NetId: {casterNetId})");
        }
        // 获取施法者


        // 如果不是服务器，播放客户端效果
        if (!isServer)
        {
            ExecuteClient();
        }
    }

    private void ExecuteClient()
    {
        foreach (var component in components)
        {
            component.ExecuteClient();
        }
    }

    public void AddComponent(SkillComponent component)
    {
        components.Add(component);
    }
}