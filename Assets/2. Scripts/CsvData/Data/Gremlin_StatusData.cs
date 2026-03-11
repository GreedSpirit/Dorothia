using UnityEngine;
using System;

[Serializable]
public class Gremlin_StatusData : ICSVLoad, ITableKey
{
    public int Gremlin_Id { get; set; }
    public float Gremlin_Atk { get; set; }
    public float Gremlin_Atk_M { get; set; }
    public float Gremlin_Dps { get; set; }
    public Status Gremlin_Buff { get; set; }
    public float Gremlin_Cooltime { get; set; }
    public Effect_Type Effect_Type { get; set; }
    public float Buff_Value { get; set; }
    public Target_Type Target_Type { get; set; }

    int ITableKey.Id => Gremlin_Id;
    string ITableKey.Key => Gremlin_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gremlin_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Gremlin_Id = v0;
        // 1: Gremlin_Atk (float)
        if (values.Length > 1 && float.TryParse(values[1], out float v1)) Gremlin_Atk = v1;
        // 2: Gremlin_Atk_M (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Gremlin_Atk_M = v2;
        // 3: Gremlin_Dps (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Gremlin_Dps = v3;
        // 4: Gremlin_Buff (enum)
        // TODO: enum 타입 파싱 로직 추가 필요
        if (values.Length > 4 && Enum.TryParse(values[4], out Status v4)) Gremlin_Buff = v4;
        // 5: Gremlin_Cooltime (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Gremlin_Cooltime = v5;
        // 6: Effect_Type (enum)
        // TODO: enum 타입 파싱 로직 추가 필요
        if (values.Length > 6 && Enum.TryParse(values[6], out Effect_Type v6)) Effect_Type = v6;
        // 7: Buff_Value (float)
        if (values.Length > 7 && float.TryParse(values[7], out float v7)) Buff_Value = v7;
        // 8: Target_Type (enum)
        // TODO: enum 타입 파싱 로직 추가 필요
        if (values.Length > 8 && Enum.TryParse(values[8], out Target_Type v8)) Target_Type = v8;
    }
}
