using UnityEngine;
using System;
using System.Numerics;

[Serializable]
public class Dungeon_RewardData : ICSVLoad, ITableKey
{
    public int Reward_Group_Id { get; set; }
    public Dungeon_Type Dungeon_Type { get; set; }
    public int Consum_Id { get; set; }
    public Reward_Rank Reward_Rank { get; set; }
    public BigInteger Reward_Min { get; set; }
    public BigInteger Reward_Max { get; set; }

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
        if (values.Length > 3 && Enum.TryParse(values[3], out Reward_Rank v3)) Reward_Rank = v3;
        // 4: Reward_Min (string)
        if (values.Length > 4 && BigInteger.TryParse(values[4], out BigInteger v4)) Reward_Min = v4;
        // 5: Reward_Max (string)
        if (values.Length > 5 && BigInteger.TryParse(values[5], out BigInteger v5)) Reward_Max = v5;
    }
}
