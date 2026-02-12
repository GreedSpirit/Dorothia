using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "Scriptable Objects/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Identity")]
    public string monsterID = "Monster_Normal_Melee";
    public MonsterRank rank = MonsterRank.Normal;
    public MonsterArchetype archetype = MonsterArchetype.Melee;

    [Header("Stats")]
    public int maxHp = 100;
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 10f;

    [Header("Damage")]
    public int damage = 10;

    [Header("Chase / Attack")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1.0f;

    public float preferredRange = 6.0f;

    [Header("NavMesh / Avoidance")]
    public float agentRadius = 0.35f;
    public int avoidancePriorityMin = 30;
    public int avoidancePriorityMax = 70;

    [Header("Range Only")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 12f;
    public float projectileLifeTime = 3f;
}
