using UnityEngine;

/// <summary>
/// 최소 30마리 유지
/// 30마리 이상부터 TTK 반영
/// 100마리 근접시 TTK 느리면 스폰도 느리게
/// </summary>
public class DynamicSpawnPolicy : ISpawnPolicy
{
    private readonly int _minCount;
    private readonly int _softMaxCount;

    public DynamicSpawnPolicy(int minCount = 30, int softMaxCount = 100)
    {
        _minCount = Mathf.Max(0, minCount);
        _softMaxCount = Mathf.Max(_minCount, softMaxCount);
    }

    public float GetSpawnInterval(SpawnMetrics metrics, int currentCount)
    {
        //최소유지: 빠른 보충
        if (currentCount < _minCount)
            return 0.4f;

        //100근접 + TTK 느림: 더 느리게
        if (currentCount >= _softMaxCount && metrics.AverageTTK > 3.0f)
            return 4.5f;

        //기본: TTK 기반
        if (metrics.AverageTTK <= 1.2f)     // 엄청 잘 잡음 -> 빠르게
            return 1.0f;
        if (metrics.AverageTTK <= 3.0f)     // 보통
            return 2.0f;

        return 3.5f;                        // 느림 -> 천천히
    }

    public float GetSpawnMultiplier(SpawnMetrics metrics, int currentCount)
    {
        // 최소 유지
        if (currentCount < _minCount)
            return 1.8f;

        // 너무 많고 느리면 감소
        if (currentCount >= _softMaxCount && metrics.AverageTTK > 3.0f)
            return 0.5f;

        // TTK 기반
        if (metrics.AverageTTK <= 1.2f)
            return 1.5f;

        if (metrics.AverageTTK <= 3.0f)
            return 1.0f;

        return 0.7f;
    }
}
