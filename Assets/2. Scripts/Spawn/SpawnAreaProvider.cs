using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 스폰 가능한 위치 계산만
/// SafeZone, 플레이어 시야 내 생성 금지
/// 맵 경계 내에서만 생성
/// </summary>
public class SpawnAreaProvider : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private float _mapHalfSize = 30f; // 맵 절반 크기 (중심 0,0 기준)

    [Header("Player")]
    //[SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _player;

    [Header("Spawn Setting")]
    private float _playerSafeSpawnRadius = 2f;      // 플레이어 안전 거리
    private float _playerNearSpawnRadius = 8f;      // 플레이어 주변 스폰 반경
    private float _playerNearSpawnWeight = 0.7f;    // 플레이어 근처 스폰 확률

    private const int _maxTryCount = 20; // 무한루프 방지

    public float MapHalfSize => _mapHalfSize;

    private void Awake()
    {
        if (_player == null)
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        //if (_playerCamera == null)
        //    _playerCamera = Camera.main;
    }

    /// <summary>
    /// 스폰 가능한 위치 계산
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public bool TryGetSpawnPosition(out Vector3 spawnPos)
    {
        for (int i = 0; i < _maxTryCount; i++)
        {
            Vector3 candidate;

            //플레이어 근처 가중치 스폰
            if (Random.value < _playerNearSpawnWeight && _player != null)
            {
                candidate = GetPlayerNearPosition();
            }
            else
            {
                candidate = GetRandomInsideMap();
            }

            //NavMesh 위로 스냅
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
                return true;
            }
        }

        spawnPos = Vector3.zero;
        return false;
    }

    /// <summary>
    /// 사각형 맵 영역 내부 램덤 위치
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomInsideMap()
    {
        float x = Random.Range(-_mapHalfSize, _mapHalfSize);
        float z = Random.Range(-_mapHalfSize, _mapHalfSize);
        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// 플레이어 주변 랜덤위치
    /// </summary>
    /// <returns></returns>
    private Vector3 GetPlayerNearPosition()
    {
        //도넛 반경 랜덤
        float radius = Random.Range(_playerSafeSpawnRadius, _playerNearSpawnRadius);

        //방향 랜덤
        Vector2 dir = Random.insideUnitCircle.normalized;

        Vector3 pos = _player.position + new Vector3(dir.x, 0f, dir.y) * radius;

        //맵 경계 제한
        pos.x = Mathf.Clamp(pos.x, -_mapHalfSize, _mapHalfSize);
        pos.z = Mathf.Clamp(pos.z, -_mapHalfSize, _mapHalfSize);

        return pos;
    }

    public bool TryGetBossSpawnPosition(out Vector3 spawnPos)
    {
        if (_player == null)
        {
            spawnPos = Vector3.zero;
            return false;
        }

        //보스는 플레이어 Z축 전방 5f 위치 기준
        float forwardDistance = 5f;

        //좌우 랜덤 (너무 정중앙 생성 방지)
        float sideOffset = Random.Range(-1f, 1f);

        Vector3 candidate = new Vector3(
            _player.position.x + sideOffset, 0f, _player.position.z + forwardDistance);

        candidate.x = Mathf.Clamp(candidate.x, -_mapHalfSize, _mapHalfSize);
        candidate.z = Mathf.Clamp(candidate.z, -_mapHalfSize, _mapHalfSize);

        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
            return true;
        }

        spawnPos = Vector3.zero;
        return false;
    }

    //private bool IsInsideSafeZone(Vector3 pos)
    //{
    //    if (_player == null)
    //        return false;

    //    Vector3 p = _player.position;
    //    p.y = 0f;
    //    pos.y = 0f;

    //    return Vector3.Distance(p, pos) < _safeZoneRadius;
    //}

    ///// <summary>
    ///// 현재 플레이어 카메라의 시야 내부인지
    ///// </summary>
    ///// <param name="worldPos"></param>
    ///// <returns></returns>
    //private bool IsInPlayerView(Vector3 worldPos)
    //{
    //    if (_playerCamera == null)
    //        return false;

    //    Vector3 viewportPos = _playerCamera.WorldToViewportPoint(worldPos);

    //    return viewportPos.z > 0 &&
    //        viewportPos.x > 0 && viewportPos.x < 1 &&
    //        viewportPos.y > 0 && viewportPos.y < 1;
    //}

    #region 기즈모 영역
    private void OnDrawGizmos()
    {
        //맵 경계 영역: 녹색
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(_mapHalfSize * 2f, 0.1f, _mapHalfSize * 2f)
        );

        //플레이어 근처 영역: 노란색
        if (_player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.position, _playerNearSpawnRadius);
        }
    }
    #endregion
}
