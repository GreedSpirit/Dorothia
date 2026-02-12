using UnityEngine;
using System;

[Serializable]
public class EquipData : ICSVLoad, ITableKey
{
    public int equip_id { get; set; }
    public string equip_name { get; set; }
    public int equip_type { get; set; }
    public string model { get; set; }
    public string equip_icon { get; set; }

    int ITableKey.Id => equip_id;
    string ITableKey.Key => equip_id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: equip_id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) equip_id = v0;
        // 1: equip_name (string)
        if (values.Length > 1) equip_name = values[1];
        // 2: equip_type (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) equip_type = v2;
        // 3: model (string)
        if (values.Length > 3) model = values[3];
        // 4: equip_icon (string)
        if (values.Length > 4) equip_icon = values[4];
    }
}
