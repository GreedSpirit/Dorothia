using UnityEngine;
using System;

[Serializable]
public class Equip_LevelData : ICSVLoad, ITableKey
{
    public int Equip_Level { get; set; }
    public float Equip_Level_Value { get; set; }

    int ITableKey.Id => Equip_Level;
    string ITableKey.Key => Equip_Level.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_level (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Level = v0;
        // 1: equip_level_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Equip_Level_Value = v1;
    }
}
