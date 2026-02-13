using UnityEngine;

public class MonsterStatsFromSO : IMonsterStats
{
    private readonly MonsterData _data;

    public MonsterStatsFromSO(MonsterData data)
    {
        _data = data;
    }

    public string MonsterID => _data.monsterID;
    public MonsterRank Rank => _data.rank;
    public MonsterArchetype Archetype => _data.archetype;

    public int MaxHp => _data.maxHp;
    public float MoveSpeed => _data.moveSpeed;
    public float RotateSpeed => _data.rotateSpeed;

    public int Damage => _data.damage;

    public float AttackRange => _data.attackRange;  
    public float AttackCooldown => _data.attackCooldown;
    public float PreferredRange => _data.preferredRange;

    public float AgentRadius => _data.agentRadius;
    public int AvoidancePriorityMin => _data.avoidancePriorityMin;
    public int AvoidancePriorityMax => _data.avoidancePriorityMax;

    public GameObject ProjectilePrefab => _data.projectilePrefab;
    public float ProjectileSpeed => _data.projectileSpeed;
    public float ProjectileLifeTime => _data.projectileLifeTime;

}
