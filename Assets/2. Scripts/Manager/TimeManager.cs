using UnityEngine;
using System.Collections.Generic;
using System;

public enum TimerType
{
    Boss,
    Dungeon,
    Skill
}

/// <summary>
/// 게임 전체 시간 관리
/// </summary>
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    private float _startTime;

    private DateTime _lastQuitTime; // 마지막 종료 시간

    private Dictionary<TimerType, float> _timerStartDict = new(); // 타이머들 관리

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _startTime = Time.time;
    }

    /// <summary>
    /// 현재 게임 시간 (절대시간)
    /// </summary>
    public float Now => Time.time;

    /// <summary>
    /// 타이머 시작
    /// </summary>
    /// <param name="type"></param>
    public void StartTimer(TimerType type)
    {
        _timerStartDict[type] = Time.time;
    }

    /// <summary>
    /// 타이머 종료 (경과시간 반환)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float StopTimer(TimerType type)
    {
        if (!_timerStartDict.TryGetValue(type, out float start))
            return 0f;

        float elapsed = Time.time - start;
        _timerStartDict.Remove(type);
        return elapsed;
    }

    /// <summary>
    /// 현재 진행중 타이머 시간
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public float GetElapsed(TimerType type)
    {
        if (!_timerStartDict.TryGetValue(type, out float start))
            return 0f;

        return Time.time - start;
    }

    /// <summary>
    /// 특정 시간 기록
    /// </summary>
    /// <returns></returns>
    public float RecordTime()
    {
        return Time.time;
    }

    /// <summary>
    /// 기록된 시간 간 차이
    /// </summary>
    /// <param name="startTime"></param>
    /// <returns></returns>
    public float GetDelta(float startTime)
    {
        return Time.time - startTime;
    }

    #region 저장
    /*
     게임 시작시
        double offlineSeconds = TimeManager.Instance.GetOfflineSeconds();
        Debug.Log($"방치 시간: {offlineSeconds}초");

     보상 계산
        BigInteger gold = (BigInteger)(offlineSeconds * goldPerSecond);

     종료 또는 저장 시점
        TimeManager.Instance.SaveQuitTime();
     */

    /// <summary>
    /// 저장
    /// </summary>
    public void SaveQuitTime()
    {
        _lastQuitTime = DateTime.UtcNow;

        PlayerPrefs.SetString("LastQuitTime", _lastQuitTime.ToBinary().ToString());
    }

    /// <summary>
    /// 로드
    /// </summary>
    /// <returns></returns>
    public DateTime LoadQuitTime()
    {
        if (!PlayerPrefs.HasKey("LastQuitTime"))
            return DateTime.UtcNow;

        long binary = Convert.ToInt64(PlayerPrefs.GetString("LastQuitTime"));
        return DateTime.FromBinary(binary);
    }

    /// <summary>
    /// 방치 시간 계산
    /// </summary>
    /// <returns></returns>
    public double GetOfflineSeconds()
    {
        DateTime last = LoadQuitTime();
        DateTime now = DateTime.UtcNow;

        return (now - last).TotalSeconds;
    }
    #endregion
}