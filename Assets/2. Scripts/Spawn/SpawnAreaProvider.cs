using UnityEngine;

/// <summary>
/// 스폰 가능한 위치 계산만
/// SafeZone, 플레이어 시야 내 생성 금지
/// 맵 경계 내에서만 생성
/// </summary>
public class SpawnAreaProvider : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private float _mapHalfSize = 30f; // 맵 절반 크기 (중심 0,0 기준)
    [SerializeField] private float _safeZoneRadius = 6f; // 플레이어 주위 보호 반경

    [Header("View Check")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _player;

    private const int _maxTryCount = 20; // 무한루프 방지

    public float MapHalfSize => _mapHalfSize;

    /// <summary>
    /// 스폰 가능한 위치 계산
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public bool TryGetSpawnPosition(out Vector3 spawnPos)
    {
        for (int i = 0; i < _maxTryCount; i++)
        {
            Vector3 candidate = GetRandomInsideMap();

            if (IsInsideSafeZone(candidate)) // 세이프존 내
                continue;

            if (IsInPlayerView(candidate)) // 카메라 시야 내
                continue;

            spawnPos = candidate;
            return true;
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

    private bool IsInsideSafeZone(Vector3 pos)
    {
        if (_player == null)
            return false;

        Vector3 p = _player.position;
        p.y = 0f;
        pos.y = 0f;

        return Vector3.Distance(p, pos) < _safeZoneRadius;
    }

    /// <summary>
    /// 현재 플레이어 카메라의 시야 내부인지
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    private bool IsInPlayerView(Vector3 worldPos)
    {
        if (_playerCamera == null)
            return false;

        Vector3 viewportPos = _playerCamera.WorldToViewportPoint(worldPos);

        return viewportPos.z > 0 &&
            viewportPos.x > 0 && viewportPos.x < 1 &&
            viewportPos.y > 0 && viewportPos.y < 1;
    }

    #region 기즈모 영역
    private void OnDrawGizmos()
    {
        //맵 경계 영역: 녹색
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(_mapHalfSize * 2f, 0.1f, _mapHalfSize * 2f)
        );

        //플레이어 SafeZone: 노란색
        if (_player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_player.position, _safeZoneRadius);
        }
    }
    #endregion
}
