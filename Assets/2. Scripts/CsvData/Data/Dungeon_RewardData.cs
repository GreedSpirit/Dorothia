using UnityEngine;
using System;

[Serializable]
public class Dungeon_RewardData : ICSVLoad, ITableKey
{
    public int Reward_Group_Id { get; set; }
    public Dungeon_Type Dungeon_Type { get; set; }
    public int Consum_Id { get; set; }
    public Rarity Reward_Rank { get; set; }
    public int Reward_Min { get; set; }
    public int Reward_Max { get; set; }

    int ITableKey.Id => Reward_Group_Id;
    string ITableKey.Key => Reward_Group_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Reward_Group_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Reward_Group_Id = v0;
        // 1: Dungeon_Type (enum)
        // TODO: enum 타입 파싱 로직 추가 필요
        if (values.Length > 1 && Enum.TryParse(values[1], out Dungeon_Type v1)) Dungeon_Type = v1;
        // 2: Consum_Id (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Consum_Id = v2;
        // 3: Reward_Rank (enum)
        // TODO: enum 타입 파싱 로직 추가 필요
        if (values.Length > 3 && Enum.TryParse(values[3], out Rarity v3)) Reward_Rank = v3;
        // 4: Reward_Min (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Reward_Min = v4;
        // 5: Reward_Max (int)
        if (values.Length > 5 && int.TryParse(values[5], out int v5)) Reward_Max = v5;
    }
}
