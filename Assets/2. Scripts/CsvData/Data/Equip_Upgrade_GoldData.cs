using UnityEngine;
using System;

[Serializable]
public class Equip_Upgrade_GoldData : ICSVLoad, ITableKey
{
    public int Equip_Upgrade { get; set; }
    public float Equip_Upgrade_Value { get; set; }

    int ITableKey.Id => Equip_Upgrade;
    string ITableKey.Key => Equip_Upgrade.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Upgrade (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Upgrade = v0;
        // 1: Equip_Upgrade_Value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Equip_Upgrade_Value = v1;
    }
}
