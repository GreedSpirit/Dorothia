using UnityEngine;
using System;

[Serializable]
public class G_StoneData : ICSVLoad, ITableKey
{
    public int G_Stone_Id { get; set; }
    public string G_Stone_Name { get; set; }
    public string G_Stone_Icon { get; set; }

    int ITableKey.Id => G_Stone_Id;
    string ITableKey.Key => G_Stone_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: G_Stone_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) G_Stone_Id = v0;
        // 1: G_Stone_Name (string)
        if (values.Length > 1) G_Stone_Name = values[1];
        // 2: G_Stone_Icon (string)
        if (values.Length > 2) G_Stone_Icon = values[2];
    }
}
