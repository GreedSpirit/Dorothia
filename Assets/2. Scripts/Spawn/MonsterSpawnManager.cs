using UnityEngine;
using UnityEngine.Pool;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 몬스터의 생성과 회수만 담당
/// 스테이지 제어는 추후에 Stagecontroller로
/// 크게 Start/Stop 및 ForceClear를 호출하는 구조
/// </summary>
public class MonsterSpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private int _maxMonsterCount = 120; // 최대 몬스터 수
    [SerializeField] private float _spawnInterval = 3f; // 스폰 주기
    [SerializeField] private Vector2Int _spawnGroupSize = new Vector2Int(6, 7);

    [Header("References")]
    [SerializeField] private Monster _monsterPrefab;
    [SerializeField] private SpawnAreaProvider _spawnAreaProvider;

    private ObjectPool<Monster> _monsterPool;
    private readonly List<Monster> _activeMonsters = new();
    private int _currentMonsterCount;
    private bool _isSpawning;

    private void Awake()
    {
        _monsterPool = new ObjectPool<Monster>(
            CreateMonster,
            OnGetMonster,
            OnReleaseMonster,
            OnDestroyMonster,

            collectionCheck: false,
            defaultCapacity: _maxMonsterCount,
            maxSize: _maxMonsterCount
            );
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
    private void TrySpawnGroup()
    {
        if (_currentMonsterCount >= _maxMonsterCount)
            return;

        int spawnCount = Random.Range(_spawnGroupSize.x, _spawnGroupSize.y + 1);

        //그룹 중심점 1회만 계산
        if (!_spawnAreaProvider.TryGetSpawnPosition(out Vector3 centerPos))
            return;

        float maxRadius = 2.8f;         // 군집 반경
        float minDistance = 1.2f;       // 몬스터 간 최소 간격
        int maxAttemptsPerMonster = 10; // 뿌릴 몬스터 위치 찾기 재시도 횟수(겹침방지)

        float mapHalfSize = _spawnAreaProvider.MapHalfSize;

        List<Vector3> usedPositions = new();

        for (int i = 0; i < spawnCount; i++)
        {
            if (_currentMonsterCount >= _maxMonsterCount)
                break;

            bool found = false;
            Vector3 candidate = Vector3.zero;

            for (int attempt = 0; attempt < maxAttemptsPerMonster; attempt++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * maxRadius;
                candidate = centerPos + new Vector3(randomCircle.x, 0f, randomCircle.y);

                //맵 경계 Clamp
                candidate.x = Mathf.Clamp(candidate.x, -mapHalfSize, mapHalfSize);
                candidate.z = Mathf.Clamp(candidate.z, -mapHalfSize, mapHalfSize);

                bool overlaps = false;
                foreach (var pos in usedPositions)
                {
                    if (Vector3.Distance(candidate, pos) < minDistance)
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                continue;

            usedPositions.Add(candidate);

            Monster monster = _monsterPool.Get();
            monster.Initialize(this);
            monster.transform.position = candidate;

            _activeMonsters.Add(monster);
            _currentMonsterCount++;
        }
    }
    #endregion

    #region 오브젝트풀 콜백
    private Monster CreateMonster()
    {
        Monster monster = Instantiate(_monsterPrefab);
        monster.gameObject.SetActive(false);
        return monster;
    }

    private void OnGetMonster(Monster monster)
    {
        monster.gameObject.SetActive(true);
    }

    private void OnReleaseMonster(Monster monster)
    {
        monster.gameObject.SetActive(false);
        _activeMonsters.Remove(monster);
        _currentMonsterCount--;
    }

    private void OnDestroyMonster(Monster monster)
    {
        Destroy(monster.gameObject);
    }
    #endregion

    /// <summary>
    /// 몬스터 사망 시 풀로 반환
    /// </summary>
    /// <param name="monster"></param>
    public void ReleaseMonster(Monster monster)
    {
        _monsterPool.Release(monster);
    }

    /// <summary>
    /// 추후 보스 등장 등으로
    /// 모든 몬스터들 제거
    /// </summary>
    public void ForceClearAll()
    {
        for (int i = _activeMonsters.Count - 1; i >= 0; i--)
        {
            _monsterPool.Release(_activeMonsters[i]);
        }

        _activeMonsters.Clear();
        _currentMonsterCount = 0;
    }
}
