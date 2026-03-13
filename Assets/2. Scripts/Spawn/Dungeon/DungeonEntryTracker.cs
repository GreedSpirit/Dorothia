using UnityEngine;

/// <summary>
/// 던전 일일 입장 횟수 관리
/// 현재는 PlayerPrefs 기반 임시 저장
/// 추후 SaveManager 연동 시 이 클래스 내부만 교체하면 됨
/// </summary>
public static class DungeonEntryTracker
{
    private const string DateKey = "DUNGEON_ENTRY_RESET_DATE";
    private const string EntryKeyPrefix = "DUNGEON_ENTRY_USED_";

    private static string TodayKey => System.DateTime.Now.ToString("yyyyMMdd");

    private static void ValidateReset()
    {
        string savedDate = PlayerPrefs.GetString(DateKey, string.Empty);

        if (savedDate == TodayKey)
            return;

        //날짜가 바뀌면 오늘 기준으로 리셋
        PlayerPrefs.SetString(DateKey, TodayKey);
        PlayerPrefs.Save();
    }

    private static string GetEntryKey(int dungeonId)
    {
        return $"{EntryKeyPrefix}{dungeonId}";
    }

    public static int GetUsedCount(int dungeonId)
    {
        ValidateReset();
        return PlayerPrefs.GetInt(GetEntryKey(dungeonId), 0);
    }

    public static int GetRemainCount(int dungeonId, int maxEntry)
    {
        int used = GetUsedCount(dungeonId);
        return Mathf.Max(0, maxEntry - used);
    }

    public static bool CanEnter(int dungeonId, int maxEntry)
    {
        return GetRemainCount(dungeonId, maxEntry) > 0;
    }

    public static bool TryConsumeEntry(int dungeonId, int maxEntry, out int usedCount, out int remainCount)
    {
        ValidateReset();

        usedCount = GetUsedCount(dungeonId);
        remainCount = Mathf.Max(0, maxEntry - usedCount);

        if (remainCount <= 0)
            return false;

        usedCount++;
        remainCount = Mathf.Max(0, maxEntry - usedCount);

        PlayerPrefs.SetInt(GetEntryKey(dungeonId), usedCount);
        PlayerPrefs.Save();

        return true;
    }

    public static void ForceSetUsedCount(int dungeonId, int usedCount)
    {
        ValidateReset();
        PlayerPrefs.SetInt(GetEntryKey(dungeonId), Mathf.Max(0, usedCount));
        PlayerPrefs.Save();
    }
}