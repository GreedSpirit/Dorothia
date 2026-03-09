using UnityEngine;
using System;

[Serializable]
public class Dungeon_StepData : ICSVLoad, ITableKey
{
    public int Dungeon_Step_Id { get; set; }
    public int Dungeon_Id { get; set; }
    public string Rec_Cp { get; set; }
    public string Time_Limit { get; set; }
    public int Monster_Group_Id { get; set; }
    public int Reward_Group_Id { get; set; }

    int ITableKey.Id => Dungeon_Step_Id;
    string ITableKey.Key => Dungeon_Step_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Dungeon_Step_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Dungeon_Step_Id = v0;
        // 1: Dungeon_Id (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Dungeon_Id = v1;
        // 2: Rec_Cp (string)
        if (values.Length > 2) Rec_Cp = values[2];
        // 3: Time_Limit (string)
        if (values.Length > 3) Time_Limit = values[3];
        // 4: Monster_Group_Id (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Monster_Group_Id = v4;
        // 5: Reward_Group_Id (int)
        if (values.Length > 5 && int.TryParse(values[5], out int v5)) Reward_Group_Id = v5;
    }
}
