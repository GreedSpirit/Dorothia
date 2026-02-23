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

    [Header("Monster Prefabs")]
    [SerializeField] private List<Entry> _entries = new();

    //런타임 조회용 캐시 (monsterId -> Prefab)
    private readonly Dictionary<int, MonsterController> _map = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[MonsterPrefabRegistry] Duplicate instance detected. Destroying.");
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
    /// Inspector Entries -> Dictionary 캐시 생성
    /// 중복 monsterId는 경고 후 무시(첫 등록 유지)
    /// </summary>
    private void BuildCache()
    {
        _map.Clear();

        for (int i = 0; i < _entries.Count; i++)
        {
            var e = _entries[i];
            if (e == null) continue;

            if (e.monsterId <= 0)
                continue;

            if (e.prefab == null)
                continue;

            if (_map.ContainsKey(e.monsterId))
            {
                Debug.LogWarning($"[MonsterPrefabRegistry] 중복 {e.monsterId}");
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