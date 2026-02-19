using UnityEngine;
using System;

[Serializable]
public class Character_RankData : ICSVLoad, ITableKey
{
    public Character_Rank Character_Rank { get; set; }
    public int Character_Rank_Level { get; set; }
    public float Character_Rank_Value { get; set; }
    public int Character_Rank_Scrap { get; set; }

    int ITableKey.Id => (int)Character_Rank;
    string ITableKey.Key => Character_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Character_Rank (enum)
        if (values.Length > 0 && Enum.TryParse(values[0], out Character_Rank v0)) Character_Rank = v0;
        // 1: Character_Rank_Level (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Character_Rank_Level = v1;
        // 2: Character_Rank_Value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Character_Rank_Value = v2;
        // 3: Character_Rank_Scrap (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Character_Rank_Scrap = v3;
    }
}
