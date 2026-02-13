using UnityEngine;
using System;

public enum Status
{
    체력 = 1,
    공격력 = 2,
    마법공격력 = 3,
    공격속도 = 4,
    크리티컬확률 = 5,
    크리티컬데미지 = 6,
    방어력 = 7,
    마법저항력 = 8,
    체력재생력 = 9,
    이동속도 = 10
}

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
        // 1: affection_skill (int)
        if (values.Length > 1 && Enum.TryParse(values[1], out Status v1)) Affection_Skill = v1;
        // 2: affection_skill_value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Affection_Skill_Value = v2;
    }
}
