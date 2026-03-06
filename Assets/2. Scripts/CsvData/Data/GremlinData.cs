using UnityEngine;
using System;

[Serializable]
public class GremlinData : ICSVLoad, ITableKey
{
    public int Gramlin_Id { get; set; }
    public string Gramlin_Name { get; set; }
    public Rarity Gremlin_Tier { get; set; }
    public string Gramlin_Model { get; set; }
    public string Gramlin_Icon { get; set; }

    int ITableKey.Id => Gramlin_Id;
    string ITableKey.Key => Gramlin_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Gremlin_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Gramlin_Id = v0;
        // 1: Gremlin_Name (string)
        if (values.Length > 1) Gramlin_Name = values[1];
        // 2: Gremlin_Type (Rarity)
        if (values.Length > 2 && Enum.TryParse(values[2], out Rarity v2)) Gremlin_Tier = v2;
        // 3: Gremlin_Model (string)
        if (values.Length > 3) Gramlin_Model = values[3];
        // 4: Gremlin_Icon (string)
        if (values.Length > 4) Gramlin_Icon = values[4];
    }
}
