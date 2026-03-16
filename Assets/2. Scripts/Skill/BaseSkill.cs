using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class BaseSkill
{
    public SkillData Data { get; set; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; }

    public float CurrentCooldown { get; private set; }
    public bool IsReady => CurrentCooldown <= 0f;
    public float CooldownRatio
    {
        get
        {
            if (Data == null || Data.Skill_Cooltime <= 0f) return 1f;
            return Mathf.Clamp01((Data.Skill_Cooltime - CurrentCooldown) / Data.Skill_Cooltime);
        }
    }

    public static BaseSkill Create(SkillData data, Skill_StatusData statusData = null)
    {
        if (data.Skill_Type == Skill_Type.Passive)
            return CreatePassive(data, statusData);

        return CreateModular(data);
    }

    private static BaseSkill CreateModular(SkillData data)
    {
        var skill = new ModularSkill { Data = data };

        var moduleList = DataManager.Instance.GetList<SkillModuleData>(10001);

        if (moduleList == null || moduleList.Count == 0)
        {
            Debug.LogWarning($"[BaseSkill] {data.Job_Skill_Id} 스킬의 모듈 데이터 없음");
            return skill;
        }

        foreach (var moduleData in moduleList)
        {
            ModuleParamData param = DataManager.Instance
                .GetData<ModuleParamData>(moduleData.Module_Param_Id);
            skill.AddModule(BuildModule(moduleData.Module_Type, param));
        }

        return skill;
    }

    private static BaseSkill CreatePassive(SkillData data, Skill_StatusData statusData)
    {
        return new PassiveSkill { Data = data};
    }

    private static ISkillModule BuildModule(Skill_Module type, ModuleParamData p)
    {
        ISkillModule module = type switch
        {
            Skill_Module.TargetLock => new TargetLockModule(),
            Skill_Module.Melee => new MeleeAttackModule(),
            Skill_Module.MeleeAoe => new MeleeAttackModule(p.Aoe_Radius),
            //Skill_Module.Projectile => new ProjectileModule(p.Projectile_Name, p.Projectile_Speed),
            Skill_Module.Teleport => new TeleportModule(p.Behind_Offset),
            Skill_Module.Dash => new DashModule(p.Dash_Distance, p.Dash_Duration),
            Skill_Module.Jump => new JumpAttackModule(p.Skill_Effect_Time),
            _ => throw new System.ArgumentException($"[BaseSkill] 미정의 모듈: {type}")
        };

        if (module is BaseSkillModule baseModule)
            baseModule.SetParamData(p);

        return module;
    }
    // 공통
    public void UpdateCooldown(float dt)
        => CurrentCooldown = Mathf.Max(0f, CurrentCooldown - dt);

    public void StartCooldown()
    {
        if (Data != null) CurrentCooldown = Data.Skill_Cooltime;
    }

    public abstract void Execute(PlayerCtrl player);
}