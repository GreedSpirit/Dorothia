using UnityEngine;
using System;

[Serializable]
public class Gremlin_UpgradeData : ICSVLoad, ITableKey
{
    public Rarity Gremlin_Tier { get; set; }
    public int Gremlin_Upgrade_Cost { get; set; }
    public float Gremlin_Upgrade_Prob { get; set; }
    public float Up_Cost_Value { get; set; }
    public float Up_Prob_Value { get; set; }

    int ITableKey.Id => (int)Gremlin_Tier;
    string ITableKey.Key => Gremlin_Tier.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gremlin_Tier (enum)
        if (values.Length > 0 && Enum.TryParse(values[0], out Rarity v0)) Gremlin_Tier = v0;
        // 1: Gremlin_Upgrade_Cost (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Gremlin_Upgrade_Cost = v1;
        // 2: Gremlin_Upgrade_Prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Gremlin_Upgrade_Prob = v2;
        // 3: Up_Cost_Value (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Up_Cost_Value = v3;
        // 4: Up_Prob_Value (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Up_Prob_Value = v4;
    }
}
