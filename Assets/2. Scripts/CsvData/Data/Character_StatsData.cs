using UnityEngine;
using System;

[Serializable]
public class Character_StatsData : ICSVLoad, ITableKey
{
    public int Character_Id { get; set; }
    public int Character_Level { get; set; }
    public float Character_Hp { get; set; }
    public float Character_Atk { get; set; }
    public float Character_Atk_M { get; set; }
    public float Character_Dps { get; set; }
    public float Character_Crt_Prob { get; set; }
    public float Character_Crt_Dmg { get; set; }
    public float Character_Def { get; set; }
    public float Character_Def_M { get; set; }
    public float Character_Hp_Regen { get; set; }
    public float Character_Agi { get; set; }
    public int Character_Upgrade_Scrap_N { get; set; }
    public float Character_Level_Exp_N { get; set; }

    int ITableKey.Id => Character_Id;
    string ITableKey.Key => Character_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Character_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Character_Id = v0;
        // 1: Character_Level (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Character_Level = v1;
        // 2: Character_Hp (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Character_Hp = v2;
        // 3: Character_Atk (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Character_Atk = v3;
        // 4: Character_Atk_M (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Character_Atk_M = v4;
        // 5: Character_Dps (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Character_Dps = v5;
        // 6: Character_Crt_Prob (float)
        if (values.Length > 6 && float.TryParse(values[6], out float v6)) Character_Crt_Prob = v6;
        // 7: Character_Crt_Dmg (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Character_Crt_Dmg = v7;
        // 8: Character_Def (float)
        if (values.Length > 8 && float.TryParse(values[8], out float v8)) Character_Def = v8;
        // 9: Character_Def_M (float)
        if (values.Length > 9 && float.TryParse(values[9], out float v9)) Character_Def_M = v9;
        // 10: Character_Hp_Regen (float)
        if (values.Length > 10 && float.TryParse(values[10], out float v10)) Character_Hp_Regen = v10;
        // 11: Character_Agi (float)
        if (values.Length > 11 && float.TryParse(values[11], out float v11)) Character_Agi = v11;
        // 12: Character_Upgrade_Scrap_N (int)
        if (values.Length > 12 && int.TryParse(values[12], out int v12)) Character_Upgrade_Scrap_N = v12;
        // 13: Character_Level_Exp_N (float)
        if (values.Length > 13 && float.TryParse(values[13], out float v13)) Character_Level_Exp_N = v13;
    }
}
