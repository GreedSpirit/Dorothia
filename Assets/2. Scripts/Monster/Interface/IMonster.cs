public interface IMonster
{
    void Initialize(
        MonsterSpawnManager owner,
        IMonsterTarget target, 
        MonsterController poolKeyPrefab
        );
    void TakeDamage(int amount);
    void ForceDespawn();
}
