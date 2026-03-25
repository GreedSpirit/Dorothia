using UnityEngine;
using System;

[Serializable]
public class Skill_Upgrade_GoldData : ICSVLoad, ITableKey
{
    public Skill_Rank Skill_Rank { get; set; }
    public int Skill_Rank_Gold { get; set; }
    public float Skill_Upgrade_CostRate { get; set; }

    int ITableKey.Id => (int)Skill_Rank;
    string ITableKey.Key => Skill_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Skill_Rank (Skill_Rank)
        if(values.Length > 0 && Enum.TryParse(values[0], out Skill_Rank v0)) Skill_Rank = v0;
        // 1: Skill_Rank_Gold (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Skill_Rank_Gold = v1;
        // 2: Skill_Upgrade_CostRate (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Skill_Upgrade_CostRate = v2;
    }
}
