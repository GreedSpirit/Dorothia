using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StrikerGremlin : GremlinBehaviour
{
    [SerializeField] private float _attackRange = 10f;                  // 공격범위
    [SerializeField] private LayerMask _enemyLayer;                     // 공격할 적을 정의하기 위한 적 레이어

    [SerializeField] protected Transform followTarget;                  // 추적할 대상

    public float attackSpeed { get; private set; }                      // 테이블에서 받아올 공격속도
    public float attackDamage { get; private set; }                     // 테이블에서 받아올 공격력
    public float finalAttack { get; set; }                              // 별도의 계산을 거쳐 받아올 최종 피해량
    private Rarity _rarity;                                             // 등급
    private Transform _transform;                                       // 해당 그렘린의 위치

    private float _timer;                                               // 시간 체크용
    private PlayerCtrl _player;                                         // 플레이어

    private void Awake()
    {
        //플레이어 찾기
        _player = FindAnyObjectByType<PlayerCtrl>();
    }

    public void Init(Gremlin_StatusData data, Transform transform, Rarity rarity)
    {
        attackSpeed = data.Gremlin_Dps;
        attackDamage = data.Gremlin_Atk;
        _rarity = rarity;

        _transform = transform;
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;

        if (_timer >= 1f/attackSpeed)
        {
            Transform targetEnemy = FindTarget();

            if (targetEnemy != null)
            {
                _timer = 0f;
                ExecuteAttack(targetEnemy);
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
        Collider[] hitColliders = Physics.OverlapSphere(followTarget.position, _attackRange, _enemyLayer);
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(followTarget.position, hitCollider.transform.position);
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

        //TODO 투사체 발사, 즉발 데미지 처리, 이펙트 혹은 사운드 같은 기능 구현 여기다가!
        Debug.Log($"그렘린이 {targetEnemy}에게 {finalDamage}를 입힙니다.");
    }
}

