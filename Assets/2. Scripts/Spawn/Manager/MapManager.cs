using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    private bool _eventsRegistered;

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

    public DungeonSpawnAreaProvider CurrentDungeonSpawnProvider { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //MapRoot가 없으면 자동으로 자기 자신 사용
        if (_mapRoot == null)
        {
            Debug.LogWarning("[MapManager] MapRoot가 설정되지 않아 MapManager Transform을 사용");
            _mapRoot = transform;
        }

        BuildStageMapTable();
        BuildDungeonMapTable();

        _spawnManager = MonsterSpawnManager.Instance;

        if (_spawnManager == null)
            Debug.LogError("[MapManager] MonsterSpawnManager 없음.");
    }

    private void OnEnable()
    {
        if (_eventsRegistered)
            return;

        StageManager.OnStageIdChanged -= HandleStageChanged;
        StageManager.OnStageIdChanged += HandleStageChanged;

        _eventsRegistered = true;
    }

    private void OnDisable()
    {
        if (!_eventsRegistered)
            return;

        StageManager.OnStageIdChanged -= HandleStageChanged;

        _eventsRegistered = false;
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
        if (_currentStageId == stageId && _currentMapInstance != null)
            return;

        var entry = _stageMaps.FirstOrDefault(x => x.StageId == stageId);

        if (entry == null || entry.MapPrefab == null)
        {
            Debug.LogError($"[MapManager] Map prefab 없음 stageId={stageId}");
            return;
        }

        LoadMapCommon(entry.MapPrefab, isDungeon: false);

        _currentStageId = stageId;
        _currentDungeonId = -1;

        if (entry.BGM != null) // BGM 적용
        {
            SoundManager.Instance.PlayBGM(entry.BGM);
        }

        //Debug.Log($"[MapManager] Stage 맵 로드 : {stageId}");
    }

    public void LoadDungeonMap(int dungeonId)
    {
        if (_currentDungeonId == dungeonId && _currentMapInstance != null)
            return;

        var entry = _dungeonMaps.FirstOrDefault(x => x.DungeonId == dungeonId);

        if (entry == null || entry.MapPrefab == null)
        {
            Debug.LogError($"[MapManager] Dungeon Map prefab 없음 dungeonId={dungeonId}");
            return;
        }

        LoadMapCommon(entry.MapPrefab, isDungeon: true);

        _currentDungeonId = dungeonId;
        _currentStageId = -1;

        // ★ 여기 추가
        if (entry.BGM != null)
        {
            SoundManager.Instance.PlayBGM(entry.BGM);
        }

        Debug.Log($"[MapManager] Dungeon 맵 로드 : {dungeonId}");
    }

    /// <summary>
    /// Stage / Dungeon 공통 로딩 처리
    /// 던전맵이면 DungeonSpawnAreaProvider도 캐싱
    /// </summary>
    /// <param name="prefab"></param>
    private void LoadMapCommon(GameObject prefab, bool isDungeon)
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

        //던전 전용 provider 찾기
        if (isDungeon)
        {
            CurrentDungeonSpawnProvider =
                _currentMapInstance.GetComponentInChildren<DungeonSpawnAreaProvider>();

            if (CurrentDungeonSpawnProvider == null)
            {
                Debug.LogWarning("[MapManager] DungeonSpawnAreaProvider를 찾지 못함");
            }
        }
        else
        {
            CurrentDungeonSpawnProvider = null;
        }

        //런타임 BuildNavMesh
        //빌드 환경에서 Read/Write Enabled 문제를 꼭 처리해야 함
        //NavMeshSurface surface =
        //    _currentMapInstance.GetComponentInChildren<NavMeshSurface>();

        //if (surface != null)
        //{
        //    surface.BuildNavMesh();
        //}
        //else
        //{
        //    Debug.LogWarning("[MapManager] NavMeshSurface를 찾지 못함");
        //}
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

            CurrentDungeonSpawnProvider = null; // 현재 던전 Provider 해제

            NavMesh.RemoveAllNavMeshData();

            Destroy(_currentMapInstance);
            _currentMapInstance = null;
        }
    }
}