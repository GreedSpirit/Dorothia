using UnityEngine;
using System;

[System.Serializable]
public class Skill_RankData : ICSVLoad, ITableKey
{
    public int Skill_Rank { get; set; }
    public float Skill_Value { get; set; }
    public float Skill_Success_Prob { get; set; }
    public float Skill_Rank_Failure { get; set; }

    int ITableKey.Id => Skill_Rank;
    string ITableKey.Key => Skill_Rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: skill_rank (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Skill_Rank = v0;
        // 1: skill_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Skill_Value = v1;
        // 2: skill_success_prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Skill_Success_Prob = v2;
        // 3: skill_rank_failure (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Skill_Rank_Failure = v3;
    }
}
