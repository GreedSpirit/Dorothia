using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Transform _mapRoot;

    [Header("Stage Map Table")]
    [SerializeField] private List<StageMapEntry> _stageMaps;

    // StageId -> MapPrefab 조회용
    private readonly Dictionary<int, GameObject> _mapTable = new();

    private GameObject _currentMapInstance;
    private int _currentStageId;

    private MonsterSpawnManager _spawnManager;

    private void Awake()
    {
        //MapRoot가 없으면 자동으로 자기 자신 사용
        if (_mapRoot == null)
        {
            Debug.LogWarning("[MapManager] MapRoot가 설정되지 않아 MapManager Transform을 사용");
            _mapRoot = transform;
        }

        BuildMapTable();

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

    /// <summary>
    /// Inspector에 등록된 StageMapEntry를 Dictionary로 변환
    /// </summary>
    private void BuildMapTable()
    {
        _mapTable.Clear();

        foreach (var entry in _stageMaps)
        {
            if (entry == null || entry.MapPrefab == null)
                continue;

            if (!_mapTable.ContainsKey(entry.StageId))
                _mapTable.Add(entry.StageId, entry.MapPrefab);
            else
                Debug.LogWarning($"[MapManager] 중복 StageId 발견 : {entry.StageId}");
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

        if (!_mapTable.TryGetValue(stageId, out GameObject prefab))
        {
            Debug.LogError($"[MapManager] Map prefab 없음 stageId={stageId}");
            return;
        }

        //기존 맵 제거
        ClearCurrentMap();

        //맵 생성
        _currentMapInstance = Instantiate(prefab, _mapRoot);

        //SpawnAreaProvider 자동 연결
        SpawnAreaProvider spawnArea =
            _currentMapInstance.GetComponentInChildren<SpawnAreaProvider>();

        if (spawnArea != null && _spawnManager != null)
        {
            _spawnManager.SetSpawnAreaProvider(spawnArea);
        }
        else
        {
            Debug.LogWarning("[MapManager] SpawnAreaProvider를 찾지 못했습니다.");
        }

        //NaveMesh 생성 시 자동 Bake
        NavMeshSurface surface = 
            _currentMapInstance.GetComponentInChildren<NavMeshSurface>();

        if (surface != null)
        {
            surface.BuildNavMesh();
        }

        _currentStageId = stageId;

        Debug.Log($"[MapManager] 맵 로드 : {stageId}");
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