using UnityEngine;
using System;

[Serializable]
public class Stage_RewardData : ICSVLoad, ITableKey
{
    public int Stage_Id { get; set; }
    public int Stage_Gold { get; set; }
    public float Stage_Gold_Value { get; set; }
    public float Stage_Exp { get; set; }
    public float Stage_Exp_Value { get; set; }
    public float Stage_Orb { get; set; }

    int ITableKey.Id => Stage_Id;
    string ITableKey.Key => Stage_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Stage_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Stage_Id = v0;
        // 1: Stage_Gold (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Stage_Gold = v1;
        // 2: Stage_Gold_Value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Stage_Gold_Value = v2;
        // 3: Stage_Exp (float)
        if (values.Length > 3 && float.TryParse(values[3], out float v3)) Stage_Exp = v3;
        // 4: Stage_Exp_Value (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Stage_Exp_Value = v4;
        // 5: Stage_Orb (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Stage_Orb = v5;
    }
}
