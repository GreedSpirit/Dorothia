using UnityEngine;
using System;

[Serializable]
public class Equip_RankData : ICSVLoad, ITableKey
{
    public int equip_rank { get; set; }
    public float equip_value { get; set; }
    public float equip_success_prob { get; set; }
    public float equip_rank_failure { get; set; }

    int ITableKey.Id => equip_rank;
    string ITableKey.Key => equip_rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_rank (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_rank = v0;
        // 1: equip_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) equip_value = v1;
        // 2: equip_success_prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) equip_success_prob = v2;
        // 3: equip_rank_failure (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) equip_rank_failure = v3;
    }
}
