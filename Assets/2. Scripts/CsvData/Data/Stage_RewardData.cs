using System;
using System.Numerics;
using UnityEngine;

[Serializable]
public class Stage_RewardData : ICSVLoad, ITableKey
{
    public int Section_Id { get; set; }
    public BigInteger Section_Gold { get; set; }
    public float Section_Gold_Value { get; set; }
    public BigInteger Section_Exp { get; set; }
    public float Section_Exp_Value { get; set; }
    public float Section_Orb { get; set; }

    int ITableKey.Id => Section_Id;
    string ITableKey.Key => Section_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Section_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Section_Id = v0;
        // 1: Section_Gold (int)
        if (values.Length > 1 && BigInteger.TryParse(values[1], out BigInteger v1)) Section_Gold = v1;
        // 2: Section_Gold_Value (float)
        if (values.Length > 2 && float.TryParse(values[2], out float v2)) Section_Gold_Value = v2;
        // 3: Section_Exp (string)
        //if (values.Length > 3) Section_Exp = values[3];
        if (values.Length > 3 && BigInteger.TryParse(values[3], out BigInteger v3)) Section_Exp = v3;
        // 4: Section_Exp_Value (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Section_Exp_Value = v4;
        // 5: Section_Orb (float)
        if (values.Length > 5 && float.TryParse(values[5], out float v5)) Section_Orb = v5;
    }
}
