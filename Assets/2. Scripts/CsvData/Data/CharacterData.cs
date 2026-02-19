using UnityEngine;
using System;

[Serializable]
public class CharacterData : ICSVLoad, ITableKey
{
    public int Character_Id { get; set; }
    public string Character_Job { get; set; }
    public int Character_Animation_Id { get; set; }
    public string Character_Model { get; set; }

    int ITableKey.Id => Character_Id;
    string ITableKey.Key => Character_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Character_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Character_Id = v0;
        // 1: Character_Job (string)
        if (values.Length > 1) Character_Job = values[1];
        // 2: Character_Animation_Id (int)
        if (values.Length > 2 && int.TryParse(values[2], out int v2)) Character_Animation_Id = v2;
        // 3: Character_Model (string)
        if (values.Length > 3) Character_Model = values[3];
    }
}
