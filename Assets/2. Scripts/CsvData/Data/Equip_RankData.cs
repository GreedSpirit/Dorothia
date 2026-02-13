using UnityEngine;
using System;

[Serializable]
public class Equip_RankData : ICSVLoad, ITableKey
{
    public int Equip_Rank_Id { get; set; }
    public Equip_Rank Equip_Rank { get; set; }
    public float Equip_Value { get; set; }
    public float Equip_Success_Prob { get; set; }
    public float Equip_Rank_Failure { get; set; }

    int ITableKey.Id => Equip_Rank_Id;
    string ITableKey.Key => Equip_Rank_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Rank_ID (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Rank_Id = v0;
        // 1: Equip_Rank (enum)
        if(values.Length > 0 && Enum.TryParse(values[1], out Equip_Rank v1)) Equip_Rank = v1;
        // 2: Equip_Value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Equip_Value = v2;
        // 3: Equip_Success_prob (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Equip_Success_Prob = v3;
        // 4: Equip_Rank_Failure (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Equip_Rank_Failure = v4;
    }
}
