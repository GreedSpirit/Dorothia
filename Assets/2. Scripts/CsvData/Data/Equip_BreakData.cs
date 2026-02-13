using UnityEngine;
using System;

[Serializable]
public class Equip_BreakData : ICSVLoad, ITableKey
{
    public int Equip_Id { get; set; }
    public int Equip_Break_Gold { get; set; }
    public int Equip_Break_Gold_Scrap { get; set; }

    int ITableKey.Id => Equip_Id;
    string ITableKey.Key => Equip_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_rank (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Id = v0;
        // 1: equip_break_gold (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Equip_Break_Gold = v1;
        // 2: equip_break_gold_scrap (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Equip_Break_Gold_Scrap = v2;
    }
}
