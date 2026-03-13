using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 던전 룰 공통 베이스
/// </summary>
public abstract class DungeonRuleBase : IDungeonRule
{
    protected DungeonManager Manager;

    public virtual void Initialize(DungeonManager manager)
    {
        Manager = manager;
    }

    public virtual void OnPrepareStarted()
    {
    }

    public abstract void OnCombatStarted();

    public abstract void OnMonsterKilled(int monsterId);

    /// <summary>
    /// 공통 웨이브 처리
    /// 현재 웨이브 몬스터 전멸 시 다음 웨이브 또는 클리어
    /// </summary>
    protected void HandleStandardWaveProgress()
    {
        if (Manager.AliveMonsterCount > 0)
            return;

        if (Manager.CurrentWave < Manager.MaxWave)
        {
            Manager.AdvanceToNextWave();
        }
        else
        {
            Manager.RequestClear();
        }
    }

    /// <summary>
    /// 웨이브 엔트리를 고정 스폰 포인트 순서대로 스폰
    /// </summary>
    protected void SpawnWaveOrdered(int wave)
    {
        List<DungeonWaveSpawnEntry> entries = Manager.GetWaveEntries(wave);

        int pointOrder = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = 0; j < entries[i].spawnNum; j++)
            {
                if (Manager.TryGetDungeonOrderedSpawnPoint(pointOrder, out Vector3 pos))
                {
                    if (Manager.SpawnDungeonMonster(entries[i].monsterId, pos))
                        pointOrder++;
                }
                else
                {
                    //ordered point가 부족하면 random point fallback
                    if (Manager.TryGetDungeonRandomSpawnPoint(out Vector3 fallbackPos))
                    {
                        if (Manager.SpawnDungeonMonster(entries[i].monsterId, fallbackPos))
                            pointOrder++;
                    }
                    else
                    {
                        Debug.LogWarning($"[DungeonRule] Ordered/Random 스폰 포인트 없음 wave={wave}");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 웨이브 엔트리를 랜덤 스폰 포인트 기반으로 스폰
    /// </summary>
    protected void SpawnWaveRandom(int wave)
    {
        List<DungeonWaveSpawnEntry> entries = Manager.GetWaveEntries(wave);

        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = 0; j < entries[i].spawnNum; j++)
            {
                if (Manager.TryGetDungeonRandomSpawnPoint(out Vector3 pos))
                {
                    Manager.SpawnDungeonMonster(entries[i].monsterId, pos);
                }
                else if (Manager.TryGetDungeonOrderedSpawnPoint(j, out Vector3 fallbackPos))
                {
                    Manager.SpawnDungeonMonster(entries[i].monsterId, fallbackPos);
                }
                else
                {
                    Debug.LogWarning($"[DungeonRule] Random/Ordered 스폰 포인트 없음 wave={wave}");
                }
            }
        }
    }

    /// <summary>
    /// 특수 포인트 한 곳에 웨이브 몬스터 전부 스폰
    /// </summary>
    protected void SpawnWaveAtSpecialPoint(int wave, DungeonSpecialPointType pointType)
    {
        List<DungeonWaveSpawnEntry> entries = Manager.GetWaveEntries(wave);

        if (!Manager.TryGetDungeonSpecialPoint(pointType, out Vector3 pos))
        {
            Debug.LogWarning($"[DungeonRule] 특수 포인트 없음 {pointType}");
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            for (int j = 0; j < entries[i].spawnNum; j++)
            {
                Manager.SpawnDungeonMonster(entries[i].monsterId, pos);
            }
        }
    }
}