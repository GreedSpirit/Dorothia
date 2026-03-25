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
    [SerializeField] private AnimationClip[] _attackClips; // 공격 애니클립

    [Header("Death Animation")]
    [SerializeField] private AnimationClip[] _deathClips;   // 죽는 애니클립

    [Header("Static Monster")]
    [SerializeField] private GameObject _aliveModel;
    [SerializeField] private GameObject _brokenModel;

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

    private float _attackEnterRange;                // 진입
    private float _attackExitRange;                 // 이탈
    private float _nextRepathTime;
    private bool _isInSlotArea;

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
        ProjectileDatabase projectileDb,
        PlayerCombatSlots slotsystem)
    {
        _owner = owner;
        _target = target;
        _poolKeyPrefab = poolKeyPrefab;
        _projectileDatabase = projectileDb;
        _slotSystem = slotsystem;

        _mySlot = null;
        _isInSlotArea = false;

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
        
        _stats = new MonsterStatsFromCSV(monsterData, valueData, _projectileDatabase, poolKeyPrefab);

        _spawnTime = Time.time; // 생존시간 측정 시작
        _hp = _stats.MaxHp; // 체력 초기화

        _isAttacking = false; // 공격
        _attackTimer = 0f;

        if (_agent != null && !_agent.enabled)
            _agent.enabled = true;

        ApplyNavMeshSettings();

        //히스테리시스
        _attackEnterRange = _stats.AttackRange * 0.9f;
        _attackExitRange = _stats.AttackRange * 1.3f;

        if (_attack != null) // 공격타입 바인딩
            _attack.Bind(this);

        if (_hitCollider != null)
            _hitCollider.enabled = true;

        _nextSlotRefreshTime = 0f; // 슬롯 관련 런타임 변수 초기화
        _stuckTimer = 0f;

        CancelInvoke();
        ChangeState(MonsterState.Spawn); // 상태 시작

        if (_stats.Archetype == Monster_Kind.Passive)
        {
            SetupStaticMonster(); // 특이 몬스터
        }
    }

    /// <summary>
    /// 특이 몬스터 예외처리
    /// </summary>
    private void SetupStaticMonster()
    {
        if (_agent != null)
        {
            _agent.speed = 0f;
            _agent.isStopped = true;
            _agent.updateRotation = false;
            _agent.updatePosition = false;

            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        if (_attack != null)
            _attack.enabled = false;

        if (_animator != null)
            _animator.enabled = false;

        if (_aliveModel != null)
            _aliveModel.SetActive(true);

        if (_brokenModel != null)
            _brokenModel.SetActive(false);
    }

    private void ApplyNavMeshSettings()
    {
        _agent.speed = _stats.MoveSpeed;
        _agent.radius = _stats.AgentRadius;

        _agent.stoppingDistance = _stats.AttackRange * 0.8f;

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
        if (_stats.Archetype == Monster_Kind.Passive) // 특이 몬스터
        {
            if (_hp <= 0 && _currentState != MonsterState.Dead)
                ChangeState(MonsterState.Dead);

            return;
        }

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

            case MonsterState.Idle:
                UpdateIdle();
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
                _animator.SetBool("IsMoving", true);
                break;

            case MonsterState.Idle:
                _agent.isStopped = true;
                _agent.ResetPath();
                _animator.SetBool("IsMoving", false);
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

    private void UpdateChase()
    {
        if (_stats.Archetype == Monster_Kind.Ranged)
        {
            HandleRangedChase(); // 원거리
            return;
        }

        HandleMeleeChase(); // 근접
    }

    private void UpdateIdle()
    {
        if (_slotSystem == null || _mySlot == null)
        {
            ChangeState(MonsterState.Chase);
            return;
        }

        _agent.isStopped = true;

        RotateToTarget(_target.Transform.position);

        float dist = DistanceXZ(transform.position, _target.Transform.position);

        //공격 가능하면 즉시 전환
        if (dist <= _attackEnterRange)
        {
            ChangeState(MonsterState.Attack);
            return;
        }

        //일정 시간마다 승격 시도
        if (Time.time >= _nextRepathTime)
        {
            Transform better = _slotSystem.AcquireOrRefreshSlot(this, false);

            if (better != null)
                _mySlot = better;

            _nextRepathTime = Time.time + 0.5f;
        }
    }

    /// <summary>
    /// 공격 상태
    /// 사거리 안이면 공격 지속
    /// 사거리 밖이면 추격으로
    /// </summary>
    private void UpdateAttack()
    {
        RotateToTarget(_target.Transform.position);

        float distance = DistanceXZ(transform.position, _target.Transform.position);

        if (distance > _stats.AttackRange)
        {
            _agent.isStopped = false;
            ChangeState(MonsterState.Chase);
            return;
        }

        // 공격 루프
        if (!_isAttacking)
        {
            StartAttack();
            return;
        }

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _currentAttackDuration)
        {
            _isAttacking = false;
            _animator.speed = 1f;
        }
    }

    /// <summary>
    /// 근접 몬스터 Chase
    /// </summary>
    private void HandleMeleeChase()
    {
        //보스는 슬롯 제외
        if (IsBoss)
        {
            float distance = DistanceXZ(transform.position, _target.Transform.position);

            if (distance <= _stats.AttackRange)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
                ChangeState(MonsterState.Attack);
                return;
            }

            _agent.isStopped = false;
            _agent.SetDestination(_target.Transform.position);
            return;
        }

        //플레이어가 공격 사거리 안이면 슬롯 무시하고 즉시 공격
        //float playerDistance = DistanceXZ(transform.position, _target.Transform.position);
        //if (playerDistance <= _stats.AttackRange * 1.1f) // 여유 조금 추가
        //{
        //    _agent.isStopped = true;
        //    _agent.ResetPath();
        //    ChangeState(MonsterState.Attack);
        //    return;
        //}

        if (_slotSystem == null)
            return;

        //슬롯 유효성 체크
        if (_mySlot != null && !_slotSystem.IsValidSlot(_mySlot))
        {
            _mySlot = null;
        }

        //슬롯 영역 진입 체크
        bool inside = _slotSystem.IsInsideSlotArea(transform.position);

        //영역 밖 -> 그냥 추적
        if (!inside)
        {
            _isInSlotArea = false;

            _agent.isStopped = false;
            _agent.SetDestination(_target.Transform.position);
            return;
        }

        //최초 진입 시
        if (!_isInSlotArea)
        {
            _isInSlotArea = true;

            //가장 가까운 슬롯 확보
            _mySlot = _slotSystem.AcquireNearestSlot(this);
        }

        //슬롯 없으면 fallback
        if (_mySlot == null)
        {
            _mySlot = _slotSystem.AcquireNearestSlot(this);

            if (_mySlot == null)
            {
                _agent.SetDestination(_target.Transform.position);
                return;
            }
        }

        if (Time.time >= _nextSlotRefreshTime)
        {
            Transform better = _slotSystem.AcquireOrRefreshSlot(this, true);

            if (better != null && better != _mySlot)
            {
                _mySlot = better;
            }

            _nextSlotRefreshTime = Time.time + _slotRefreshInterval;
        }

        float distToSlot = DistanceXZ(transform.position, _mySlot.position);
        bool reached = distToSlot <= _agent.stoppingDistance + 0.1f;

        _slotSystem.NotifyArrived(this, reached);

        //슬롯 이동
        if (Vector3.Distance(_agent.destination, _mySlot.position) > _slotRepathThreshold)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_mySlot.position);
        }

        bool isInner = _slotSystem.IsInnerRing(_mySlot);

        //외부 링 -> 계속 이동
        if (!isInner)
        {
            return;
        }

        //내부 링 도착
        if (reached)
        {
            float distToPlayer = DistanceXZ(transform.position, _target.Transform.position);

            //공격 가능
            if (distToPlayer <= _attackEnterRange)
            {
                ChangeState(MonsterState.Attack);
            }
            else
            {
                _agent.isStopped = false;
                //자리 없으면 Idle
                //ChangeState(MonsterState.Idle);
            }
        }
    }

    /// <summary>
    /// 원거리 몬스터 Chase
    /// </summary>
    private void HandleRangedChase()
    {
        float distance = DistanceXZ(transform.position, _target.Transform.position);

        //사거리 밖이면 이동
        if (distance > _stats.AttackRange)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_target.Transform.position);
        }
        else
        {
            //사거리 안이면 바로 공격
            _agent.ResetPath();
            ChangeState(MonsterState.Attack);
        }

        RotateToTarget(_target.Transform.position);
    }

    private void EnterDead()
    {
        if (_stats.Archetype == Monster_Kind.Passive) // 특이 몬스터
        {
            if (_aliveModel != null)
                _aliveModel.SetActive(false);

            if (_brokenModel != null)
                _brokenModel.SetActive(true);

            if (_hitCollider != null)
                _hitCollider.enabled = false;

            float brokenTime = Mathf.Max(0.01f, Time.time - _spawnTime);
            OnMonsterKilledLifeTime?.Invoke(brokenTime);
            OnMonsterKilled?.Invoke(_monsterId, IsBoss);

            Invoke(nameof(ForceDespawn), 1f);
            return;
        }

        _agent.isStopped = true;
        _agent.ResetPath();

        int deathIndex = 0;

        //죽는 애니 랜덤
        if (_deathClips != null && _deathClips.Length > 0)
            deathIndex = Random.Range(0, _deathClips.Length);

        _animator.SetInteger("DeathIndex", deathIndex);

        _animator.speed = 1f;

        _animator.SetTrigger("Dead");

        if (_slotSystem != null && _mySlot != null)
        {
            _slotSystem.ReleaseSlot(this); // 슬롯 반환
            _mySlot = null;
        }

        if (_hitCollider != null)
            _hitCollider.enabled = false;

        if (_stats.Rank == Monster_Type.Normal && _owner != null) // 일반 몬스터만 드랍
        {
            _owner.SpawnOverdriveOrb(transform.position);
        }

        //동적 스폰용 생존시간 이벤트 발행
        float lifeTime = Mathf.Max(0.01f, Time.time - _spawnTime);
        OnMonsterKilledLifeTime?.Invoke(lifeTime);

        //스테이지 매니저가 킬카운트/보스판정 받게
        OnMonsterKilled?.Invoke(_monsterId, IsBoss);
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
        if (_attackClips == null || _attackClips.Length == 0)
            return;

        if (_isAttacking)
            return;

        _isAttacking = true;

        int attackIndex = Random.Range(0, _attackClips.Length);
        AnimationClip selectedClip = _attackClips[attackIndex];

        //Animator 전달
        _animator.SetInteger("AttackIndex", attackIndex);

        float baseLength = selectedClip.length; // 애니 길이 기준
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

            //todo : 테스트 코드
        DamageTextManager.Instance.ShowDamage(amount, transform.position, Random.value < 0.5 ? false : true);
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
