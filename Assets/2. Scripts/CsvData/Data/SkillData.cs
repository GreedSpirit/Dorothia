using UnityEngine;
using System;

[System.Serializable]
public class SkillData : ICSVLoad, ITableKey
{
    public int Job_Skill_Id { get; set; }
    public string Skill_Name { get; set; }
    public int Skill_Type { get; set; }
    public float Skill_Cooltime { get; set; }
    public int Skill_Target { get; set; }
    public int Skill_Status_Id { get; set; }
    public string Skill_Icon { get; set; }
    public string Skill_Sfx_Patch { get; set; }
    public string Skill_Effect_Patch { get; set; }
    public string Skill_Animation_Patch { get; set; }

    int ITableKey.Id => Job_Skill_Id;
    string ITableKey.Key => Skill_Name;

    public void LoadFromCsv(string[] values)
    {
        // 0: job_skill_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Job_Skill_Id = v0;
        // 1: skill_name (string)
        if (values.Length > 1) Skill_Name = values[1];
        // 2: skill_type (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Skill_Type = v2;
        // 3: skill_cooltime (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Skill_Cooltime = v3;
        // 4: skill_target (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Skill_Target = v4;
        // 5: skill_status_id (int)
        if (values.Length > 5 && int.TryParse(values[5], out int v5)) Skill_Status_Id = v5;
        // 6: skill_icon (string)
        if (values.Length > 6) Skill_Icon = values[6];
        // 7: SFX_patch (string)
        if (values.Length > 7) Skill_Sfx_Patch = values[7];
        // 8: effect_patch (string)
        if (values.Length > 8) Skill_Effect_Patch = values[8];
        // 9: animation_patch (string)
        if (values.Length > 9) Skill_Animation_Patch = values[9];
    }
}
