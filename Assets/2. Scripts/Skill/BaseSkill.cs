using System;
using UnityEngine;

[Serializable]
public abstract class BaseSkill
{
    public SkillData Data { get; set; }
    public Skill_StatusData Status { get; set; }
    public float CurrentCooldown { get; protected set; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; }

    public virtual void Initialize(SkillData data, Skill_StatusData status)
    {
        Data = data;
        Status = status;
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

    public virtual void StartCooldown()
    {
        if (Data != null)
        {
            CurrentCooldown = Data.Skill_Cooltime;
        }
    }
    public virtual void Execute(PlayerCtrl owner)
    {
        StartCooldown();
        // 마나 소모 등 공통 로직
    }

    public abstract void Undo();

    public void UpdateCooldown(float dt) => CurrentCooldown = Mathf.Max(0, CurrentCooldown - dt);
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
}
