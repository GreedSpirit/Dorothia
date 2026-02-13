using UnityEngine;
using System;

[Serializable]
public class Skill_Upgrade_ScrapData : ICSVLoad, ITableKey
{
    public Skill_Rank Skill_Rank { get; set; }
    public int Skill_Rank_Value { get; set; }
    public float Skill_Upgrade_Value { get; set; }

    int ITableKey.Id => (int)Skill_Rank;
    string ITableKey.Key => Skill_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Skill_Rank (enum)
        if(values.Length > 0 && Enum.TryParse(values[0], out Skill_Rank Rank)) Skill_Rank = Rank;
        // 1: Skill_Rank_Value (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Skill_Rank_Value = v1;
        // 2: Skill_Upgrade_Value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Skill_Upgrade_Value = v2;
    }
}
