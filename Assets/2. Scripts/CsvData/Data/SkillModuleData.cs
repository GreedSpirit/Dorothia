using UnityEngine;
using System;

[System.Serializable]
public class SkillModuleData : ICSVLoad, ITableKey
{
    public int Skill_Module_Id { get; set; }
    public int Job_Skill_Id { get; set; }
    public int Module_Order { get; set; }
    public Skill_Module Module_Type { get; set; }
    public int Module_Param_Id { get; set; }

    int ITableKey.Id => Job_Skill_Id;
    string ITableKey.Key => Job_Skill_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Skill_Module_Id = v0;
        // 1: Skill_Id (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Job_Skill_Id = v1;
        // 2: Module_Order (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Module_Order = v2;
        // 3: Module_Type (Skill_Module)
        if (values.Length > 3 && Enum.TryParse(values[3], out Skill_Module v3)) Module_Type = v3;
        // 4: Module_Param_Id (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Module_Param_Id = v4;

    }
}
