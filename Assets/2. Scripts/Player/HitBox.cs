using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    private PlayerCtrl _player;
    private readonly HashSet<IMonster> _hitMonsters = new();

    private void Awake()
    {
        _player = GetComponentInParent<PlayerCtrl>();
    }

    private void OnEnable()
    {
        //공격 시작 시 중복 방지 리스트 초기화
        _hitMonsters.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_player == null)
            return;

        IMonster monster = other.GetComponentInParent<IMonster>();

        if (monster == null || !monster.IsAlive)
            return;

        //이미 맞은 몬스터면 무시
        //if (_hitMonsters.Contains(monster))
        //    return;

        _hitMonsters.Add(monster);

        int damage = Mathf.RoundToInt(_player.PlayerStats.Attack); // 플레이어스탯 공격력

        monster.TakeDamage(damage);
    }
}