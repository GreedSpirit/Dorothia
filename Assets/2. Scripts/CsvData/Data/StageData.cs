using UnityEngine;
using System;

[Serializable]
public class StageData : ICSVLoad, ITableKey
{
    public int Stage_Id { get; set; }
    public int Stage_Section_Id { get; set; }
    public int Same_Spawn_Max { get; set; }
    public int Boss_Summon_Dead_Namber { get; set; }
    public int Monster_Spawn_Id { get; set; }

    int ITableKey.Id => Stage_Id;
    string ITableKey.Key => Stage_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Stage_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Stage_Id = v0;
        // 1: Stage_Section_Id (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Stage_Section_Id = v1;
        // 2: Same_Spawn_Max (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Same_Spawn_Max = v2;
        // 3: Boss_Summon_Dead_Namber (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Boss_Summon_Dead_Namber = v3;
        // 4: Monster_Spawn_Id (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Monster_Spawn_Id = v4;
    }
}
