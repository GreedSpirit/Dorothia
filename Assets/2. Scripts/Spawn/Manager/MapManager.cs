using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _mapRoot;

    [Header("Stage Map Table")]
    [SerializeField] private List<StageMapEntry> _stageMaps;

    [Header("Dungeon Map Table")]
    [SerializeField] private List<DungeonMapEntry> _dungeonMaps;

    // StageId -> MapPrefab 조회용
    private readonly Dictionary<int, GameObject> _stageMapTable = new();
    private readonly Dictionary<int, GameObject> _dungeonMapTable = new();

    private GameObject _currentMapInstance;

    private int _currentStageId = -1;
    private int _currentDungeonId = -1;

    private MonsterSpawnManager _spawnManager;

    private void Awake()
    {
        //MapRoot가 없으면 자동으로 자기 자신 사용
        if (_mapRoot == null)
        {
            Debug.LogWarning("[MapManager] MapRoot가 설정되지 않아 MapManager Transform을 사용");
            _mapRoot = transform;
        }

        BuildStageMapTable();
        BuildDungeonMapTable();

        //MonsterSpawnManager 캐싱
        _spawnManager = FindAnyObjectByType<MonsterSpawnManager>();

        if (_spawnManager == null)
            Debug.LogError("[MapManager] MonsterSpawnManager 없음.");
    }

    private void OnEnable()
    {
        StageManager.OnStageIdChanged += HandleStageChanged;
    }

    private void OnDisable()
    {
        StageManager.OnStageIdChanged -= HandleStageChanged;
    }

    private void BuildStageMapTable()
    {
        _stageMapTable.Clear();

        foreach (var entry in _stageMaps)
        {
            if (entry == null || entry.MapPrefab == null)
                continue;

            if (!_stageMapTable.ContainsKey(entry.StageId))
                _stageMapTable.Add(entry.StageId, entry.MapPrefab);
            else
                Debug.LogWarning($"[MapManager] 중복 StageId 발견 : {entry.StageId}");
        }
    }

    private void BuildDungeonMapTable()
    {
        _dungeonMapTable.Clear();

        foreach (var entry in _dungeonMaps)
        {
            if (entry == null || entry.MapPrefab == null)
                continue;

            if (!_dungeonMapTable.ContainsKey(entry.DungeonId))
                _dungeonMapTable.Add(entry.DungeonId, entry.MapPrefab);
            else
                Debug.LogWarning($"[MapManager] 중복 DungeonId 발견 : {entry.DungeonId}");
        }
    }

    /// <summary>
    /// StageManager에서 StageId 변경 이벤트 수신
    /// </summary>
    /// <param name="stageId"></param>
    private void HandleStageChanged(int stageId)
    {
        LoadStageMap(stageId);
    }

    /// <summary>
    /// StageId에 해당하는 맵 로드
    /// </summary>
    /// <param name="stageId"></param>
    public void LoadStageMap(int stageId)
    {
        //이미 로드된 맵이면 무시
        if (_currentStageId == stageId)
            return;

        if (!_stageMapTable.TryGetValue(stageId, out GameObject prefab))
        {
            Debug.LogError($"[MapManager] Map prefab 없음 stageId={stageId}");
            return;
        }

        LoadMapCommon(prefab);

        _currentStageId = stageId;
        _currentDungeonId = -1;

        Debug.Log($"[MapManager] Stage 맵 로드 : {stageId}");
    }

    public void LoadDungeonMap(int dungeonId)
    {
        if (_currentDungeonId == dungeonId)
            return;

        if (!_dungeonMapTable.TryGetValue(dungeonId, out GameObject prefab))
        {
            Debug.LogError($"[MapManager] Dungeon Map prefab 없음 dungeonId={dungeonId}");
            return;
        }

        LoadMapCommon(prefab);

        _currentDungeonId = dungeonId;
        _currentStageId = -1;

        Debug.Log($"[MapManager] Dungeon 맵 로드 : {dungeonId}");
    }

    /// <summary>
    /// Stage / Dungeon 공통 로딩 처리
    /// </summary>
    /// <param name="prefab"></param>
    private void LoadMapCommon(GameObject prefab)
    {
        ClearCurrentMap();

        _currentMapInstance = Instantiate(prefab, _mapRoot);

        SpawnAreaProvider spawnArea =
            _currentMapInstance.GetComponentInChildren<SpawnAreaProvider>();

        if (spawnArea != null && _spawnManager != null)
        {
            _spawnManager.SetSpawnAreaProvider(spawnArea);
        }
        else
        {
            Debug.LogWarning("[MapManager] SpawnAreaProvider를 찾지 못함");
        }

        //런타임 BuildNavMesh
        //빌드 환경에서 Read/Write Enabled 문제를 꼭 처리해야 함
        NavMeshSurface surface =
            _currentMapInstance.GetComponentInChildren<NavMeshSurface>();

        if (surface != null)
        {
            surface.BuildNavMesh();
        }
        else
        {
            Debug.LogWarning("[MapManager] NavMeshSurface를 찾지 못함");
        }
    }

    /// <summary>
    /// 현재 맵 제거
    /// </summary>
    private void ClearCurrentMap()
    {
        if (_currentMapInstance != null)
        {
            //SpawnAreaProvider 참조 제거
            if (_spawnManager != null)
                _spawnManager.SetSpawnAreaProvider(null);

            Destroy(_currentMapInstance);
            _currentMapInstance = null;
        }
    }
}