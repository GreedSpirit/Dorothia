using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public abstract class BaseSkill
{
    public SkillData Data { get; set; }
    public float CurrentCooldown { get; private set; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; }
    public bool IsReady => CurrentCooldown <= 0f;
    public float CooldownRatio
    {
        get
        {
            if (Data == null || Data.Skill_Cooltime <= 0f) return 1f;
            return Mathf.Clamp01((Data.Skill_Cooltime - CurrentCooldown) / Data.Skill_Cooltime);
        }
    }
    public static BaseSkill Create(SkillData data)
    {
        return data.Skill_Type == Skill_Type.Passive
            ? new PassiveSkill { Data = data }
            : CreateModular(data);
    }

    private static BaseSkill CreateModular(SkillData data)
    {
        var skill = new ModularSkill { Data = data };
        var moduleList = DataManager.Instance.GetList<SkillModuleData>(data.Job_Skill_Id);

        if (moduleList == null || moduleList.Count == 0)
        {
            Debug.LogWarning($"[BaseSkill] {data.Job_Skill_Id} 모듈 데이터 없음");
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

    private static ISkillModule BuildModule(Skill_Module type, ModuleParamData p)
    {
        ISkillModule module = type switch
        {
            Skill_Module.TargetLock => new TargetLockModule(),
            Skill_Module.Melee => new MeleeModule(),
            Skill_Module.MeleeAoe => new MeleeAoeModule(),
            Skill_Module.Projectile => new ProjectileModule(),
            Skill_Module.Teleport => new TeleportModule(),
            Skill_Module.Dash => new DashModule(),
            Skill_Module.HideAppear => new HideAppearModule(),
            Skill_Module.EffectHit => new EffectHitModule(),
            _ => throw new ArgumentException($"[BuildModule] 미정의 모듈: {type}")
        };

        if (module is BaseSkillModule base_)
            base_.SetParamData(p);

        return module;
    }

    public void UpdateCooldown(float dt)
        => CurrentCooldown = Mathf.Max(0f, CurrentCooldown - dt);

    public void ResetCoolDown() => CurrentCooldown = 0;

    public void StartCooldown()
    {
        if (Data != null) CurrentCooldown = Data.Skill_Cooltime;
    }

    public abstract void Execute(PlayerCtrl player);
}