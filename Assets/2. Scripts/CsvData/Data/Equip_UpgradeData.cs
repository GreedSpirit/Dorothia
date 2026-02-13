using UnityEngine;
using System;

[Serializable]
public class Equip_UpgradeData : ICSVLoad, ITableKey
{
    public int Equip_Upgrade { get; set; }
    public string Equip_Upgrade_Section { get; set; }
    public float Equip_Success_Prob { get; set; }
    public float Equip_Value { get; set; }
    public float Equip_Upgrade_Failure { get; set; }

    int ITableKey.Id => Equip_Upgrade;
    string ITableKey.Key => Equip_Upgrade.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Upgrade (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Upgrade = v0;
        // 1: Equip_Upgrade_section (string)
        if (values.Length > 1) Equip_Upgrade_Section = values[1];
        // 2: Equip_Success_prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Equip_Success_Prob = v2;
        // 3: Equip_Value (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Equip_Value = v3;
        // 4: Equip_Upgrade_Failure (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Equip_Upgrade_Failure = v4;
    }
}
