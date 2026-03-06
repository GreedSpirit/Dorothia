using UnityEngine;
using System;

[Serializable]
public class Gremlin_TierData : ICSVLoad, ITableKey
{
    public Rarity Gremlin_Tier { get; set; }
    public float Gremlin_Tier_Multiplier { get; set; }

    int ITableKey.Id => (int)Gremlin_Tier;
    string ITableKey.Key => Gremlin_Tier.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gramlin_Tier (Rarity)
        if (values.Length > 0 && Enum.TryParse(values[0], out Rarity v0)) Gremlin_Tier = v0;
        // 1: Gramlin_Tier_Multiplier (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Gremlin_Tier_Multiplier = v1;
    }
}
