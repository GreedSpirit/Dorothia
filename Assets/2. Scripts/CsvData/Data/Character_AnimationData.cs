using UnityEngine;
using System;

[Serializable]
public class Character_AnimationData : ICSVLoad, ITableKey
{
    public int Character_Animation_Id { get; set; }
    public string Character_Animation_Idle { get; set; }
    public string Character_Animation_Run { get; set; }
    public string Character_Animation_Dead { get; set; }

    int ITableKey.Id => Character_Animation_Id;
    string ITableKey.Key => Character_Animation_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Character_Animation_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Character_Animation_Id = v0;
        // 1: Character_Animation_Idle (string)
        if (values.Length > 1) Character_Animation_Idle = values[1];
        // 2: Character_Animation_Run (string)
        if (values.Length > 2) Character_Animation_Run = values[2];
        // 3: Character_Animation_Dead (string)
        if (values.Length > 3) Character_Animation_Dead = values[3];
    }
}
