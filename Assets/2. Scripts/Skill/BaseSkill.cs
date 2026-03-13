using System;
using UnityEngine;

[Serializable]
public abstract class BaseSkill
{
    public SkillData Data { get; set; }
    public Skill_StatusData Status { get; set; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; }

    public float CurrentCooldown { get; protected set; }
    public virtual bool IsReady => CurrentCooldown <= 0f;
    public float CooldownRatio
    {
        get
        {
            if (Data == null || Data.Skill_Cooltime <= 0) return 1f;

            float ratio = (Data.Skill_Cooltime - CurrentCooldown) / Data.Skill_Cooltime;
            return Mathf.Clamp01(ratio);
        }
    }

    public static BaseSkill Create(SkillData data, Skill_StatusData status)
    {
        BaseSkill skill = data.Job_Skill_Id switch
        {
            101 => new ActiveSkill(),
            102 => new ActiveSkill(),
            103 => new ActiveSkill(),
            104 => new ActiveSkill(),
            _ => (data.Skill_Type == Skill_Type.Active) ? new ActiveSkill() : new PassiveSkill()
        };

        skill.Initialize(data, status);
        return skill;
    }

    public void Initialize(SkillData skillData, Skill_StatusData statusData)
    {
        Data = skillData;
        Status = statusData;
        Rarity = Rarity.Normal;
        Level = 1;

        CurrentCooldown = 0f;
    }

    public virtual void UpdateCooldown(float deltaTime)
    {
        if (CurrentCooldown > 0)
        {
            CurrentCooldown -= deltaTime;

            // 오차 방지
            if (CurrentCooldown < 0)
                CurrentCooldown = 0f;
        }
    }

    public virtual void StartCooldown()
    {
        if (Data != null)
        {
            CurrentCooldown = Data.Skill_Cooltime;
        }
    }

    public abstract void Execute(PlayerCtrl owner = null);
    public abstract void Undo();
}