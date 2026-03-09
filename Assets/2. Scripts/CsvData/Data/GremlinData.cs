using UnityEngine;
using System;

[Serializable]
public class GremlinData : ICSVLoad, ITableKey
{
    public int Gremlin_Id { get; set; }
    public string Gremlin_Name { get; set; }
    public Gremlin_Type Gremlin_Type { get; set; }
    public string Gremlin_Model { get; set; }
    public string Gremlin_Icon { get; set; }

    int ITableKey.Id => Gremlin_Id;
    string ITableKey.Key => Gremlin_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gremlin_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Gremlin_Id = v0;
        // 1: Gremlin_Name (string)
        if (values.Length > 1) Gremlin_Name = values[1];
        // 2: Gremlin_Type (Rarity)
        if (values.Length > 2 && Enum.TryParse(values[2], out Gremlin_Type v2)) Gremlin_Type = v2;
        // 3: Gremlin_Model (string)
        if (values.Length > 3) Gremlin_Model = values[3];
        // 4: Gremlin_Icon (string)
        if (values.Length > 4) Gremlin_Icon = values[4];
    }
}
