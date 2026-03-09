using UnityEngine;
using System;

[Serializable]
public class Gremlin_AtkerData : ICSVLoad, ITableKey
{
    public Rarity Gremlin_Tier { get; set; }
    public float Gremlin_Level_Bonus { get; set; }
    public float Gremlin_Tier_Dps { get; set; }

    int ITableKey.Id => (int)Gremlin_Tier;
    string ITableKey.Key => Gremlin_Tier.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gramlin_Tier (Rarity)
        if (values.Length > 0 && Enum.TryParse(values[0], out Rarity v0)) Gremlin_Tier = v0;
        // 1: Gramlin_Level_Bonus (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Gremlin_Level_Bonus = v1;
        // 2: Gramlin_Tier_Dps (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Gremlin_Tier_Dps = v2;
    }
}
