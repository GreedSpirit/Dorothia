using UnityEngine;
using System;

[Serializable]
public class Equip_StatusData : ICSVLoad, ITableKey
{
    public int equip_id { get; set; }
    public float HP { get; set; }
    public float ATK { get; set; }
    public float ATK_M { get; set; }
    public float DPS { get; set; }
    public float CRT_prob { get; set; }
    public float CRT_DMG { get; set; }
    public float DEF { get; set; }
    public float DEF_M { get; set; }
    public float HP_regen { get; set; }
    public float AGI { get; set; }
    public int equip_price { get; set; }

    int ITableKey.Id => equip_id;
    string ITableKey.Key => equip_id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_id = v0;
        // 1: HP (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) HP = v1;
        // 2: ATK (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) ATK = v2;
        // 3: ATK_M (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) ATK_M = v3;
        // 4: DPS (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) DPS = v4;
        // 5: CRT_prob (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) CRT_prob = v5;
        // 6: CRT_DMG (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) CRT_DMG = v6;
        // 7: DEF (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) DEF = v7;
        // 8: DEF_M (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) DEF_M = v8;
        // 9: HP_regen (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) HP_regen = v9;
        // 10: AGI (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) AGI = v10;
        // 11: equip_price (int)
        if (values.Length > 11 && int.TryParse(values[11], out int v11)) equip_price = v11;
    }
}
