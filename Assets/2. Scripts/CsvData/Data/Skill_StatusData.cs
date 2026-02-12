using UnityEngine;
using System;

[System.Serializable]
public class Skill_StatusData : ICSVLoad, ITableKey
{
    public int skill_status_id { get; set; }
    public int affection_skill { get; set; }
    public float affection_skill_value { get; set; }

    int ITableKey.Id => skill_status_id;
    string ITableKey.Key => skill_status_id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: skill_status_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) skill_status_id = v0;
        // 1: affection_skill (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) affection_skill = v1;
        // 2: affection_skill_value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) affection_skill_value = v2;
    }
}
