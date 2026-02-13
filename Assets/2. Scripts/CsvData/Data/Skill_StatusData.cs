using UnityEngine;
using System;
[Serializable]
public class Skill_StatusData : ICSVLoad, ITableKey
{
    public int Skill_Status_Id { get; set; }
    public Status Affection_Skill { get; set; }
    public float Affection_Skill_Value { get; set; }

    int ITableKey.Id => Skill_Status_Id;
    string ITableKey.Key => Skill_Status_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: skill_status_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Skill_Status_Id = v0;
        // 1: affection_skill (Status)
        if (values.Length > 1 && Enum.TryParse(values[1], out Status v1)) Affection_Skill = v1;
        // 2: affection_skill_value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Affection_Skill_Value = v2;
    }
}
