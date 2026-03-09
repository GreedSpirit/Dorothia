using UnityEngine;
using System;

[Serializable]
public class ConsumData : ICSVLoad, ITableKey
{
    public int Consum_Id { get; set; }
    public string Consum_Name { get; set; }
    public string Consum_Icon { get; set; }

    int ITableKey.Id => Consum_Id;
    string ITableKey.Key => Consum_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Consum_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Consum_Id = v0;
        // 1: Consum_Name (string)
        if (values.Length > 1) Consum_Name = values[1];
        // 2: Consum_Icon (string)
        if (values.Length > 2) Consum_Icon = values[2];
    }
}
