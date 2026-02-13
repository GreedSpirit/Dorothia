using UnityEngine;
using System;

[Serializable]
public class Equip_SetData : ICSVLoad, ITableKey
{
    public int Equip_Set_Id { get; set; }
    public int Set_Manager_Id { get; set; }
    public string Equip_Set_Need_Name { get; set; }
    public int Equip_Set_Need_Number { get; set; }
    public Status Affection_Equip_Set { get; set; }
    public float Affection_Equip_Set_Value { get; set; }

    int ITableKey.Id => Equip_Set_Id;
    string ITableKey.Key => Equip_Set_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Equip_Set_ID (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Equip_Set_Id = v0;
        // 1: Set_Manager_ID (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Set_Manager_Id = v1;
        // 2: Equip_Set_Need_Name (string)
        if (values.Length > 2) Equip_Set_Need_Name = values[2];
        // 3: Equip_Set_Need_Nember (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Equip_Set_Need_Number = v3;
        // 4: Affection_Equip_Set (Status)
        if (values.Length > 4 && Enum.TryParse(values[4], out Status v4)) Affection_Equip_Set = v4;
        // 5: Affection_Equip_Set_Value (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Affection_Equip_Set_Value = v5;
    }
}
