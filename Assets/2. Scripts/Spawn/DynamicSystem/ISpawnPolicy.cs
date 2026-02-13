public interface ISpawnPolicy
{
    float GetSpawnInterval(SpawnMetrics metrics, int currentCount);

    /// <summary>
    /// 군집 기본 크기에 곱할 배율 반환
    /// 1.0 = 기본 유지
    /// >1.0 = 더 많이
    /// <1.0 = 줄이기
    /// </summary>
    float GetSpawnMultiplier(SpawnMetrics metrics, int currentCount);
}
