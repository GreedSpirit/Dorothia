using UnityEngine;
using System;

[Serializable]
public class EquipData : ICSVLoad, ITableKey
{
    public int Equip_Id { get; set; }
    public string Equip_Name { get; set; }
    public Equip_Type Equip_Type { get; set; }
    public float Equip_Hp { get; set; }
    public float Equip_Atk { get; set; }
    public float Equip_Dps { get; set; }
    public float Equip_Crt_Prob { get; set; }
    public float Equip_Crt_Dmg { get; set; }
    public float Equip_Def { get; set; }
    public float Equip_Hp_Regen { get; set; }
    public float Equip_Agi { get; set; }
    public int Equip_Price { get; set; }
    public string Equip_Icon { get; set; }

    int ITableKey.Id => Equip_Id;
    string ITableKey.Key => Equip_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Id = v0;
        // 1: Equip_Name (string)
        if (values.Length > 1) Equip_Name = values[1];
        // 2: Equip_Type (Equip_Type)
        if (values.Length > 2 && Enum.TryParse(values[2], out Equip_Type v2)) Equip_Type = v2;
        // 3: Equip_Hp (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Equip_Hp = v3;
        // 4: Equip_Atk (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Equip_Atk = v4;
        // 5: Equip_Dps (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Equip_Dps = v5;
        // 6: Equip_Crt_Prob (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Equip_Crt_Prob = v6;
        // 7: Equip_Crt_Dmg (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Equip_Crt_Dmg = v7;
        // 8: Equip_Def (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) Equip_Def = v8;
        // 9: Equip_Hp_Regen (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) Equip_Hp_Regen = v9;
        // 10: Equip_Agi (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Equip_Agi = v10;
        // 11: Equip_Price (int)
        if (values.Length > 11 && int.TryParse(values[11], out int v11)) Equip_Price = v11;
        // 12: Equip_Icon (string)
        if (values.Length > 12) Equip_Icon = values[12];
    }
}
