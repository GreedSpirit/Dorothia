using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 몬스터의 생성과 오브젝트 풀 관리
/// 군집 형태 스폰 위치 계산
/// AI 로직은 MonsterController에서
/// 위치 계산은 SpawnAreaProvider에서
/// </summary>
public class MonsterSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int _maxMonsterCount = 120; // 최대 몬스터 수
    [SerializeField] private float _spawnInterval = 3f; // 스폰 주기
    [SerializeField] private Vector2Int _spawnGroupSize = new Vector2Int(6, 7); // 군집 내 개체수

    [Header("Spawn Table")]
    [SerializeField] private List<MonsterSpawnEntry> _spawnTable = new();

    [Header("References")]
    [SerializeField] private SpawnAreaProvider _spawnAreaProvider;
    [SerializeField] private MonoBehaviour _targetProvider;

    private IMonsterTarget _target;

    private readonly Dictionary<MonsterController, ObjectPool<MonsterController>> _pools = new(); // 몬스터풀
    private readonly List<MonsterController> _activeMonsters = new();

    private int _currentMonsterCount;
    private bool _isSpawning;

    private readonly Dictionary<GameObject, ObjectPool<SimpleProjectile>> _projectilePools = new(); // 투사체풀

    private void Awake()
    {
        _target = _targetProvider as IMonsterTarget;
        if (_target == null)
            Debug.LogError("[MonsterSpawnManager] TargetProvider 설정");
    }

    private void Start()
    {
        StartNormalSpawn();
    }

    #region 스폰 컨트롤
    public void StartNormalSpawn()
    {
        if (_isSpawning)
            return;

        _isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    public void StopNormalSpawn()
    {
        _isSpawning = false;
    }

    private IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            yield return new WaitForSeconds(_spawnInterval);
            TrySpawnGroup();
        }
    }
    #endregion

    #region 스폰 로직
    /// <summary>
    /// 군집 단위 스폰
    /// </summary>
    private void TrySpawnGroup()
    {
        if (_currentMonsterCount >= _maxMonsterCount)
            return;

        if (_spawnTable == null || _spawnTable.Count == 0)
            return;

        int spawnCount = Random.Range(_spawnGroupSize.x, _spawnGroupSize.y + 1);

        //그룹 중심점은 1회만 계산
        if (!_spawnAreaProvider.TryGetSpawnPosition(out Vector3 centerPos))
            return;

        float maxRadius = 2.8f;
        float minDistance = 1.2f;
        int maxAttemptsPerMonster = 12;

        float mapHalfSize = _spawnAreaProvider.MapHalfSize;

        List<Vector3> usedPositions = new();

        for (int i = 0; i < spawnCount; i++)
        {
            if (_currentMonsterCount >= _maxMonsterCount)
                break;

            MonsterSpawnEntry entry = PickEntryByWeight();
            if (entry == null || entry.prefab == null)
                continue;

            if (!TryGetClusterPosition(centerPos, mapHalfSize, maxRadius, minDistance, 
                maxAttemptsPerMonster, usedPositions, out Vector3 spawnPos))
                continue;

            //프리팹 키 기반 풀에서 Get
            ObjectPool<MonsterController> pool = GetOrCreatePool(entry.prefab);
            MonsterController monster = pool.Get();

            monster.transform.position = spawnPos;

            //Initialize에 poolKeyPrefab을 넘겨서 어느 풀로 돌아갈지
            monster.Initialize(this, _target, entry.prefab);

            _activeMonsters.Add(monster);
            _currentMonsterCount++;
        }
    }

    private bool TryGetClusterPosition(
        Vector3 centerPos,
        float mapHalfSize,
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
            candidate.x = Mathf.Clamp(candidate.x, -mapHalfSize, mapHalfSize);
            candidate.z = Mathf.Clamp(candidate.z, -mapHalfSize, mapHalfSize);

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
    
    /// <summary>
    /// 가중치 기반 랜덤 선택
    /// 값이 높을수록 확률 증가
    /// </summary>
    /// <returns></returns>
    private MonsterSpawnEntry PickEntryByWeight()
    {
        float total = 0f;
        for (int i = 0; i < _spawnTable.Count; i++)
        {
            if (_spawnTable[i] == null)
                continue;

            total += Mathf.Max(0f, _spawnTable[i].weight);
        }

        if (total <= 0f)
            return _spawnTable[0];

        float roll = Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < _spawnTable.Count; i++)
        {
            MonsterSpawnEntry entry = _spawnTable[i];
            if (entry == null)
                continue;

            acc += Mathf.Max(0f, entry.weight);
            if (roll <= acc)
                return entry;
        }

        return _spawnTable[_spawnTable.Count - 1];
    }
    #endregion

    #region 오브젝트풀
    private ObjectPool<MonsterController> GetOrCreatePool(MonsterController prefabKey)
    {
        if (_pools.TryGetValue(prefabKey, out ObjectPool<MonsterController> pool))
            return pool;

        pool = new ObjectPool<MonsterController>(
            () => CreateMonster(prefabKey),
            OnGetMonster,
            OnReleaseMonster,
            OnDestroyMonster,
            collectionCheck: false,
            defaultCapacity: 16,
            maxSize: _maxMonsterCount
            );

        _pools.Add(prefabKey, pool);
        return pool;
    }

    private MonsterController CreateMonster(MonsterController prefabKey)
    {
        MonsterController monster = Instantiate(prefabKey);
        monster.gameObject.SetActive(false);
        return monster;
    }

    private void OnGetMonster(MonsterController monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(MonsterController monster)
    {
        monster.gameObject.SetActive(false);
    }

    private void OnDestroyMonster(MonsterController monster)
    {
        Destroy(monster.gameObject);
    }

    public void ReleaseMonster(MonsterController monster, MonsterController poolKeyPrefab)
    {
        if (monster == null || poolKeyPrefab == null)
            return;

        //active 목록/카운트 정리
        _activeMonsters.Remove(monster);
        _currentMonsterCount = Mathf.Max(0, _currentMonsterCount - 1);

        //정확한 풀로 반환
        if (_pools.TryGetValue(poolKeyPrefab, out ObjectPool<MonsterController> pool))
        {
            pool.Release(monster);
        }
        else
        {
            //키가 없으면 안전하게 비활성화
            monster.gameObject.SetActive(false);
        }
    }
    #endregion

    #region 투사체
    public SimpleProjectile GetProjectile(GameObject prefab)
    {
        if (!_projectilePools.TryGetValue(prefab, out var pool))
        {
            pool = new ObjectPool<SimpleProjectile>(
                () => CreateProjectile(prefab),
                OnGetProjectile,
                OnReleaseProjectile,
                OnDestroyProjectile,
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
        GameObject gameObject = Instantiate(prefab);
        gameObject.SetActive(false);
        return gameObject.GetComponent<SimpleProjectile>();
    }
    private void OnGetProjectile(SimpleProjectile proj)
    {
        proj.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(SimpleProjectile proj)
    {
        proj.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(SimpleProjectile proj)
    {
        Destroy(proj.gameObject);
    }

    public void ReleaseProjectile(SimpleProjectile proj, GameObject prefabKey)
    {
        if (_projectilePools.TryGetValue(prefabKey, out var pool))
        {
            pool.Release(proj);
        }
        else
        {
            Destroy(proj.gameObject);
        }
    }
    #endregion

    /// <summary>
    /// 추후 보스 등장 등으로
    /// 모든 몬스터들 제거
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
}
