using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour, IMonsterTarget, IResettable
{
    //프로퍼티
    public Vector2 MoveInput => _moveInput;
    public PlayerStats PlayerStats => _playerStats;
    public Animator Anima => _anima;
    public NavMeshAgent NavMesh => _navMesh;
    public IMonster CurrentTarget => _currentTarget;
    public bool IsAutoMode => _isAutoMode;
    public bool IsAttack => _isAttack;
    public float EnemyFindRange => _enemyFindRange;
    public float AttackRange => _attackRange;

    //콤보 체크용변수
    public int ComboIndex { get; set; } = 0;

    public Transform Transform => transform;

    public bool IsAlive => !_isDead;

    [SerializeField] LayerMask _enemyLayer;

    //드래그 사거리
    [SerializeField] float _dragDistance = 100f;

    //적 탐지범위
    [SerializeField] float _enemyFindRange = 20f;

    //공격실행범위
    [SerializeField] float _attackRange = 1f;

    //히트박스
    [SerializeField] BoxCollider _hitBox;
    [SerializeField] BoxCollider _hitBox3;

    //이펙트
    [SerializeField] ParticleSystem _attackEffect1;
    [SerializeField] ParticleSystem _attackEffect2;
    [SerializeField] ParticleSystem _attackEffect3;
    [SerializeField] ParticleSystem _attackHitEffect;

    PlayerStats _playerStats;
    Animator _anima;
    NavMeshAgent _navMesh;

    //외부에서쓸 타겟변수
    IMonster _currentTarget;

    //오토모드 체크용변수
    bool _isAutoMode = false;

    //공격상태 체크용변수
    bool _isAttack = false;

    int _maxComboIndex = 3;

    bool _isDead = false;



    //입력값 저장변수
    Vector2 _moveInput;
    Vector2 _currentInput;
    Vector2 _touchStart;

    //상태
    IPlayerState<PlayerCtrl> _currentState;
    PlayerMoveState _moveState;
    PlayerIdleState _idleState;
    PlayerAutoState _autoState;
    PlayerDeadState _deadState;

    public event Action OnDead;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _anima = GetComponent<Animator>();
        _navMesh = GetComponent<NavMeshAgent>();
               


        //상태들 캐싱
        _moveState = new PlayerMoveState();
        _idleState = new PlayerIdleState();
        _autoState = new PlayerAutoState();
        _deadState = new PlayerDeadState();

        //상태 초기화
        _currentState = _idleState;
        _currentState.Enter(this);
    }

    private void OnEnable()
    {
        _playerStats.OnDead += ChangeDead;
    }

    private void OnDisable()
    {
        _playerStats.OnDead -= ChangeDead;
    }
    void Update()
    {
        //죽은상태면 리턴
        if (_isDead == true) return;

        //오토모드가 활성화되어있고 입력이 안들어오면
        if (_isAutoMode && _moveInput.sqrMagnitude < 0.001f)
        {
            ChangeState(_autoState);
        }

        //오토모드가 켜져있으면서 입력들어오면
        else if (_isAutoMode && _moveInput.sqrMagnitude > 0.001f)
        {
            ChangeState(_moveState);
        }

        //수동모드
        else if (!_isAutoMode && _moveInput.sqrMagnitude > 0.001f)
        {
            ChangeState(_moveState);
        }
        //키를 뗐다면
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            //초기화후 상태전환
            _navMesh.ResetPath();
            _moveInput = Vector2.zero;

            //오토모드면 오토모드로
            if (_isAutoMode)
            {
                ChangeState(_autoState);
            }
            else
            {
                ChangeState(_idleState);
            }
        }
            _currentState.Execute(this);
    }


    

    //오토/수동모드 전환용 토글
    public void ChangeAutoMode()
    {
        _isAutoMode = !_isAutoMode;
        Debug.Log($"AutoMode: {_isAutoMode}, Current State: {_currentState.GetType().Name}");
    }


    public void ChangeState(IPlayerState<PlayerCtrl> newState)
    {
        //같은상태면 전환못하게
        if (_currentState == newState) return;

        Debug.Log($"상태 변경: {_currentState.GetType().Name} → {newState.GetType().Name}");
        //상태아웃시키고 전환
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            //터치 시작 위치 저장
            _touchStart = ctx.ReadValue<Vector2>();
        }

        if (ctx.performed)
        {
            Vector2 current = ctx.ReadValue<Vector2>();

            //방향계산
            Vector2 delta = current - _touchStart;

            //드래그거리제한
            float distance = Mathf.Min(delta.magnitude, _dragDistance);

            //방향적용
            _moveInput = delta.normalized * (distance / _dragDistance);
        }

        if (ctx.canceled)
        {
            _moveInput = Vector2.zero;
        }
    }

    //이벤트 알림 구독함수
    void ChangeDead()
    {
        if (_isDead) return;

        _isDead = true;
        ChangeState(_deadState);

        OnDead?.Invoke();
    }


    //애니메이션 이벤트 함수
    public void ResetCombo() //콤보리셋
    {
        ComboIndex = 0;
        _anima.SetInteger("Combo", 0);
        _anima.SetBool("Attack", false);

    }

    public void StartCombo() //콤보시작
    {
        ComboIndex++;
        if (ComboIndex >= _maxComboIndex)
        {
            ComboIndex = 0;
        }
    }
    public void EnableAttackCollider()  //1,2히트박스활성화
    {
        _hitBox.enabled = true;
    }

    public void DisableAttackCollider() //1,2히트박스 비활성화
    {
        _hitBox.enabled = false;
    }

    public void EnableFinalAttackCollider()  //3히트박스활성화
    {
        _hitBox3.enabled = true;
    }

    public void DisableFinalAttackCollider() //3히트박스 비활성화
    {
        _hitBox3.enabled = false;
    }

    public void EnableAttackEffect1()
    {
        
    }

    public void DisableAttackEffect1()
    {
        
    }

    public void EnableAttackEffect2()
    {

    }

    public void DisableAttackEffect2()
    {

    }

    public void EnableAttackEffect3()
    {

    }

    public void DisableAttackEffect3()
    {

    }

    //public void Disable

    //에디터 체크용 기즈모
    void OnDrawGizmos()
    {
        //이동방향
        Gizmos.color = Color.red;
        Vector3 dir = new Vector3(_moveInput.x, 0, _moveInput.y);
        Gizmos.DrawLine(transform.position, transform.position + dir);

        //탐지범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _enemyFindRange);

        //공격범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

    }

    public void ApplyDamage(int amount)
    {
        if (_isDead) return;

        _playerStats.TakeDamage(amount);
    }

    //TestEnemy 임시클래스명
    public IMonster FindEnemy()
    {
        //EnemyFindRange 탐지범위안에 있는 콜라이더 가져오기
        Collider[] colliders =
        Physics.OverlapSphere(
            transform.position,
            EnemyFindRange,
            _enemyLayer);

        //초기값셋팅
        IMonster nearest = null;
        float minSqrDistance = EnemyFindRange * EnemyFindRange; // 비교 기준 거리

        foreach (Collider col in colliders)
        {
            if (!col.gameObject.activeInHierarchy) continue; // 비활성화 몬스터면 무시

            IMonster monster =
                col.GetComponentInParent<IMonster>(); // IMonster 확인

            if (monster == null) continue; // IMonster가 아니면 무시
            if (!monster.IsAlive) continue; // 이미 죽은 몬스터면 무시

            //플레이어와 몬스터 사이의 제곱거리 계산(루트 연산 피하기 위해서 성능 최적화)
            float sqrDist =
                (monster.Transform.position - transform.position).sqrMagnitude;

            //저장되있던 최소거리보다 가까우면
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist; // 최소거리 갱신
                nearest = monster; // 가까운적 갱신
            }
        }
        
        //외부에서쓸 타겟변수갱신
        _currentTarget = nearest;

        //가까운적 리턴
        //없으면 널리턴
        return nearest;
    }

    public void ResetState()
    {
        _isDead = false;

        //체력 복구
        _playerStats.ResetHPToMax();

        //상태 초기화
        ChangeState(_idleState);

        //애니메이션 초기화
        _anima.SetBool("Attack", false);
        _anima.SetBool("Run", false);
        _anima.SetInteger("Combo", 0);

        //이동 초기화
        _navMesh.ResetPath();
    }
}
