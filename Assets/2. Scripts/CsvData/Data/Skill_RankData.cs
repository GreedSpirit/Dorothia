using UnityEngine;
using System;

[System.Serializable]
public class Skill_RankData : ICSVLoad, ITableKey
{
    public int skill_rank { get; set; }
    public float skill_value { get; set; }
    public float skill_success_prob { get; set; }
    public float skill_rank_failure { get; set; }

    int ITableKey.Id => skill_rank;
    string ITableKey.Key => skill_rank.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: skill_rank (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) skill_rank = v0;
        // 1: skill_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) skill_value = v1;
        // 2: skill_success_prob (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) skill_success_prob = v2;
        // 3: skill_rank_failure (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) skill_rank_failure = v3;
    }
}
