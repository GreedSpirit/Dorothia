using UnityEngine;
using System;

[Serializable]
public class CharacterData : ICSVLoad, ITableKey
{
    public int Character_Id { get; set; }
    public string Character_Job { get; set; }
    public string Character_Model { get; set; }
    public string Character_Icon_1 { get; set; }
    public string Character_Stand_1 { get; set; }
    public string Character_Icon_2 { get; set; }
    public string Character_Stand_2 { get; set; }
    public string Character_Icon_3 { get; set; }
    public string Character_Stand_3 { get; set; }
    public string Character_Icon_4 { get; set; }
    public string Character_Stand_4 { get; set; }
    public string Character_Icon_5 { get; set; }
    public string Character_Stand_5 { get; set; }
    public string Character_Icon_6 { get; set; }
    public string Character_Stand_6 { get; set; }
    public string Character_Icon_7 { get; set; }
    public string Character_Stand_7 { get; set; }
    public string Character_Icon_8 { get; set; }
    public string Character_Stand_8 { get; set; }

    int ITableKey.Id => Character_Id;
    string ITableKey.Key => Character_Id.ToString();

    public void LoadFromCsv(string[] values)
    {
        // 0: Character_Id (int)
        if (values.Length > 0 && int.TryParse(values[0], out int v0)) Character_Id = v0;
        // 1: Character_Job (string)
        if (values.Length > 1) Character_Job = values[1];
        // 2: Character_Model (string)
        if (values.Length > 2) Character_Model = values[2];
        // 3: Character_Icon_1 (string)
        if (values.Length > 3) Character_Icon_1 = values[3];
        // 4: Character_Stand_1 (string)
        if (values.Length > 4) Character_Stand_1 = values[4];
        // 5: Character_Icon_2 (string)
        if (values.Length > 5) Character_Icon_2 = values[5];
        // 6: Character_Stand_2 (string)
        if (values.Length > 6) Character_Stand_2 = values[6];
        // 7: Character_Icon_3 (string)
        if (values.Length > 7) Character_Icon_3 = values[7];
        // 8: Character_Stand_3 (string)
        if (values.Length > 8) Character_Stand_3 = values[8];
        // 9: Character_Icon_4 (string)
        if (values.Length > 9) Character_Icon_4 = values[9];
        // 10: Character_Stand_4 (string)
        if (values.Length > 10) Character_Stand_4 = values[10];
        // 11: Character_Icon_5 (string)
        if (values.Length > 11) Character_Icon_5 = values[11];
        // 12: Character_Stand_5 (string)
        if (values.Length > 12) Character_Stand_5 = values[12];
        // 13: Character_Icon_6 (string)
        if (values.Length > 13) Character_Icon_6 = values[13];
        // 14: Character_Stand_6 (string)
        if (values.Length > 14) Character_Stand_6 = values[14];
        // 15: Character_Icon_7 (string)
        if (values.Length > 15) Character_Icon_7 = values[15];
        // 16: Character_Stand_7 (string)
        if (values.Length > 16) Character_Stand_7 = values[16];
        // 17: Character_Icon_8 (string)
        if (values.Length > 17) Character_Icon_8 = values[17];
        // 18: Character_Stand_8 (string)
        if (values.Length > 18) Character_Stand_8 = values[18];
    }
}
