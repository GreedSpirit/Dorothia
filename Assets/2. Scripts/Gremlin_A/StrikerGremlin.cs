using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    범위형 = 1, 단일형 = 2
}

[Serializable]
public class StrikerGremlin : GremlinBehaviour
{
    [SerializeField] private float _attackRange = 10f;                  // 공격범위
    [SerializeField] private LayerMask _enemyLayer;                     // 공격할 적을 정의하기 위한 적 레이어

    [SerializeField] private PlayerCtrl _player;                  // 추적할 대상

    public float attackSpeed { get; private set; }                      // 테이블에서 받아올 공격속도
    public float attackDamage { get; private set; }                     // 테이블에서 받아올 공격력
    public float finalAttack { get; set; }                              // 별도의 계산을 거쳐 받아올 최종 피해량
    private Rarity _rarity;                                             // 등급
    private Transform _transform;                                       // 해당 그렘린의 위치

    private float _timer;                                               // 시간 체크용
                                       // 플레이어

    private AttackType _attackType;

    private void Awake()
    {
        //플레이어 찾기
        _player = FindAnyObjectByType<PlayerCtrl>();
    }
    private void Update()
    {
        Tick();
    }

    public void Init(List<Gremlin_StatusData> data, Transform transform, Rarity rarity)
    {
        foreach(var statusData in data)
        {

            attackSpeed = statusData.Gremlin_Dps;
            attackDamage = statusData.Gremlin_Atk;
            _rarity = rarity;

            _transform = transform;

            _attackType = (AttackType)statusData.Target_Type;
            Debug.Log(_attackType);
            _enemyLayer = LayerMask.GetMask("Monster");
        }
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;

        if (_timer >= 1f/attackSpeed)
        {
            Transform targetEnemy = FindTarget();
            Debug.Log(targetEnemy == null);

            if (targetEnemy != null)
            {
                _timer = 0f;
                OnTick?.Invoke();
                if(_attackType == AttackType.단일형)
                {
                    ExecuteAttack(targetEnemy);
                }
                else if(_attackType == AttackType.범위형)
                {
                    ExecuteAOEAttack(targetEnemy);
                }
            }
        }
    }

    private Transform FindTarget()
    {
        if (_player != null && _player.CurrentTarget != null)
        {
            if (Vector3.Distance(_transform.position, _player.CurrentTarget.Transform.position) <= _attackRange)
            {
                return _player.CurrentTarget.Transform;
            }
        }

        // 플레이어와 가장 가까운 적
        Collider[] hitColliders = Physics.OverlapSphere(_player.transform.position, _attackRange, _enemyLayer);
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(_player.transform.position, hitCollider.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = hitCollider.transform;
            }
        }

        return closestEnemy;
    }

    private void ExecuteAttack(Transform targetEnemy)
    {
        float finalDamage = finalAttack;

        var enemy = targetEnemy.GetComponent<IMonster>();
        if(enemy != null)
        {
            enemy.TakeDamage((int)finalDamage);
        }
        else
        {
            Debug.Log("오류! 대상을 찾을 수 없습니다.");
        }
            Debug.Log($"그렘린이 {targetEnemy}에게 {finalDamage}를 입힙니다.");
    }

    private void ExecuteAOEAttack(Transform targetEnemy)
    {
        float finalDamage = finalAttack;
        var hits = Physics.OverlapSphere(targetEnemy.position, 1f, _enemyLayer);

        foreach(var hit in hits)
        {
            var enemy = hit.GetComponent<IMonster>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)finalDamage);
                Debug.Log($"그렘린이 {targetEnemy}에게 {finalDamage}를 입힙니다.");
            }
            else
            {
                Debug.Log("오류! 대상을 찾을 수 없습니다.");
            }
        }
    }
}

