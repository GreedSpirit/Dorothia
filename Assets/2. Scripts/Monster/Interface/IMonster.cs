using UnityEngine;

public interface IMonster
{
    Transform Transform { get; }
    bool IsAlive { get; }
    IMonsterStats Stats { get; }

    void Initialize(
        MonsterSpawnManager owner,
        IMonsterTarget target, 
        MonsterController poolKeyPrefab,
        int monsterId,
        ProjectileDatabase projectileDb,
        PlayerCombatSlots slotSystem
        );
    void TakeDamage(int amount, bool isCritical = false);
    void ForceDespawn();
}
