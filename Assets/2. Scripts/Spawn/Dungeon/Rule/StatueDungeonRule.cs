using UnityEngine;

/// <summary>
/// 150001 우상의 제단
/// 조각상 1마리만 스폰
/// 해당 조각상 처치 시 즉시 클리어
/// </summary>
public class StatueDungeonRule : DungeonRuleBase
{
    public override void OnCombatStarted()
    {
        Manager.SetCurrentWave(1);
        Manager.NotifyWaveChanged();

        //조각상은 보스/중앙 포인트 우선 사용
        SpawnWaveAtSpecialPoint(1, DungeonSpecialPointType.BossSpawn);

        //포인트가 없다면 center fallback
        if (Manager.AliveMonsterCount <= 0)
        {
            SpawnWaveAtSpecialPoint(1, DungeonSpecialPointType.Center);
        }
    }

    public override void OnMonsterKilled(int monsterId)
    {
        if (Manager.AliveMonsterCount <= 0)
        {
            Manager.RequestClear();
        }
    }
}
