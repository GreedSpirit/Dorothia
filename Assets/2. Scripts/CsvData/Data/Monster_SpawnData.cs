using UnityEngine;
using System;

[Serializable]
public class Monster_SpawnData : ICSVLoad, ITableKey
{
    public int Monster_Spawn_Id { get; set; }
    public int Spawn_Stage_Start { get; set; }
    public int Spawn_Stage_End { get; set; }
    public int Monster_Id_1 { get; set; }
    public int Monster_Number_1 { get; set; }
    public int Monster_Id_2 { get; set; }
    public int Monster_Number_2 { get; set; }
    public int Monster_Id_3 { get; set; }
    public int Monster_Number_3 { get; set; }
    public int Monster_Id_4 { get; set; }
    public int Monster_Number_4 { get; set; }
    public int Monster_Id_5 { get; set; }
    public int Monster_Number_5 { get; set; }
    public int Monster_Id_6 { get; set; }
    public int Monster_Number_6 { get; set; }
    public int Monster_Id_7 { get; set; }
    public int Monster_Number_7 { get; set; }
    public int Monster_Id_8 { get; set; }
    public int Monster_Number_8 { get; set; }

    int ITableKey.Id => Monster_Spawn_Id;
    string ITableKey.Key => Monster_Spawn_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Monster_Spawn_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Monster_Spawn_Id = v0;
        // 1: Spawn_Stage_Start (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Spawn_Stage_Start = v1;
        // 2: Spawn_Stage_End (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Spawn_Stage_End = v2;
        // 3: Monster_Id_1 (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Monster_Id_1 = v3;
        // 4: Monster_Number_1 (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Monster_Number_1 = v4;
        // 5: Monster_Id_2 (int)
        if (values.Length > 5 && int.TryParse(values[5], out int v5)) Monster_Id_2 = v5;
        // 6: Monster_Number_2 (int)
        if (values.Length > 6 && int.TryParse(values[6], out int v6)) Monster_Number_2 = v6;
        // 7: Monster_Id_3 (int)
        if (values.Length > 7 && int.TryParse(values[7], out int v7)) Monster_Id_3 = v7;
        // 8: Monster_Number_3 (int)
        if (values.Length > 8 && int.TryParse(values[8], out int v8)) Monster_Number_3 = v8;
        // 9: Monster_Id_4 (int)
        if (values.Length > 9 && int.TryParse(values[9], out int v9)) Monster_Id_4 = v9;
        // 10: Monster_Number_4 (int)
        if (values.Length > 10 && int.TryParse(values[10], out int v10)) Monster_Number_4 = v10;
        // 11: Monster_Id_5 (int)
        if (values.Length > 11 && int.TryParse(values[11], out int v11)) Monster_Id_5 = v11;
        // 12: Monster_Number_5 (int)
        if (values.Length > 12 && int.TryParse(values[12], out int v12)) Monster_Number_5 = v12;
        // 13: Monster_Id_6 (int)
        if (values.Length > 13 && int.TryParse(values[13], out int v13)) Monster_Id_6 = v13;
        // 14: Monster_Number_6 (int)
        if (values.Length > 14 && int.TryParse(values[14], out int v14)) Monster_Number_6 = v14;
        // 15: Monster_Id_7 (int)
        if (values.Length > 15 && int.TryParse(values[15], out int v15)) Monster_Id_7 = v15;
        // 16: Monster_Number_7 (int)
        if (values.Length > 16 && int.TryParse(values[16], out int v16)) Monster_Number_7 = v16;
        // 17: Monster_Id_8 (int)
        if (values.Length > 17 && int.TryParse(values[17], out int v17)) Monster_Id_8 = v17;
        // 18: Monster_Number_8 (int)
        if (values.Length > 18 && int.TryParse(values[18], out int v18)) Monster_Number_8 = v18;
    }
}
