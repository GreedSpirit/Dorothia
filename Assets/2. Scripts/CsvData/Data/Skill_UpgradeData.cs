using UnityEngine;
using System;

[Serializable]
public class Skill_UpgradeData : ICSVLoad, ITableKey
{
    public int Skill_Upgrade { get; set; }
    public float Skill_Upgrade_Value { get; set; }
    int ITableKey.Id => Skill_Upgrade;
    string ITableKey.Key => Skill_Upgrade.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: skill_upgrade (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Skill_Upgrade = v0;
        // 1: skill_upgrade_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Skill_Upgrade_Value = v1;
    }
}
