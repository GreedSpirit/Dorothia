using UnityEngine;
using System;

[Serializable]
public class Monster_Data : ICSVLoad, ITableKey
{
    public int Monster_Id { get; set; }
    public string Monster_Name { get; set; }
    public Monster_Type Monster_Type { get; set; }
    public Monster_Kind Monster_Kind { get; set; }
    public float Monster_Hp { get; set; }
    public float Monster_Atk { get; set; }
    public float Monster_Atk_M { get; set; }
    public float Monster_Def { get; set; }
    public float Monster_Def_M { get; set; }
    public float Monster_Agi { get; set; }
    public float Monster_Atk_Range { get; set; }
    public string Monster_Model { get; set; }

    int ITableKey.Id => Monster_Id;
    string ITableKey.Key => Monster_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Monster_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Monster_Id = v0;
        // 1: Monster_Name (string)
        if (values.Length > 1) Monster_Name = values[1];
        // 2: Monster_Type (Monster_Type)
        if (values.Length > 2 && Enum.TryParse(values[2], out Monster_Type v2)) Monster_Type = v2;
        // 3: Monster_Kind (Monster_Kind)
        if (values.Length > 3 && Enum.TryParse(values[3], out Monster_Kind v3)) Monster_Kind = v3;
        // 4: Monster_Hp (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Monster_Hp = v4;
        // 5: Monster_Atk (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Monster_Atk = v5;
        // 6: Monster_Atk_M (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Monster_Atk_M = v6;
        // 7: Monster_Def (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Monster_Def = v7;
        // 8: Monster_Def_M (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) Monster_Def_M = v8;
        // 9: Monster_Agi (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) Monster_Agi = v9;
        // 10: Monster_Atk_Range (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Monster_Atk_Range = v10;
        // 11: Monster_Model (string)
        if (values.Length > 11) Monster_Model = values[11];
    }
}
