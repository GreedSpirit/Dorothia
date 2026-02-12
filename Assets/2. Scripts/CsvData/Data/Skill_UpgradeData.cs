using UnityEngine;
using System;

[System.Serializable]
public class Skill_UpgradeData : ICSVLoad, ITableKey
{
    public int skill_upgrade { get; set; }
    public float skill_upgrade_value { get; set; }
    int ITableKey.Id => skill_upgrade;
    string ITableKey.Key => skill_upgrade.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: skill_upgrade (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) skill_upgrade = v0;
        // 1: skill_upgrade_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) skill_upgrade_value = v1;
    }
}
