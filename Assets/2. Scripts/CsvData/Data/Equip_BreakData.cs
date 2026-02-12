using UnityEngine;
using System;

[Serializable]
public class Equip_BreakData : ICSVLoad, ITableKey
{
    public int equip_rank { get; set; }
    public int equip_break_gold { get; set; }
    public int equip_break_gold_scrap { get; set; }

    int ITableKey.Id => equip_rank;
    string ITableKey.Key => equip_rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_rank (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_rank = v0;
        // 1: equip_break_gold (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) equip_break_gold = v1;
        // 2: equip_break_gold_scrap (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) equip_break_gold_scrap = v2;
    }
}
