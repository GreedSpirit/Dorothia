using UnityEngine;
using System;

[Serializable]
public class Talk_TableData : ICSVLoad, ITableKey
{
    public int id { get; set; }
    public int next_id { get; set; }
    public int Section_id { get; set; }
    public int output_time { get; set; }
    public string name { get; set; }
    public string line_desc { get; set; }
    public string portrait { get; set; }
    public string cg { get; set; }
    public string cg_item { get; set; }
    public string background { get; set; }
    public string sfx { get; set; }
    public string bgm { get; set; }

    int ITableKey.Id => id;
    string ITableKey.Key => id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) id = v0;
        // 1: next_id (int)
        if (values.Length > 1 && int.TryParse(values[1], out int v1)) next_id = v1;
        // 2: Section_id (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Section_id = v2;
        // 3: output_time (int)
        if (values.Length > 3 && int.TryParse(values[3], out int v3)) output_time = v3;
        // 4: name (string)
        if (values.Length > 4) name = values[4];
        // 5: line_desc (string)
        if (values.Length > 5) line_desc = values[5];
        // 6: portrait (string)
        if (values.Length > 6) portrait = values[6];
        // 7: cg (string)
        if (values.Length > 7) cg = values[7];
        // 8: cg_item (string)
        if (values.Length > 8) cg_item = values[8];
        // 9: background (string)
        if (values.Length > 9) background = values[9];
        // 10: sfx (string)
        if (values.Length > 10) sfx = values[10];
        // 11: bgm (string)
        if (values.Length > 11) bgm = values[11];
    }
}
