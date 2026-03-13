using UnityEngine;

/// <summary>
/// 일반 웨이브 룰
/// 현재 웨이브 전멸 -> 다음 웨이브
/// 마지막 웨이브 전멸 -> 클리어
/// </summary>
public class StandardWaveDungeonRule : DungeonRuleBase
{
    private readonly bool _useOrderedSpawn;

    public StandardWaveDungeonRule(bool useOrderedSpawn = true)
    {
        _useOrderedSpawn = useOrderedSpawn;
    }

    public override void OnCombatStarted()
    {
        Manager.SetCurrentWave(1);
        Manager.NotifyWaveChanged();

        if (_useOrderedSpawn)
            SpawnWaveOrdered(1);
        else
            SpawnWaveRandom(1);
    }

    public override void OnMonsterKilled(int monsterId)
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
}
