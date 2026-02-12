using UnityEngine;
using System;

[Serializable]
public class Equip_SetData : ICSVLoad, ITableKey
{
    public int equip_set_id { get; set; }
    public string equip_set_need_name { get; set; }
    public int equip_set_need_nember { get; set; }
    public int affection_equip_set { get; set; }
    public float affection_equip_set_value { get; set; }

    int ITableKey.Id => equip_set_id;
    string ITableKey.Key => equip_set_id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_set_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_set_id = v0;
        // 1: equip_set_need_name (string)
        if (values.Length > 1) equip_set_need_name = values[1];
        // 2: equip_set_need_nember (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) equip_set_need_nember = v2;
        // 3: affection_equip_set (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) affection_equip_set = v3;
        // 4: affection_equip_set_value (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) affection_equip_set_value = v4;
    }
}
