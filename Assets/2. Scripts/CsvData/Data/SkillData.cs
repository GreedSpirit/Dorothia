using UnityEngine;
using System;

[System.Serializable]
public class SkillData : ICSVLoad, ITableKey
{
    public int job_skill_id { get; set; }
    public string skill_name { get; set; }
    public int skill_type { get; set; }
    public float skill_cooltime { get; set; }
    public int skill_target { get; set; }
    public int skill_status_id { get; set; }
    public string skill_icon { get; set; }
    public string SFX_patch { get; set; }
    public string effect_patch { get; set; }
    public string animation_patch { get; set; }

    int ITableKey.Id => job_skill_id;
    string ITableKey.Key => skill_name;

    public void LoadFromCsv(string[] values)
    {
        // 0: job_skill_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) job_skill_id = v0;
        // 1: skill_name (string)
        if (values.Length > 1) skill_name = values[1];
        // 2: skill_type (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) skill_type = v2;
        // 3: skill_cooltime (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) skill_cooltime = v3;
        // 4: skill_target (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) skill_target = v4;
        // 5: skill_status_id (int)
        if (values.Length > 5 && int.TryParse(values[5], out int v5)) skill_status_id = v5;
        // 6: skill_icon (string)
        if (values.Length > 6) skill_icon = values[6];
        // 7: SFX_patch (string)
        if (values.Length > 7) SFX_patch = values[7];
        // 8: effect_patch (string)
        if (values.Length > 8) effect_patch = values[8];
        // 9: animation_patch (string)
        if (values.Length > 9) animation_patch = values[9];
    }
}
