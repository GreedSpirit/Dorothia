using UnityEngine;
using System;

[Serializable]
public class Equip_Upgrade_GoldData : ICSVLoad, ITableKey
{
    public Equip_Rank Equip_Rank { get; set; }
    public float Equip_Rank_Value { get; set; }
    public int Equip_Upgrade { get; set; }
    public float Equip_Upgrade_Value { get; set; }

    int ITableKey.Id => (int)Equip_Rank;
    string ITableKey.Key => Equip_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_rank (Equip_Rank)
        if (values.Length > 0 && Enum.TryParse(values[0], out Equip_Rank v0)) Equip_Rank = v0;
        // 1: equip_rank_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Equip_Rank_Value = v1;
        // 2: equip_upgrade (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Equip_Upgrade = v2;
        // 3: equip_upgrade_value (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Equip_Upgrade_Value = v3;
    }
}
