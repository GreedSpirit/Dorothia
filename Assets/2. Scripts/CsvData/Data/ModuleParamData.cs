using System;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ModuleParamData : ICSVLoad, ITableKey
{
    public int Module_Param_Id { get; set; }
    public string Skill_Effect_Name { get; set; }
    public float Skill_Effect_Time { get; set; }
    public Skill_Module Module_Type { get; set; }
    public int[] Hit_Count_Array { get; set; }
    public float[] Aoe_Radius { get; set; }
    public float Behind_Offset { get; set; }
    public int Repeat_Count { get; set; }
    public float[] Repeat_Interval { get; set; }
    public float Dash_Distance { get; set; }
    public float Dash_Duration { get; set; }
    public string Projectile_Name { get; set; }
    public float Projectile_Speed { get; set; }
    public string Skill_Sfx_Patch { get; set; }
    public float SkillCast_Range { get; set; }
    public float First_Delay { get; set; }
    public Skill_Target Skill_Target { get; set; }

    int ITableKey.Id => Module_Param_Id;
    string ITableKey.Key => Module_Param_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Module_Param_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Module_Param_Id = v0;
        // 1: Skill_Effect_Name (string)
        if (values.Length > 1) Skill_Effect_Name = values[1];
        // 2: Skill_Effect_Time (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Skill_Effect_Time = v2;
        // 3: Module_Type (Skill_Module)
        if (values.Length > 3 && Enum.TryParse(values[3], out Skill_Module v3)) Module_Type = v3;
        // 4: Hit_Count_Array (int[])
        if (values.Length > 4) Hit_Count_Array = ParseIntArray(values[4], 0);
        // 5: Aoe_Radius (float[])
        if (values.Length > 5) Aoe_Radius = ParseFloatArray(values[5], 0);
        // 6: Behind_Offset (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Behind_Offset = v6;
        // 7: Repeat_Count (int)
        if (values.Length > 7 && int.TryParse(values[7], out int v7)) Repeat_Count = v7;
        // 8: Repeat_Interval (float)
        if (values.Length > 8) Repeat_Interval = ParseFloatArray(values[8],0);
        // 9: Dash_Distance (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) Dash_Distance = v9;
        // 10: Dash_Duration (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Dash_Duration = v10;
        // 11: Projectile_Name (string)
        if (values.Length > 11) Projectile_Name = values[11];
        // 12: Projectile_Speed (float)
        if (values.Length > 12 && float.TryParse(values[12], out float v12)) Projectile_Speed = v12;
        // 13: Skill_Sfx_Patch (string)
        if (values.Length > 13) Skill_Sfx_Patch = values[13];
        // 14: SkillCast_Range (float)
        if (values.Length > 14 && float.TryParse(values[14], out float v14)) SkillCast_Range = v14;
        // 15: First_Delay (float)
        if (values.Length > 15 && float.TryParse(values[15], out float v15)) First_Delay = v15;
        // 16: Skill_Target (Skill_Target)
        if (values.Length > 16 && Enum.TryParse(values[16], out Skill_Target v16)) Skill_Target = v16;
    }
    private int[] ParseIntArray(string value, int defaultVal)
    {
        if (string.IsNullOrEmpty(value)) return new[] { defaultVal };
        var parts = value.Split(',');
        var result = new int[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            result[i] = int.TryParse(parts[i].Trim(), out int v) ? Mathf.Max(1, v) : defaultVal;
        return result;
    }

    private float[] ParseFloatArray(string value, float defaultVal)
    {
        if (string.IsNullOrEmpty(value)) return new[] { defaultVal };
        var parts = value.Split(',');
        var result = new float[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            result[i] = float.TryParse(parts[i].Trim(), out float v) ? v : defaultVal;
        return result;
    }
}
