using UnityEngine;

/// <summary>
/// 레이드 던전 룰
/// 보스 1마리만 스폰
/// 보스 처치 시 즉시 클리어
/// </summary>
public class RaidDungeonRule : DungeonRuleBase
{
    private bool _bossSpawned;

    public override void OnCombatStarted()
    {
        Manager.SetCurrentWave(1);
        Manager.NotifyWaveChanged();

        SpawnBoss();
    }

    private void SpawnBoss()
    {
        if (_bossSpawned)
            return;

        _bossSpawned = true;

        //BossSpawnPoint만 사용
        if (!Manager.TryGetDungeonSpecialPoint(
            DungeonSpecialPointType.BossSpawn,
            out Vector3 pos))
        {
            Debug.LogError("[RaidDungeonRule] BossSpawnPoint 없음");
            Manager.RequestFail();
            return;
        }

        //몬스터 가져오기
        var entries = Manager.GetWaveEntries(1);

        if (entries == null || entries.Count == 0)
        {
            Debug.LogError("[RaidDungeonRule] 스폰 데이터 없음");
            Manager.RequestFail();
            return;
        }

        //1마리만 스폰
        int bossId = entries[0].monsterId;

        if (!Manager.SpawnDungeonMonster(bossId, pos))
        {
            Debug.LogError("[RaidDungeonRule] 보스 스폰 실패");
            Manager.RequestFail();
        }
    }

    public override void OnMonsterKilled(int monsterId)
    {
        //보스 1마리니까 죽으면 바로 클리어
        if (Manager.AliveMonsterCount <= 0)
        {
            Manager.RequestClear();
        }
    }
}