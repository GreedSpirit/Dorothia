using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// FSM 기반 상태 관리 (Spawn / Chase / Attack / Daed)
/// NavMesh 기반
/// 오브젝트풀 반환 처리
/// 스폰/회수는 MonsterSpawnManager에서
/// 공격 로직은 MonsterAttackBase에서 
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterController : MonoBehaviour, IMonster
{
    public static event System.Action<float> OnMonsterKilledLifeTime;

    [Header("Editor Data")]
    [SerializeField] private MonsterData _data;     // 에디터 테스트용 데이터

    [Header("HitCollider")]
    [SerializeField] private Collider _hitCollider; // 피격 판정용 콜라이더

    private IMonsterStats _stats;                   // 인터페이스 기반 스탯

    private MonsterSpawnManager _owner;             // 스폰매니저
    private IMonsterTarget _target;                 // 추격대상(플레이어)

    private NavMeshAgent _agent;                    // 이동담당
    private MonsterAttackBase _attack;              // 공격타입(근접/원거리)

    private MonsterState _currentState;

    private int _hp;
    private float _lastAttackTime;

    private float _spawnTime;                       // 생존시간 계산

    //어떤 프리펩풀로 반환할지 식별용 키
    private MonsterController _poolKeyPrefab;

    public IMonsterStats Stats => _stats;
    public MonsterSpawnManager SpawnManager => _owner;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (_hitCollider == null)
            _hitCollider = GetComponent<Collider>();

        _attack = GetComponent<MonsterAttackBase>();
    }

    /// <summary>
    /// 풀에서 꺼낸 직후 초기화
    /// 스탯 적용, 타겟 연결, 상태 초기화까지
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="target"></param>
    /// <param name="poolKeyPrefab"></param>
    public void Initialize(MonsterSpawnManager owner, IMonsterTarget target, MonsterController poolKeyPrefab)
    {
        _owner = owner;
        _target = target;
        _poolKeyPrefab = poolKeyPrefab;

        if (_data == null)
        {
            Debug.LogError("[MonsterController] MonsterData 없음");
            return;
        }

        _stats = new MonsterStatsFromSO(_data); // SO -> 인터페이스 어댑터 생성

        _spawnTime = Time.time; // 생존시간 측정 시작

        _hp = _stats.MaxHp; // 체력 초기화
        _lastAttackTime = -999f; // 즉시 공격방지

        ApplyNavMeshSettings();

        if (_attack != null) // 공격타입 바인딩
            _attack.Bind(this);

        if (_hitCollider != null)
            _hitCollider.enabled = true;

        CancelInvoke();
        ChangeState(MonsterState.Spawn); // 상태 시작
    }

    private void ApplyNavMeshSettings()
    {
        _agent.speed = _stats.MoveSpeed;
        _agent.radius = _stats.AgentRadius;
        
        //RVO 충돌 우선순위 랜덤화 -> 군집 이동할때 자연스러움 증가
        _agent.avoidancePriority = Random.Range(_stats.AvoidancePriorityMin, _stats.AvoidancePriorityMax + 1);

        _agent.isStopped = false;
        _agent.ResetPath();
    }

    private void Update()
    {
        if (_currentState == MonsterState.Dead)
            return;

        //체력 0 이하 우선 처리
        if (_hp <= 0)
        {
            ChangeState(MonsterState.Dead);
            return;
        }

        //타겟이 없거나 사망시 이동 중지
        if (_target == null || !_target.IsAlive)
        {
            _agent.isStopped = true;
            return;
        }

        //FSM 상태 업데이트
        switch (_currentState)
        {
            case MonsterState.Spawn:
                UpdateSpawn();
                break;

            case MonsterState.Chase:
                UpdateChase();
                break;

            case MonsterState.Attack:
                UpdateAttack();
                break;
        }
    }

    /// <summary>
    /// 상태 전환 처리
    /// </summary>
    /// <param name="newState"></param>
    private void ChangeState(MonsterState newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case MonsterState.Spawn:
                _agent.isStopped = false;
                break;

            case MonsterState.Chase:
                _agent.isStopped = false;
                break;

            case MonsterState.Attack:
                _agent.isStopped = true;
                _agent.ResetPath();
                break;

            case MonsterState.Dead:
                EnterDead();
                break;
        }
    }

    #region 상태 업데이트
    private void UpdateSpawn()
    {
        ChangeState(MonsterState.Chase); // 스폰 직후 바로 추격으로
    }

    /// <summary>
    /// 추격 상태
    /// NavMeshAgent로 최단 경로로
    /// 원거리는 PreferredRange 유지
    /// </summary>
    private void UpdateChase()
    {
        float distance = DistanceXZ(transform.position, _target.Transform.position);

        //최단경로 및 원거리공격 유지
        if (_stats.Archetype == MonsterArchetype.Ranged)
        {
            //원거리: 일정 거리 이상일 때만 접근
            if (distance > _stats.PreferredRange)
                _agent.SetDestination(_target.Transform.position);
            else
                _agent.ResetPath();
        }
        else
        {
            //근접: 무조건 접근
            _agent.SetDestination(_target.Transform.position);
        }

        RotateToTarget(_target.Transform.position);

        if (distance <= _stats.AttackRange)
            ChangeState(MonsterState.Attack);
    }

    /// <summary>
    /// 공격 상태
    /// 사거리 안이면 공격 지속
    /// 사거리 밖이면 추격으로
    /// </summary>
    private void UpdateAttack()
    {
        float distance = DistanceXZ(transform.position, _target.Transform.position);

        RotateToTarget(_target.Transform.position);

        //사거리 밖이면 다시 추격
        if (distance > _stats.AttackRange)
        {
            ChangeState(MonsterState.Chase);
            return;
        }

        //사거리 안이면 계속 공격
        if (Time.time - _lastAttackTime >= _stats.AttackCooldown)
        {
            DoAttack();
            _lastAttackTime = Time.time;
        }
    }

    private void EnterDead()
    {
        _agent.isStopped = true;
        _agent.ResetPath();

        if (_hitCollider != null)
            _hitCollider.enabled = false;

        //동적 스폰용 생존시간 이벤트 발행
        float lifeTime = Mathf.Max(0.01f, Time.time - _spawnTime);
        OnMonsterKilledLifeTime?.Invoke(lifeTime);

        Invoke(nameof(ForceDespawn), 1f); // 1초후 풀 반환
    }
    #endregion

    #region 공격
    /// <summary>
    /// 실제 공격 실행
    /// 공격 방식은 MonsterAttackBase에서
    /// </summary>
    private void DoAttack()
    {
        if (_attack != null)
            _attack.PerformAttack(_target);
        else
            _target.ApplyDamage(_stats.Damage);
    }

    public void TakeDamage(int amount)
    {
        _hp -= amount;
    }
    #endregion

    private void RotateToTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        Quaternion look = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * _stats.RotateSpeed);
    }

    private float DistanceXZ(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public void ForceDespawn()
    {
        CancelInvoke();
        _owner.ReleaseMonster(this, _poolKeyPrefab);
    }
}
