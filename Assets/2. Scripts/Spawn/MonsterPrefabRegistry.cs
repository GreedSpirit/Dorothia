using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MonsterId -> MonsterController Prefab 매핑용
/// SpawnManager가 CSV의 monsterId로 프리팹을 찾기 위해 사용
/// </summary>
public class MonsterPrefabRegistry : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public int monsterId;
        public MonsterController prefab;
    }

    public static MonsterPrefabRegistry Instance { get; private set; }

    [Header("Normal Monsters")]
    [SerializeField] private List<Entry> _normalEntries = new();

    [Header("Elite Monsters")]
    [SerializeField] private List<Entry> _eliteEntries = new();

    [Header("Boss Monsters")]
    [SerializeField] private List<Entry> _bossEntries = new();

    //런타임 조회용 캐시 (monsterId -> Prefab)
    private readonly Dictionary<int, MonsterController> _map = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildCache();
    }

    private void OnValidate()
    {
        //에디터에서 값 바꿀 때 캐시 빌드(플레이 중엔 Awake에서 다시 빌드)
        if (!Application.isPlaying)
            BuildCache();
    }

    /// <summary>
    /// Inspector 데이터 -> Dictionary 캐시 생성
    /// </summary>
    private void BuildCache()
    {
        _map.Clear();

        AddEntries(_normalEntries, "Normal");
        AddEntries(_eliteEntries, "Elite");
        AddEntries(_bossEntries, "Boss");
    }

    /// <summary>
    /// 리스트를 캐시에 등록
    /// </summary>
    /// <param name="list"></param>
    /// <param name="category"></param>
    private void AddEntries(List<Entry> list, string category)
    {
        if (list == null)
            return;

        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];

            if (e == null)
                continue;

            if (e.monsterId <= 0)
                continue;

            if (e.prefab == null)
                continue;

            if (_map.ContainsKey(e.monsterId))
            {
                Debug.LogWarning($"[MonsterPrefabRegistry] 중복 MonsterId " +
                    $"({category}) : {e.monsterId}");
                continue;
            }

            _map.Add(e.monsterId, e.prefab);
        }
    }

    /// <summary>
    /// SpawnManager가 monsterId로 프리팹을 얻어오는 API
    /// </summary>
    /// <param name="monsterId"></param>
    /// <returns></returns>
    public MonsterController GetPrefab(int monsterId)
    {
        if (monsterId <= 0)
            return null;

        if (_map.TryGetValue(monsterId, out var prefab))
            return prefab;

        Debug.LogError($"[MonsterPrefabRegistry] 프리팹 없음 {monsterId}");
        return null;
    }
}