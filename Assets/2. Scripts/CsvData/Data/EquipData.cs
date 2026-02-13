using UnityEngine;
using System;

public enum Equip_Type
{
    무기 = 1,
    상의 = 2,
    하의 = 3,
    장갑 = 4,
    신발 = 5,
    목걸이 = 6,
    반지 = 7,
}

[Serializable]
public class EquipData : ICSVLoad, ITableKey
{
    public int Equip_Id { get; set; }
    public string Equip_Name { get; set; }
    public Equip_Type Equip_Type{ get; set; }
    public float Equip_Hp { get; set; }
    public float Equip_Atk { get; set; }
    public float Equip_Atk_M { get; set; }
    public float Equip_Dps { get; set; }
    public float Equip_Crt_Prob { get; set; }
    public float Equip_Crt_Dmg { get; set; }
    public float Equip_Def { get; set; }
    public float Equip_Def_M { get; set; }
    public float Equip_Hp_Regen { get; set; }
    public float Equip_Agi { get; set; }
    public int Equip_Price { get; set; }
    public string Equip_Model { get; set; }
    public string Equip_Icon { get; set; }

    int ITableKey.Id => Equip_Id;
    string ITableKey.Key => Equip_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_ID (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Id = v0;
        // 1: Equip_Name (string)
        if (values.Length > 1) Equip_Name = values[1];
        // 2: Equip_Type (enum)
        if(values.Length > 2 && Enum.TryParse(values[2], out Equip_Type Type)) Equip_Type = Type;
        // 3: Equip_HP (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Equip_Hp = v3;
        // 4: Equip_ATK (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Equip_Atk = v4;
        // 5: Equip_ATK_M (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Equip_Atk_M = v5;
        // 6: Equip_DPS (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Equip_Dps = v6;
        // 7: Equip_CRT_prob (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Equip_Crt_Prob = v7;
        // 8: Equip_CRT_DMG (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) Equip_Crt_Dmg = v8;
        // 9: Equip_DEF (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) Equip_Def = v9;
        // 10: Equip_DEF_M (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Equip_Def_M = v10;
        // 11: Equip_HP_regen (float)
        if (values.Length > 11 && float.TryParse(values[11], out float v11)) Equip_Hp_Regen = v11;
        // 12: Equip_AGI (float)
        if (values.Length > 12 && float.TryParse(values[12], out float v12)) Equip_Agi = v12;
        // 13: Equip_price (int)
        if (values.Length > 13 && int.TryParse(values[13], out int v13)) Equip_Price = v13;
        // 14: Equip_Model (string)
        if (values.Length > 14) Equip_Model = values[14];
        // 15: Equip_Icon (string)
        if (values.Length > 15) Equip_Icon = values[15];
    }
}
