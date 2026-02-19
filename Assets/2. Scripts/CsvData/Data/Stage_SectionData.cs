using UnityEngine;
using System;

[Serializable]
public class Stage_SectionData : ICSVLoad, ITableKey
{
    public int Stage_Section_Id { get; set; }
    public int Section_Start { get; set; }
    public int Section_End { get; set; }
    public int Equip_Drop_Level { get; set; }
    public float Equip_Drop_Prob { get; set; }

    int ITableKey.Id => Stage_Section_Id;
    string ITableKey.Key => Stage_Section_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Stage_Section_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Stage_Section_Id = v0;
        // 1: Section_Start (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) Section_Start = v1;
        // 2: Section_End (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Section_End = v2;
        // 3: Equip_Drop_Level (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) Equip_Drop_Level = v3;
        // 4: Equip_Drop_Prob (float)
        if (values.Length > 4 && float.TryParse(values[4], out float v4)) Equip_Drop_Prob = v4;
    }
}
