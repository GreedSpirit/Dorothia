using UnityEngine;
using System;

[System.Serializable]
public class ModuleParamData : ICSVLoad, ITableKey
{
    public int Module_Param_Id { get; set; }
    public string Skill_Effect_Name { get; set; }
    public float Skill_Effect_Time { get; set; }
    public Skill_Module Module_Type { get; set; }
    public int Hit_Count { get; set; }
    public float Aoe_Radius { get; set; }
    public float Behind_Offset { get; set; }
    public float Dash_Distance { get; set; }
    public float Dash_Duration { get; set; }
    public string Projectile_Name { get; set; }
    public float Projectile_Speed { get; set; }
    public string Skill_Sfx_Patch { get; set; }

    int ITableKey.Id => Module_Param_Id;
    string ITableKey.Key => Module_Param_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Module_Param_Id = v0;
        // 1: Effect_Addr (string)
        if (values.Length > 1) Skill_Effect_Name = values[1];
        // 2: Effect_Duration (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Skill_Effect_Time = v2;
        // 3: Module_Type (Skill_Module)
        if (values.Length > 3 && Enum.TryParse(values[3], out Skill_Module v3)) Module_Type = v3;
        // 4: Hit_Count (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Hit_Count = v4;
        // 5: Aoe_Radius (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Aoe_Radius = v5;
        // 6: Behind_Offset (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Behind_Offset = v6;
        // 7: Dash_Distance (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Dash_Distance = v7;
        // 8: Dash_Duration (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) Dash_Duration = v8;
        // 9: Projectile_Addr (string)
        if (values.Length > 9) Projectile_Name = values[9];
        // 10: Projectile_Speed (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Projectile_Speed = v10;
        // 11: Projectile_Speed (float)
        if (values.Length > 11) Projectile_Name = values[11];
    }
}
