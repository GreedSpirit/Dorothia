using UnityEngine;
using System;

[Serializable]
public class Equip_LevelData : ICSVLoad, ITableKey
{
    public int equip_level { get; set; }
    public float equip_level_value { get; set; }

    int ITableKey.Id => equip_level;
    string ITableKey.Key => equip_level.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_level (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_level = v0;
        // 1: equip_level_value (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) equip_level_value = v1;
    }
}
