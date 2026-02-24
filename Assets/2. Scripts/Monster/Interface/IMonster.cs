using UnityEngine;

public interface IMonster
{
    Transform Transform { get; }
    bool IsAlive { get; }

    void Initialize(
        MonsterSpawnManager owner,
        IMonsterTarget target, 
        MonsterController poolKeyPrefab,
        int monsterId,
        ProjectileDatabase projectileDb
        );
    void TakeDamage(int amount);
    void ForceDespawn();
}
