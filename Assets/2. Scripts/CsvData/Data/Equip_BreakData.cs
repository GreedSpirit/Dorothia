using UnityEngine;
using System;

[Serializable]
public class Equip_BreakData : ICSVLoad, ITableKey
{
    public Rarity Equip_Rank { get; set; }
    public int Equip_Break_Gold { get; set; }
    public int Equip_Break_Gold_Scrap { get; set; }

    int ITableKey.Id => (int)Equip_Rank;
    string ITableKey.Key => Equip_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Rank (enum)
        if (values.Length > 0 && Enum.TryParse(values[0], out Rarity v0)) Equip_Rank = v0;
        // 1: Equip_Break_Gold (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Equip_Break_Gold = v1;
        // 2: Equip_Break_Gold_Scrap (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Equip_Break_Gold_Scrap = v2;
    }
}
