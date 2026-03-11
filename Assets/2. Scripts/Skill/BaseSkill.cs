using System;
using UnityEngine;

[Serializable]
public abstract class BaseSkill
{
    public SkillData Data { get; set; }
    public Skill_StatusData Status { get; set; }
    public Rarity Rarity { get; set; }
    public int Level { get; set; }

    static public BaseSkill Create(SkillData data, Skill_StatusData status)
    {
        BaseSkill skill = data.Job_Skill_Id switch
        {
            101 => new ActiveSkill(),
            102 => new ActiveSkill(),
            103 => new ActiveSkill(),
            104 => new ActiveSkill(),
            _ => skill = (data.Skill_Type == Skill_Type.Active) ? new ActiveSkill() : new PassiveSkill()
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
    }

    public abstract void Execute(PlayerCtrl owner = null);
    public abstract void Undo();
}
