using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// FSM 기반 상태 관리 (Spawn / Chase / Attack / Daed)
/// NavMesh 기반
/// 오브젝트풀 반환 처리
/// 스폰/회수는 MonsterSpawnManager에서
/// 공격 로직은 MonsterAttackBase에서
/// 
/// - Adaptive Slot: 슬롯 수/링/반경을 상황에 맞춰 동적으로 확보(슬롯 "리소스"를 런타임에 재배치)
/// - Slot Refresh: 목표가 움직이거나 혼잡/정체 시 슬롯 재배치/재할당(몬스터가 자연스럽게 재포지셔닝)
/// - Angle Bias: "플레이어 진행방향/몬스터 접근방향" 기반으로 앞쪽/뒤쪽 가중치를 줘서 더 자연스러운 둘러싸기
///
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CapsuleCollider))]
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

    [Header("Combat Slot (Melee)")]
    [SerializeField] private float _slotRefreshInterval = 0.45f;    // 재할당 최소 간격
    [SerializeField] private float _slotRepathThreshold = 0.25f;    // 슬롯이 움직였을 때 destination 갱신 임계값
    [SerializeField] private float _stuckSpeedThreshold = 0.15f;    // 정체 판단 속도
    [SerializeField] private float _stuckTimeToRefresh = 0.55f; // 정체 지속 시간

    private float _nextSlotRefreshTime;
    private float _stuckTimer;

    private void Reset()
    {
        _hitCollider = GetComponent<CapsuleCollider>();

        int layer = LayerMask.NameToLayer("Monster");

        if (layer == -1)
        {
            Debug.LogWarning("레이어 'Monster' 없음");
            return;
        }

        SetLayerRecursively(gameObject, layer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        if (_hitCollider == null)
            _hitCollider = GetComponent<CapsuleCollider>();

        int layer = LayerMask.NameToLayer("Monster");
        if (layer != -1)
            gameObject.layer = layer;

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

        _nextSlotRefreshTime = 0f; // 슬롯 관련 런타임 변수 초기화
        _stuckTimer = 0f;

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
        if (_slotSystem == null && _target is MonoBehaviour mono)
        {
            _slotSystem = mono.GetComponent<PlayerCombatSlots>();
        }

        Vector3 destination;

        if (_stats.Archetype == Monster_Kind.Melee)
        {
            //Melee는 "Adaptive Slot + Refresh + Angle Bias" 사용
            // - 슬롯이 없으면 즉시 요청
            // - 일정 주기/정체 시 재요청 (Refresh)
            if (_slotSystem != null)
            {
                if (_mySlot == null)
                {
                    _mySlot = _slotSystem.AcquireOrRefreshSlot(this, forceRefresh: false);
                }
                else
                {
                    bool stuck = UpdateStuckAndCheck();

                    if (stuck)
                    {
                        _mySlot = _slotSystem.AcquireOrRefreshSlot(this, forceRefresh: true);
                        _stuckTimer = 0f;
                    }
                }
            }

            if (_mySlot != null)
                destination = _mySlot.position;
            else
                destination = _target.Transform.position; // 슬롯 없다면 플레이어로 fallback
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

        //슬롯 목표는 "움직일 수 있음"
        // - 슬롯이 리프레시되면 destination이 조금씩 변함
        // - hasPath + destination delta 체크로 갱신
        if (!_agent.hasPath || (_agent.destination - destination).sqrMagnitude > 
            _slotRepathThreshold * _slotRepathThreshold)
        {
            _agent.SetDestination(destination);
        }

        RotateToTarget(_target.Transform.position); // 항상 플레이어 방향 보게

        //공격 가능 여부 판단
        //근접 몬스터는 슬롯 도착 여부로 공격 판단
        if (_stats.Archetype == Monster_Kind.Melee && _mySlot != null)
        {
            float slotDistance =
                DistanceXZ(transform.position, _mySlot.position);

            if (slotDistance < 0.8f) // 슬롯 거의 도착
            {
                ChangeState(MonsterState.Attack);
                return;
            }
        }
        else
        {
            //원거리 몬스터는 기존 거리 판단 유지
            float attackDistance =
                DistanceXZ(transform.position, _target.Transform.position);

            if (attackDistance <= _stats.AttackRange)
            {
                _agent.ResetPath();
                ChangeState(MonsterState.Attack);
                return;
            }
        }
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

    //정체 판단: 속도가 너무 낮은 상태가 일정 시간 지속되면 "슬롯 Refresh" 트리거
    private bool UpdateStuckAndCheck()
    {
        //NavMeshAgent.velocity는 회전 업데이트를 꺼도 이동 속도는 잡힘
        float speed = _agent.velocity.magnitude;

        if (speed < _stuckSpeedThreshold)
            _stuckTimer += Time.deltaTime;
        else
            _stuckTimer = 0f;

        return _stuckTimer >= _stuckTimeToRefresh;
    }

    public void ForceDespawn()
    {
        if (_slotSystem != null && _mySlot != null)
        {
            _slotSystem.ReleaseSlot(this);
            _mySlot = null;
        }

        CancelInvoke();
        _owner.ReleaseMonster(this, _poolKeyPrefab);
    }
}
