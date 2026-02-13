using UnityEngine;

public class MonsterRangedAttack : MonsterAttackBase
{
    public override void PerformAttack(IMonsterTarget target)
    {
        if (target == null || !target.IsAlive)
            return;

        //MonsterData data = _owner.Data;
        var stats = _owner.Stats;

        if (stats.ProjectilePrefab == null)
        {
            Debug.LogWarning("[MonsterRangedAttack] 투사체 프리팹 없음");
            return;
        }

        //발사 위치
        Vector3 spawnPos =
            _owner.transform.position +
            _owner.transform.forward * 0.8f +
            Vector3.up * 0.5f;

        //투사체풀
        SimpleProjectile projectile =
            _owner.SpawnManager.GetProjectile(stats.ProjectilePrefab);

        projectile.transform.position = spawnPos;

        Vector3 direction =
            (target.Transform.position - spawnPos).normalized;

        projectile.Initialize(
            direction,
            stats.ProjectileSpeed,
            stats.Damage,
            stats.ProjectileLifeTime,
            _owner.SpawnManager,
            stats.ProjectilePrefab
        );
    }
}
