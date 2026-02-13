using UnityEngine;
using System;

[Serializable]
public class Monster_ValueData : ICSVLoad, ITableKey
{
    public int Monster_Id { get; set; }
    public float Monster_Hp_Value { get; set; }
    public float Monster_Atk_Value { get; set; }
    public float Monster_Atk_M_Value { get; set; }
    public float Monster_Def_Value { get; set; }
    public float Monster_Def_M_Value { get; set; }

    int ITableKey.Id => Monster_Id;
    string ITableKey.Key => Monster_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Monster_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Monster_Id = v0;
        // 1: Monster_Hp_Value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Monster_Hp_Value = v1;
        // 2: Monster_Atk_Value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Monster_Atk_Value = v2;
        // 3: Monster_Atk_M_Value (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Monster_Atk_M_Value = v3;
        // 4: Monster_Def_Value (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Monster_Def_Value = v4;
        // 5: Monster_Def_M_Value (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Monster_Def_M_Value = v5;
    }
}
