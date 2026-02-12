using UnityEngine;
using System;

[Serializable]
public class Equip_UpgradeData : ICSVLoad, ITableKey
{
    public int equip_upgrade { get; set; }
    public string equip_upgrade_section { get; set; }
    public float equip_success_prob { get; set; }
    public int equip_upgrade_gold_id { get; set; }
    public float equip_value { get; set; }
    public float equip_upgrade_failure { get; set; }

    int ITableKey.Id => equip_upgrade;
    string ITableKey.Key => equip_upgrade.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_upgrade (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_upgrade = v0;
        // 1: equip_upgrade_section (string)
        if (values.Length > 1) equip_upgrade_section = values[1];
        // 2: equip_success_prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) equip_success_prob = v2;
        // 3: equip_upgrade_gold_id (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) equip_upgrade_gold_id = v3;
        // 4: equip_value (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) equip_value = v4;
        // 5: equip_upgrade_failure (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) equip_upgrade_failure = v5;
    }
}
