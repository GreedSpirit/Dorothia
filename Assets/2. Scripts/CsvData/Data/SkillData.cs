using UnityEngine;
using System;

[System.Serializable]
public class SkillData : ICSVLoad, ITableKey
{
    public int Job_Skill_Id { get; set; }
    public string Skill_Name { get; set; }
    public Skill_Type Skill_Type { get; set; }
    public float Skill_Cooltime { get; set; }
    public Status Affection_Skill { get; set; }
    public float Affection_Skill_Value { get; set; }
    public string Skill_Animation_Patch { get; set; }
    public string Skill_Icon { get; set; }

    int ITableKey.Id => Job_Skill_Id;
    string ITableKey.Key => Skill_Name;

    public void LoadFromCsv(string[] values)
    {
        // 0: job_skill_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Job_Skill_Id = v0;
        // 1: skill_name (string)
        if (values.Length > 1) Skill_Name = values[1];
        // 2: skill_type (Skill_Type)
        if (values.Length > 2 && Enum.TryParse(values[2], out Skill_Type v2)) Skill_Type = v2;
        // 3: skill_cooltime (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Skill_Cooltime = v3;
        // 4: Affection_Skill (Status)
        if (values.Length > 4 && Enum.TryParse(values[4], out Status v4)) Affection_Skill = v4;
        // 5: Affection_Skill_Value (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Affection_Skill_Value = v5;
        // 6: Skill_Animation_Patch (string)
        if (values.Length > 6) Skill_Animation_Patch = values[6];
        // 7: Skill_Icon (string)
        if (values.Length > 7) Skill_Icon = values[7];
    }
}