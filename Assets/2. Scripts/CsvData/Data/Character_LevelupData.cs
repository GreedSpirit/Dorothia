using UnityEngine;
using System;

[Serializable]
public class Character_LevelupData : ICSVLoad, ITableKey
{
    public int Character_Id { get; set; }
    public float Character_Level_Hp { get; set; }
    public float Character_Level_Atk { get; set; }
    public float Character_Level_Atk_M { get; set; }
    public float Character_Level_Dps { get; set; }
    public float Character_Level_Crt_Prob { get; set; }
    public float Character_Level_Crt_Dmg { get; set; }
    public float Character_Level_Def { get; set; }
    public float Character_Level_Def_M { get; set; }
    public float Character_Level_Hp_Regen { get; set; }
    public float Character_Level_Agi { get; set; }

    int ITableKey.Id => Character_Id;
    string ITableKey.Key => Character_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Character_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Character_Id = v0;
        // 1: Character_Level_Hp (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Character_Level_Hp = v1;
        // 2: Character_Level_Atk (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Character_Level_Atk = v2;
        // 3: Character_Level_Atk_M (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Character_Level_Atk_M = v3;
        // 4: Character_Level_Dps (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Character_Level_Dps = v4;
        // 5: Character_Level_Crt_Prob (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Character_Level_Crt_Prob = v5;
        // 6: Character_Level_Crt_Dmg (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Character_Level_Crt_Dmg = v6;
        // 7: Character_Level_Def (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Character_Level_Def = v7;
        // 8: Character_Level_Def_M (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) Character_Level_Def_M = v8;
        // 9: Character_Level_Hp_Regen (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) Character_Level_Hp_Regen = v9;
        // 10: Character_Level_Agi (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Character_Level_Agi = v10;
    }
}
