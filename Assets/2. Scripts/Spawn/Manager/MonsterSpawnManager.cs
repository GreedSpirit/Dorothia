using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

/// <summary>
/// 스폰 총괄 매니저
/// Monster_SpawnData 기반으로 "군집(클러스터)" 스폰
/// TTK(처치시간) 기반으로 스폰 간격/군집 크기 보정(DynamicSpawnPolicy)
/// 오브젝트풀로 몬스터/투사체 재사용
/// 보스전 진입 시
/// 1) 일반 스폰 루틴 정지
/// 2) ForceClearAll로 필드 몬스터 전부 회수
/// 3) 보스 1마리만 SpawnBoss로 소환
/// </summary>
public class MonsterSpawnManager : MonoBehaviour
{
    public static MonsterSpawnManager Instance { get; private set; }

    private bool _eventsRegistered;

    [Header("Spawn Settings")]
    [SerializeField] private int _maxMonsterCount = 120; // 물리적 제한 수치

    [Header("References")]
    [SerializeField] private SpawnAreaProvider _spawnAreaProvider;
    [SerializeField] private MonoBehaviour _targetProvider;
    [SerializeField] private PlayerCombatSlots _combatSlots;

    [Header("Projectile Database")]
    [SerializeField] private ProjectileDatabase _projectileDatabase;

    [Header("Overdrive Orb")]
    [SerializeField] private OverdriveOrb _overdriveOrbPrefab;

    private ObjectPool<OverdriveOrb> _orbPool;

    private IMonsterTarget _target;

    //몬스터풀, 프리팹 키단위로 풀을 분리해서 서론 다른 몬스터 프리팹을 섞어 써도 안전
    private readonly Dictionary<MonsterController, ObjectPool<MonsterController>> _pools = new();
    private readonly List<MonsterController> _activeMonsters = new();

    private int _currentMonsterCount;
    private bool _isSpawning;

    //투사체풀
    private readonly Dictionary<GameObject, ObjectPool<SimpleProjectile>> _projectilePools = new();
    
    //스폰 정보
    private Monster_SpawnData _currentSpawnData;    // CSV
    private int _stageSoftMaxCount;                 // 스테이지 동시 스폰 상한
    private int _currentBossMonsterId;              // 현재 스테이지 보스 ID
    private bool _isBossFight;                      // 보스전 플래그

    private readonly List<(int id, int count)> _spawnCandidates = new(8);
    private Coroutine _spawnRoutine;

    //동적 스폰
    private SpawnMetricsCollector _metrics;
    private DynamicSpawnPolicy _policy;

    public int CurrentMonsterCount => _currentMonsterCount;

    public void SetSpawnAreaProvider(SpawnAreaProvider provider)
    {
        _spawnAreaProvider = provider;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _target = _targetProvider as IMonsterTarget;

        if (_target == null)
            Debug.LogError("[MonsterSpawnManager] TargetProvider 설정");

        if (_projectileDatabase == null)
            Debug.LogError("[MonsterSpawnManager] ProjectileDatabase 설정 안됨");

        _orbPool = new ObjectPool<OverdriveOrb>(
            () => CreateOrb(),
            orb => orb.gameObject.SetActive(true),
            orb => orb.gameObject.SetActive(false),
            orb => Destroy(orb.gameObject),
            false,
            32,
            160);

        _metrics = new SpawnMetricsCollector(20);
        _policy = new DynamicSpawnPolicy(30);
    }

    /// <summary>
    /// StageManager가 스테이지 시작시
    /// SpawnData, 동시 스폰 상한, 보스ID 주입
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sameSpawnMax"></param>
    /// <param name="bossMonsterId"></param>
    public void InitializeStageSpawn(Monster_SpawnData data, int sameSpawnMax, int bossMonsterId)
    {
        _currentSpawnData = data;
        _stageSoftMaxCount = sameSpawnMax;
        _currentBossMonsterId = bossMonsterId;

        _isBossFight = false; // 스테이지 재진입/복귀 시 플래그 초기화
        _policy.SetSoftMax(_stageSoftMaxCount); // 스테이지 시작마다 보스전 플래그 초기화
    }

    private void OnEnable()
    {
        if (_eventsRegistered)
            return;

        MonsterController.OnMonsterKilledLifeTime -= HandleMonsterKilled;
        MonsterController.OnMonsterKilledLifeTime += HandleMonsterKilled;

        _eventsRegistered = true;
    }

    private void OnDisable()
    {
        if (!_eventsRegistered)
            return;

        MonsterController.OnMonsterKilledLifeTime -= HandleMonsterKilled;

        _eventsRegistered = false;
    }

    private void HandleMonsterKilled(float lifeTime)
    {
        _metrics.RecordTTK(lifeTime);
    }

    #region 스폰 컨트롤
    public void StartNormalSpawn()
    {
        if (_isSpawning)
            return;

        _isSpawning = true;
        _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopNormalSpawn()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }

        _isSpawning = false;
    }

    public void StopAllSpawnForDungeon()
    {
        StopNormalSpawn();
        _isBossFight = false;
    }

    /// <summary>
    /// 일반 스폰 루틴
    /// interval은 DynamicSpawnPolicy에서 metrics(TTK 등)와 현재 몬스터 수를 기반으로 계산
    /// 보스전 중에는 루틴이 스폰 X
    /// </summary>
    /// <returns></returns>
    private IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            //보스전이면 스폰 정지
            if (_isBossFight)
            {
                yield return null;
                continue;
            }

            SpawnMetrics metrics = _metrics.GetMetrics();
            float interval = _policy.GetSpawnInterval(metrics, _currentMonsterCount);

            yield return new WaitForSeconds(interval);

            if (_isBossFight)
                continue;

            if (_currentSpawnData == null)
                continue;

            if (_currentMonsterCount >= _stageSoftMaxCount)
                continue;

            if (_spawnAreaProvider == null)
                continue;

            if (_target == null || !_target.IsAlive)
                continue;

            if (!_spawnAreaProvider.TryGetSpawnPosition(out Vector3 centerPos))
                continue;

            SpawnClusterWave(metrics);
        }
    }
    #endregion

    #region 스폰 로직 (SpawnData 기반)
    /// <summary>
    /// multiplier기반으로 군집 개수 증가
    /// </summary>
    /// <param name="metrics"></param>
    private void SpawnClusterWave(SpawnMetrics metrics)
    {
        if (_currentSpawnData == null)
            return;

        List<(int id, int count)> candidates = BuildSpawnCandidates();

        if (candidates.Count == 0)
            return;

        //TTK 기반 배율
        float multiplier = _policy.GetSpawnMultiplier(metrics, _currentMonsterCount);

        //multiplier를 "군집 개수"로 변환
        int clusterCount = Mathf.FloorToInt(multiplier);

        if (Random.value < (multiplier - clusterCount))
            clusterCount++;

        clusterCount = Mathf.Max(1, clusterCount);

        for (int i = 0; i < clusterCount; i++)
        {
            if (_currentMonsterCount >= _stageSoftMaxCount)
                break;

            //군집마다 새 중심점 생성
            if (!_spawnAreaProvider.TryGetSpawnPosition(out Vector3 clusterCenter))
                continue;

            var selected = candidates[Random.Range(0, candidates.Count)];

            SpawnSingleCluster(
                selected.id,
                selected.count, // Monster_Number는 그대로 사용
                clusterCenter
            );
        }
    }

    /// <summary>
    /// 스폰후보 생성 분리
    /// </summary>
    /// <returns></returns>
    private List<(int id, int count)> BuildSpawnCandidates()
    {
        _spawnCandidates.Clear();

        void Add(int id, int count)
        {
            if (id <= 0 || count <= 0)
                return;

            //이번 스테이지 보스 ID는 무조건 제외
            if (id == _currentBossMonsterId)
                return;

            //Monster_Data 기반으로 Boss 타입이면 무조건 제외 (CSV가 어디에 넣든 안전)
            var md = DataManager.Instance.GetData<Monster_Data>(id);
            if (md != null && md.Monster_Type == Monster_Type.Boss)
                return;

            _spawnCandidates.Add((id, count));
        }

        //우선은 일반,앨리트 풀 1~7까지 사용
        Add(_currentSpawnData.Monster_Id_1, _currentSpawnData.Monster_Number_1);
        Add(_currentSpawnData.Monster_Id_2, _currentSpawnData.Monster_Number_2);
        Add(_currentSpawnData.Monster_Id_3, _currentSpawnData.Monster_Number_3);
        Add(_currentSpawnData.Monster_Id_4, _currentSpawnData.Monster_Number_4);
        Add(_currentSpawnData.Monster_Id_5, _currentSpawnData.Monster_Number_5);
        Add(_currentSpawnData.Monster_Id_6, _currentSpawnData.Monster_Number_6);
        Add(_currentSpawnData.Monster_Id_7, _currentSpawnData.Monster_Number_7);

        return _spawnCandidates;
    }

    /// <summary>
    /// 군집 1개 생성 (개체수 고정)
    /// </summary>
    /// <param name="monsterId"></param>
    /// <param name="clusterSize"></param>
    /// <param name="centerPos"></param>
    private void SpawnSingleCluster(int monsterId, int clusterSize, Vector3 centerPos)
    {
        float maxRadius = 2.8f;
        float minDistance = 1.2f;
        int maxAttemptsPerMonster = 12;

        Vector2 mapSize = _spawnAreaProvider.MapSize;
        Vector3 mapCenter = _spawnAreaProvider.MapCenter;

        float halfX = mapSize.x * 0.5f;
        float halfZ = mapSize.y * 0.5f;

        List<Vector3> usedPositions = new();

        for (int i = 0; i < clusterSize; i++)
        {
            if (_currentMonsterCount >= _stageSoftMaxCount)
                break;

            if (!TryGetClusterPosition(
                centerPos,
                mapSize,
                mapCenter,
                maxRadius,
                minDistance,
                maxAttemptsPerMonster,
                usedPositions,
                out Vector3 spawnPos))
            {
                continue;
            }

            SpawnSingle(monsterId, spawnPos);
        }
    }

    private void SpawnSingle(int monsterId, Vector3 pos)
    {
        if (monsterId <= 0)
        {
            Debug.LogError("[MonsterSpawnManager] SpawnSingle monsterId <= 0");
            return;
        }

        Monster_Data data = DataManager.Instance.GetData<Monster_Data>(monsterId);
        if (data == null)
        {
            Debug.LogError($"MonsterData 없음 {monsterId}");
            return;
        }

        MonsterController prefab = MonsterPrefabRegistry.Instance.GetPrefab(monsterId);
        if (prefab == null)
        {
            Debug.LogError($"Prefab 없음 {monsterId}");
            return;
        }

        ObjectPool<MonsterController> pool = GetOrCreatePool(prefab);

        MonsterController monster = pool.Get();
        monster.transform.position = pos;

        monster.Initialize(this, _target, prefab, monsterId, _projectileDatabase, _combatSlots);

        _activeMonsters.Add(monster);
        _currentMonsterCount++;
    }

    /// <summary>
    /// 군집 내 포지션 
    /// centerPos 주변 랜덤 샘플링 + NavMesh
    /// minDistance로 군집 내 겹침 방지
    /// </summary>
    /// <param name="centerPos"></param>
    /// <param name="mapHalfSize"></param>
    /// <param name="maxRadius"></param>
    /// <param name="minDistance"></param>
    /// <param name="maxAttempts"></param>
    /// <param name="used"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private bool TryGetClusterPosition(
        Vector3 centerPos,
        Vector2 mapSize,
        Vector3 mapCenter,
        float maxRadius,
        float minDistance,
        int maxAttempts,
        List<Vector3> used,
        out Vector3 result)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 r = Random.insideUnitCircle * maxRadius;
            Vector3 candidate = centerPos + new Vector3(r.x, 0f, r.y);

            //맵 경계 Clamp
            float halfX = mapSize.x * 0.5f;
            float halfZ = mapSize.y * 0.5f;

            candidate.x = Mathf.Clamp(candidate.x, mapCenter.x - halfX, mapCenter.x + halfX);
            candidate.z = Mathf.Clamp(candidate.z, mapCenter.z - halfZ, mapCenter.z + halfZ);

            //지형 높이 보정 (NavMesh 위로 스냅)
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                candidate = hit.position;
            }
            else
            {
                continue; // NavMesh가 아니면 다음 시도
            }

            //군집 내 최소 간격 유지
            bool overlap = false;
            for (int i = 0; i < used.Count; i++)
            {
                if (Vector3.Distance(candidate, used[i]) < minDistance)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
            {
                used.Add(candidate);
                result = candidate;
                return true;
            }
        }

        result = Vector3.zero;
        return false;
    }
    #endregion

    #region 보스
    /// <summary>
    /// 보스 스폰
    /// 보스전 플래그를 올려 일반 스폰을 막음
    /// </summary>
    /// <param name="bossMonsterId"></param>
    public void SpawnBoss(int bossMonsterId)
    {
        if (bossMonsterId <= 0)
        {
            Debug.LogError("[MonsterSpawnManager] SpawnBoss called with bossMonsterId <= 0");
            return;
        }

        _isBossFight = true;
        _isSpawning = false; // 보스전 동안 절대 스폰 루틴이 돌지 않게 강제

        if (_spawnAreaProvider == null)
        {
            Debug.LogError("SpawnAreaProvider 없음");
            return;
        }

        if (!_spawnAreaProvider.TryGetBossSpawnPosition(out Vector3 spawnPos))
            spawnPos = Vector3.zero;

        SpawnSingle(bossMonsterId, spawnPos);

        Debug.Log($"[SpawnManager] 보스 소환 : {bossMonsterId}");
    }

    public void EndBossFight()
    {
        _isBossFight = false;

        if (!_isSpawning)
        {
            _isSpawning = true;
            _spawnRoutine = StartCoroutine(SpawnRoutine());
        }
    }

    /// <summary>
    /// 현재 필드의 몬스터들 전부 풀로 반환
    /// 보스전 진입시 정리
    /// </summary>
    public void ForceClearAll()
    {
        for (int i = _activeMonsters.Count - 1; i >= 0; i--)
        {
            if (_activeMonsters[i] != null)
                _activeMonsters[i].ForceDespawn();
        }

        _activeMonsters.Clear();
        _currentMonsterCount = 0;
    }
    #endregion

    #region 오브젝트풀
    private ObjectPool<MonsterController> GetOrCreatePool(MonsterController prefabKey)
    {
        if (_pools.TryGetValue(prefabKey, out var pool))
            return pool;

        pool = new ObjectPool<MonsterController>(
            () => CreateMonster(prefabKey),
            m => m.gameObject.SetActive(true),
            m => m.gameObject.SetActive(false),
            m => Destroy(m.gameObject),
            false,
            16,
            _maxMonsterCount
        );

        _pools.Add(prefabKey, pool);
        return pool;
    }

    private MonsterController CreateMonster(MonsterController prefabKey)
    {
        MonsterController monster =
            Instantiate(prefabKey, RuntimeRootManager.Monsters);

        monster.gameObject.SetActive(false);
        return monster;
    }

    /// <summary>
    /// MonsterController가 사망, 강제 회수시
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="poolKeyPrefab"></param>
    public void ReleaseMonster(MonsterController monster, MonsterController poolKeyPrefab)
    {
        if (monster == null || poolKeyPrefab == null)
            return;

        _activeMonsters.Remove(monster);
        _currentMonsterCount =
            Mathf.Max(0, _currentMonsterCount - 1);

        if (_pools.TryGetValue(poolKeyPrefab, out var pool))
            pool.Release(monster);
    }
    #endregion

    #region 투사체풀
    public SimpleProjectile GetProjectile(GameObject prefab)
    {
        if (!_projectilePools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<SimpleProjectile>(
                () => CreateProjectile(prefab),
                p => p.gameObject.SetActive(true),
                p => p.gameObject.SetActive(false),
                p => Destroy(p.gameObject),
                false,
                16,
                256
            );

            _projectilePools.Add(prefab, pool);
        }

        return pool.Get();
    }

    private SimpleProjectile CreateProjectile(GameObject prefab)
    {
        GameObject go =
            Instantiate(prefab, RuntimeRootManager.Projectiles);

        go.SetActive(false);

        SimpleProjectile projectile =
            go.GetComponent<SimpleProjectile>();

        if (projectile == null)
            Debug.LogError("Projectile component missing");

        return projectile;
    }

    public void ReleaseProjectile(SimpleProjectile proj, GameObject prefabKey)
    {
        if (_projectilePools.TryGetValue(prefabKey, out var pool))
            pool.Release(proj);
        else
            Destroy(proj.gameObject);
    }
    #endregion

    #region 오버드라이브 오브
    private OverdriveOrb CreateOrb()
    {
        if (_overdriveOrbPrefab == null)
        {
            Debug.LogError("OverdriveOrbPrefab 없음");
            return null;
        }

        OverdriveOrb orb =
            Instantiate(_overdriveOrbPrefab, RuntimeRootManager.Orbs);

        orb.gameObject.SetActive(false);
        orb.SetOwner(this);

        return orb;
    }

    public void SpawnOverdriveOrb(Vector3 pos)
    {
        if (_orbPool == null)
            return;

        OverdriveOrb orb = _orbPool.Get();

        if (orb == null)
            return;

        orb.transform.position = pos;
        orb.Setup(_target);
    }

    public void ReleaseOrb(OverdriveOrb orb)
    {
        if (_orbPool != null)
            _orbPool.Release(orb);
    }
    #endregion

    //던전 전용 단일 스폰
    public bool SpawnSingleDungeon(int monsterId, Vector3 pos)
    {
        if (monsterId <= 0)
        {
            Debug.LogError("[MonsterSpawnManager] SpawnSingleDungeon monsterId <= 0");
            return false;
        }

        Monster_Data data = DataManager.Instance.GetData<Monster_Data>(monsterId);
        if (data == null)
        {
            Debug.LogError($"[DungeonSpawn] MonsterData 없음 {monsterId}");
            return false;
        }

        MonsterController prefab = MonsterPrefabRegistry.Instance.GetPrefab(monsterId);
        if (prefab == null)
        {
            Debug.LogError($"[DungeonSpawn] Prefab 없음 {monsterId}");
            return false;
        }

        ObjectPool<MonsterController> pool = GetOrCreatePool(prefab);

        MonsterController monster = pool.Get();

        if (monster == null)
        {
            Debug.LogError($"[DungeonSpawn] Pool Get 실패 {monsterId}");
            return false;
        }

        monster.transform.position = pos;

        monster.Initialize(this, _target, prefab, monsterId, _projectileDatabase, _combatSlots);

        _activeMonsters.Add(monster);
        _currentMonsterCount++;

        return true;
    }

    //DungeonManager에서 직접 위치를 얻을 수 있게
    public bool TryGetSpawnPosition(out Vector3 pos)
    {
        if (_spawnAreaProvider == null)
        {
            pos = Vector3.zero;
            return false;
        }

        return _spawnAreaProvider.TryGetSpawnPosition(out pos);
    }
}