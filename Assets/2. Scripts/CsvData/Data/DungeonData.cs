using UnityEngine;
using System;

[Serializable]
public class DungeonData : ICSVLoad, ITableKey
{
    public int Dungeon_Id { get; set; }
    public Dungeon_Type Dungeon_Type { get; set; }
    public string Dungeon_Name { get; set; }
    public int Daily_Entry { get; set; }
    public int Dungeon_Unlock { get; set; }
    public string Dungeon_Map { get; set; }
    public string Dungeon_Sfx { get; set; }
    public string Dungeon_Bgm { get; set; }
    public string Bgm_Win { get; set; }
    public string Bgm_Lose { get; set; }

    int ITableKey.Id => Dungeon_Id;
    string ITableKey.Key => Dungeon_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Dungeon_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Dungeon_Id = v0;
        // 1: Dungeon_Type (enum)
        // TODO: enum 타입 파싱 로직 추가 필요
        if (values.Length > 1 && Enum.TryParse(values[1], out Dungeon_Type v1)) Dungeon_Type = v1;
        // 2: Dungeon_Name (string)
        if (values.Length > 2) Dungeon_Name = values[2];
        // 3: Daily_Entry (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Daily_Entry = v3;
        // 4: Dungeon_Unlock (int)
        if (values.Length > 4 && int.TryParse(values[4], out int v4)) Dungeon_Unlock = v4;
        // 5: Dungeon_Map (string)
        if (values.Length > 5) Dungeon_Map = values[5];
        // 6: Dungeon_Sfx (string)
        if (values.Length > 6) Dungeon_Sfx = values[6];
        // 7: Dungeon_Bgm (string)
        if (values.Length > 7) Dungeon_Bgm = values[7];
        // 8: Bgm_Win (string)
        if (values.Length > 8) Bgm_Win = values[8];
        // 9: Bgm_Lose (string)
        if (values.Length > 9) Bgm_Lose = values[9];
    }
}
