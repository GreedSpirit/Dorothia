public interface IMonster
{
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
