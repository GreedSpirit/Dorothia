using UnityEngine;

public class MonsterStatsFromCSV : IMonsterStats
{
    private readonly Monster_Data _data;
    private readonly Monster_ValueData _value;
    private readonly ProjectileData _projectile;

    public MonsterStatsFromCSV(Monster_Data data, Monster_ValueData value, ProjectileDatabase projectileDb)
    {
        _data = data;
        _value = value;

        if (_data.Projectile_Id > 0 && projectileDb != null)
        {
            _projectile = projectileDb.Get(_data.Projectile_Id);
        }
    }

    public string MonsterID => _data.Monster_Name;

    public Monster_Type Rank => _data.Monster_Type;
    public Monster_Kind Archetype => _data.Monster_Kind;

    public int MaxHp => Mathf.RoundToInt(_data.Monster_Hp * _value.Monster_Hp_Value);
    public int Damage => Mathf.RoundToInt(_data.Monster_Atk * _value.Monster_Atk_Value);

    public float MoveSpeed => _data.Monster_Agi;
    public float RotateSpeed => 10f;

    public float AttackRange => _data.Monster_Atk_Range;
    public float AttackCooldown => 1f;
    public float PreferredRange => _data.Monster_Atk_Range - 0.2f;

    public float AgentRadius => 0.5f;
    public int AvoidancePriorityMin => 60;
    public int AvoidancePriorityMax => 60;

    //투사체는 SO로 관리
    public GameObject ProjectilePrefab =>
        _projectile != null ? _projectile.prefab : null;

    public float ProjectileSpeed =>
        _projectile != null ? _projectile.speed : 0f;

    public float ProjectileLifeTime =>
        _projectile != null ? _projectile.lifeTime : 0f;
}
