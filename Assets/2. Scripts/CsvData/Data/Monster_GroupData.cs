using UnityEngine;
using System;

[Serializable]
public class Monster_GroupData : ICSVLoad, ITableKey
{
    public int Monster_Group_Id { get; set; }
    public int Monster_Wave { get; set; }
    public int Monster_Id { get; set; }
    public int Spawn_Num { get; set; }

    int ITableKey.Id => Monster_Group_Id;
    string ITableKey.Key => Monster_Group_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Monster_Group_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Monster_Group_Id = v0;
        // 1: Monster_Wave (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Monster_Wave = v1;
        // 2: Monster_Id (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Monster_Id = v2;
        // 3: Spawn_Num (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Spawn_Num = v3;
    }
}
