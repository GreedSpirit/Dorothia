using UnityEngine;
using System;

[Serializable]
public class Sk_SclData : ICSVLoad, ITableKey
{
    public int Sk_Scl_Id { get; set; }
    public string Sk_Scl_Name { get; set; }
    public string Sk_Scl_Icon { get; set; }

    int ITableKey.Id => Sk_Scl_Id;
    string ITableKey.Key => Sk_Scl_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Sk_Scl_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Sk_Scl_Id = v0;
        // 1: Sk_Scl_Name (string)
        if (values.Length > 1) Sk_Scl_Name = values[1];
        // 2: Sk_Scl_Icon (string)
        if (values.Length > 2) Sk_Scl_Icon = values[2];
    }
}
