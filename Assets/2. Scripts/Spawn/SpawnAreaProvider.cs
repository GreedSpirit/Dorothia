using UnityEngine;

/// <summary>
/// 스폰 가능한 위치 계산만
/// SafeZone, 플레이어 시야 내 생성 금지
/// 맵 경계 내에서만 생성
/// </summary>
public class SpawnAreaProvider : MonoBehaviour
{
    [Header("Area Settings")]
    [SerializeField] private Vector2 _mapMin; // 최소 경계
    [SerializeField] private Vector2 _mapMax; // 최대 경계
    [SerializeField] private float _safeZoneRadius = 6f; // 플레이어 주위 보호 반경

    [Header("View Check")]
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Transform _player;

    private const int _maxTryCount = 20; // 무한루프 방지

    /// <summary>
    /// 스폰 가능한 위치 계산
    /// </summary>
    /// <param name="spawnPos"></param>
    /// <returns></returns>
    public bool TryGetSpawnPosition(out Vector3 spawnPos)
    {
        for (int i = 0; i < _maxTryCount; i++)
        {
            Vector3 candidate = GetRandomSpawnPosition();

            if (!IsOutsideSafeZone(candidate)) // 플레이어 보호 반경 내
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
    /// 경계 내에서 랜덤 위치
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(_mapMin.x, _mapMax.x);
        float z = Random.Range(_mapMin.y, _mapMax.y);
        return new Vector3 (x, 0f, z);
    }

    /// <summary>
    /// 플레이어 주변 세이프존 밖인지
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsOutsideSafeZone(Vector3 pos)
    {
        float distance = Vector3.Distance(
            new Vector3(_player.position.x, 0, _player.position.z),
            new Vector3(pos.x, 0, pos.z)
        );

        return distance >= _safeZoneRadius;
    }

    /// <summary>
    /// 현재 플레이어 카메라의 시야 내부인지
    /// </summary>
    /// <param name="worldPos"></param>
    /// <returns></returns>
    private bool IsInPlayerView(Vector3 worldPos)
    {
        Vector3 viewportPos = _playerCamera.WorldToViewportPoint(worldPos);

        return viewportPos.z > 0 &&
            viewportPos.x > 0 && viewportPos.x < 1 &&
            viewportPos.y > 0 && viewportPos.y < 1;
    }
}
