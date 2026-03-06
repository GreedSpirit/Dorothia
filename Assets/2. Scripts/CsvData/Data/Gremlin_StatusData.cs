using UnityEngine;
using System;

[Serializable]
public class Gremlin_StatusData : ICSVLoad, ITableKey
{
    public int Gremlin_Id { get; set; }
    public float Gremlin_Atk { get; set; }
    public float Gremlin_Atk_M { get; set; }
    public float Gremlin_Dps { get; set; }
    public Rarity Gremlin_Buff { get; set; }
    public float Gremlin_Cooltime { get; set; }

    int ITableKey.Id => Gremlin_Id;
    string ITableKey.Key => Gremlin_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gramlin_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Gremlin_Id = v0;
        // 1: Gramlin_Atk (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Gremlin_Atk = v1;
        // 2: Gramlin_Atk_M (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Gremlin_Atk_M = v2;
        // 3: Gramlin_Dps (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Gremlin_Dps = v3;
        // 4: Gramlin_Buff (Rarity)
        if (values.Length > 4 && Enum.TryParse(values[4], out Rarity v4)) Gremlin_Buff = v4;
        // 5: Gramlin_Cooltime (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Gremlin_Cooltime = v5;
    }
}
