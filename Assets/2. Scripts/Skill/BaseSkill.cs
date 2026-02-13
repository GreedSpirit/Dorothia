using System;
using UnityEngine;

public enum SkillRarity { Common, Rare, Epic, Legendary, Mistic }

[Serializable]
public abstract class BaseSkill
{
    public SkillData Data { get; set; }
    public Skill_StatusData Status { get; set; }
    public SkillRarity Rarity { get; set; }
    public int Level { get; set; }

    static public BaseSkill Create(SkillData data, Skill_StatusData status)
    {
        BaseSkill skill = (data.Skill_Type == Skill_Type.Active) ? new ActiveSkill() : new PassiveSkill();
        skill.Initialize(data, status);

        return skill;
    }
    public void Initialize(SkillData skillData, Skill_StatusData statusData)
    {
        Data = skillData;
        Status = statusData;
        Rarity = SkillRarity.Common;
        Level = 1;
    }

    public abstract void Execute();
    public abstract void Undo();
}
