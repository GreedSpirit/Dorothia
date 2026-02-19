using UnityEngine;
using System;

[Serializable]
public class Equip_Rank_GoldData : ICSVLoad, ITableKey
{
    public Equip_Rank Equip_Rank { get; set; }
    public float Equip_Rank_Value { get; set; }

    int ITableKey.Id => (int)Equip_Rank;
    string ITableKey.Key => Equip_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Rank (Equip_Rank)
        if (values.Length > 0 && Enum.TryParse(values[0], out Equip_Rank v0)) Equip_Rank = v0;
        // 1: Equip_Rank_Value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Equip_Rank_Value = v1;
    }
}
