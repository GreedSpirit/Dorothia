using UnityEngine;

public interface IMonsterStats
{
    string MonsterID { get; }
    Monster_Type Rank { get; }
    Monster_Kind Archetype { get; }

    int MaxHp { get; }
    float MoveSpeed { get; }
    float RotateSpeed { get; }

    int Damage { get; }

    float AttackRange { get; }
    float AttackCooldown { get; }
    float PreferredRange { get; }

    float AgentRadius { get; }
    int AvoidancePriorityMin { get; }
    int AvoidancePriorityMax { get; }

    GameObject ProjectilePrefab { get; }
    float ProjectileSpeed { get; }
    float ProjectileLifeTime { get; }
}
