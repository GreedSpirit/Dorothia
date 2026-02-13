using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 최근 N개의 TTK를 평균내서
/// 현재 처치 속도를 추정
/// </summary>
public class SpawnMetricsCollector
{
    private readonly Queue<float> _recentTTK = new Queue<float>();      // 최근 N개의 TTK 기록
    private readonly int _windowSize;                                   // 몇개를 평균 낼지 크기

    private float _sumTTK;                                              // 평균 계산을 위한 누적값

    public SpawnMetricsCollector(int windowSize = 20)
    {
        _windowSize = Mathf.Max(5, windowSize);                         // 최소 5개 이상 유지
    }

    public void RecordTTK(float ttk)
    {
        ttk = Mathf.Max(0.01f, ttk);

        _recentTTK.Enqueue(ttk);
        _sumTTK += ttk;

        //오래된거는 제거
        while (_recentTTK.Count > _windowSize)
        {
            _sumTTK -= _recentTTK.Dequeue();
        }
    }

    public SpawnMetrics GetMetrics()
    {
        //아직 데이터가 없으면 플레이어가 약하다고 가정
        float average = (_recentTTK.Count == 0) ? 999f : (_sumTTK / _recentTTK.Count);

        //초당 처치량
        float killPerSecond = 1f / Mathf.Max(0.01f, average);

        return new SpawnMetrics
        {
            AverageTTK = average,
            KillPerSecond = killPerSecond
        };
    }
}
