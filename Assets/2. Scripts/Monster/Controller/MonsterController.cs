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
[RequireComponent(typeof(Animator))]
public class MonsterController : MonoBehaviour, IMonster
{
    public static event System.Action<float> OnMonsterKilledLifeTime;   // 동적 스폰 TTK 기록용
    public static event System.Action<int, bool> OnMonsterKilled;       // 스테이지 진행용
    
    [Header("Projectile")]
    [SerializeField] private ProjectileDatabase _projectileDatabase;

    [Header("HitCollider")]
    [SerializeField] private Collider _hitCollider; // 피격 판정용 콜라이더

    [Header("Attack Animation")]
    [SerializeField] private AnimationClip _attackClip; // 공격 애니클립

    private IMonsterStats _stats;                   // 인터페이스 기반 스탯

    private MonsterSpawnManager _owner;             // 스폰매니저
    private IMonsterTarget _target;                 // 추격대상(플레이어)

    private NavMeshAgent _agent;                    // 이동담당
    private MonsterAttackBase _attack;              // 공격타입(근접/원거리)

    private Animator _animator;

    private MonsterState _currentState;

    private PlayerCombatSlots _slotSystem;          // 슬롯시스템
    private Transform _mySlot;

    private int _hp;                                // 체력

    private bool _isAttacking;                      // 실제 공격 여부
    private float _attackTimer;                     // 공격 타이머
    private float _currentAttackDuration;           // 계산된 공격 지속시간

    private float _spawnTime;                       // 생존시간 계산

    //어떤 프리펩풀로 반환할지 식별용 키
    private MonsterController _poolKeyPrefab;

    //스테이지, 보스
    private int _monsterId;
    public int MonsterId => _monsterId;
    private bool IsBoss => _stats != null && _stats.Rank == Monster_Type.Boss;

    public IMonsterStats Stats => _stats;
    public MonsterSpawnManager SpawnManager => _owner;
    public Transform Transform => transform;
    public bool IsAlive => _currentState != MonsterState.Dead;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

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
    public void Initialize(
        MonsterSpawnManager owner, 
        IMonsterTarget target, 
        MonsterController poolKeyPrefab, 
        int monsterId, 
        ProjectileDatabase projectileDb)
    {
        _owner = owner;
        _target = target;
        _poolKeyPrefab = poolKeyPrefab;
        _projectileDatabase = projectileDb;

        _monsterId = monsterId;

        if (DataManager.Instance == null)
        {
            Debug.LogError("[MonsterController] 씬에 DataManager 없음");
            return;
        }

        //CSV 가져오기
        var monsterData = DataManager.Instance.GetData<Monster_Data>(monsterId);
        var valueData = DataManager.Instance.GetData<Monster_ValueData>(monsterId);        

        if (monsterData == null || valueData == null)
        {
            Debug.LogError($"[MonsterController] CSV 데이터 없음: {monsterId}");
            return;
        }

        if (_projectileDatabase == null)
        {
            Debug.LogError("ProjectileDatabase NULL");
        }
        
        _stats = new MonsterStatsFromCSV(monsterData, valueData, _projectileDatabase);

        _spawnTime = Time.time; // 생존시간 측정 시작
        _hp = _stats.MaxHp; // 체력 초기화

        _isAttacking = false; // 공격
        _attackTimer = 0f;

        if (_agent != null && !_agent.enabled)
            _agent.enabled = true;

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
        _agent.avoidancePriority = 
            Random.Range(_stats.AvoidancePriorityMin, _stats.AvoidancePriorityMax + 1);

        //빠른 근접 몬스터가 몰릴 때 플레이어 밀림/겹침을 줄이기 위한
        _agent.obstacleAvoidanceType = 
            ObstacleAvoidanceType.HighQualityObstacleAvoidance;

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
                _animator.SetBool("IsMoving", true);    // 애니
                break;

            case MonsterState.Attack:
                _agent.isStopped = true;
                _agent.ResetPath();
                _animator.SetBool("IsMoving", false);
                StartAttack(); // 공격시작
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
    /// 근접 몬스터
    /// 플레이어 중심이 아닌 '전투 슬롯 위치'를 목표로 이동
    /// 슬롯 시스템을 통해 겹침 및 밀림 현상 방지
    /// 
    /// 원거리 몬스터
    /// 플레이어와의 거리(PreferredRange)를 유지
    /// 사거리보다 멀면 접근, 가까우면 정지 후 공격
    /// 
    /// 공통
    /// 항상 플레이어 방향으로 회전
    /// 실제 공격 사거리(AttackRange)에 진입하면 Attack 상태로 전환
    /// </summary>
    private void UpdateChase()
    {
        //슬롯 시스템 가져오기
        if (_slotSystem == null)
        {
            _slotSystem =
                (_target as MonoBehaviour)
                ?.GetComponent<PlayerCombatSlots>();
        }

        //근접 몬스터일 경우 슬롯 요청
        if (_mySlot == null && _slotSystem != null)
        {
            _mySlot = _slotSystem.RequestSlof(this);
        }

        Vector3 destination;

        if (_stats.Archetype == Monster_Kind.Melee && _mySlot != null)
        {
            //근접 몬스터 -> 슬롯 위치 이동
            destination = _mySlot.position;
        }
        else
        {
            //원거리 몬스터 -> 플레이어 거리 유지
            float distance =
                DistanceXZ(transform.position, _target.Transform.position);

            if (distance > _stats.PreferredRange)
            {
                //사거리 보다 멀면 접근
                destination = _target.Transform.position;
            }
            else
            {
                //적정 거리 유지 -> 이동 정지 후 공격
                _agent.ResetPath();
                RotateToTarget(_target.Transform.position);
                return;
            }
        }

        _agent.SetDestination(destination); // NavMesh 설정

        RotateToTarget(_target.Transform.position); // 항상 플레이어 방향 보게

        //공격 가능 여부 판단
        float attackDistance =
            DistanceXZ(transform.position, _target.Transform.position);

        if (attackDistance <= _stats.AttackRange) // 공격
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
        if (!_isAttacking)
            return;

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _currentAttackDuration)
        {
            _isAttacking = false;
            _animator.speed = 1f; // 속도 복구
            ChangeState(MonsterState.Chase);
        }
    }

    private void EnterDead()
    {
        _agent.isStopped = true;
        _agent.ResetPath();

        _animator.SetTrigger("Dead");

        if (_slotSystem != null && _mySlot != null)
        {
            _slotSystem.ReleaseSlot(this); // 슬롯 반환
            _mySlot = null;
        }

        if (_hitCollider != null)
            _hitCollider.enabled = false;

        //동적 스폰용 생존시간 이벤트 발행
        float lifeTime = Mathf.Max(0.01f, Time.time - _spawnTime);
        OnMonsterKilledLifeTime?.Invoke(lifeTime);

        //스테이지 매니저가 킬카운트/보스판정 받게
        OnMonsterKilled?.Invoke(_monsterId, IsBoss);

        //Invoke(nameof(ForceDespawn), 1f); // 1초후 풀 반환
    }

    //애니메이션 이벤트용
    public void OnDeathAnimationEnd()
    {
        ForceDespawn();
    }
    #endregion

    #region 공격
    /// <summary>
    /// 애니 기반 공격
    /// </summary>
    private void StartAttack()
    {
        if (_isAttacking)
            return;

        _isAttacking = true;

        float baseLength = _attackClip.length; // 애니 길이 기준
        _currentAttackDuration = baseLength * _stats.AttackCooldown; // 속도 배율

        _animator.speed = 1f / _stats.AttackCooldown; // 애니 속도 조절
        _animator.SetTrigger("Attack");
        _attackTimer = 0f;
    }

    //애니메이션 이벤트용
    public void OnAttackHit()
    {
        if (!_isAttacking)
            return;

        DoAttack();
    }

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
        if (_currentState == MonsterState.Dead)
            return;

        _hp -= amount;

        if (_hp <= 0)
        {
            _hp = 0;
            ChangeState(MonsterState.Dead);
        }
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
