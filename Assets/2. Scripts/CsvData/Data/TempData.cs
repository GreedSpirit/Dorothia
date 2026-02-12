[System.Serializable]
public class TempData : ICSVLoad, ITableKey
{
    //프로퍼티 엑셀 컬럼명과 일치시켜서 만들기
    public int Level { get; set; }
    public float NeedExp { get; set; }

    // Level로 ID를 대체해서 사용
    int ITableKey.Id
    {
        get { return Level; }
    }

    // 별도의 Key 문자열이 없으므로 문자열로 변환해서 사용
    string ITableKey.Key
    {
        get {return Level.ToString();}
    }

    public void LoadFromCsv(string[] values)
    {
        // 0: Level (int)
        if (int.TryParse(values[0], out int levelValue))
            Level = levelValue;
        else
            Level = 0;

        // 1: NeedExp (int)
        if (float.TryParse(values[1], out float expValue))
            NeedExp = expValue;
        else
            NeedExp = 0;
    }
}
