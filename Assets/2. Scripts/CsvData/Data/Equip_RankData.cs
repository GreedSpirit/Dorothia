using UnityEngine;
using System;

[Serializable]
public class Equip_RankData : ICSVLoad, ITableKey
{
    public Rarity Equip_Rank { get; set; }
    public float Equip_Value { get; set; }
    public float Equip_Success_Prob { get; set; }
    public float Equip_Rank_Failure { get; set; }

    int ITableKey.Id => (int)Equip_Rank;
    string ITableKey.Key => Equip_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Rank (enum)
        if (values.Length > 0 && Enum.TryParse(values[0], out Rarity v0)) Equip_Rank = v0;
        // 1: Equip_Value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Equip_Value = v1;
        // 2: Equip_Success_Prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Equip_Success_Prob = v2;
        // 3: Equip_Rank_Failure (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Equip_Rank_Failure = v3;
    }
}
