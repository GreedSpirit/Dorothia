using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour
{
    //프로퍼티
    public Vector2 MoveInput => _moveInput;
    public PlayerStats PlayerStats => _playerStats;
    public Animator Anima => _anima;
    public NavMeshAgent NavMesh => _navMesh;
    public bool IsAutoMode => _isAutoMode;
    public float EnemyFindRange => _enemyFindRange;
    public float AttackRange => _attackRange;


    [SerializeField] PlayerStats _playerStats;

    [SerializeField] LayerMask _enemyLayer;

    //드래그 사거리
    [SerializeField] float _dragDistance = 100f;

    //적 탐지범위
    [SerializeField] float _enemyFindRange = 20f;

    //공격실행범위
    [SerializeField] float _attackRange = 1f;

    Animator _anima;
    NavMeshAgent _navMesh;

    //오토모드 체크용변수
    bool _isAutoMode = false;

    //입력값 저장변수
    Vector2 _moveInput;
    Vector2 _currentInput;
    Vector2 _touchStart;

    //상태
    IPlayerState<PlayerCtrl> _currentState;
    PlayerMoveState _moveState;
    PlayerIdleState _idleState;
    PlayerAutoState _autoState;


    private void Awake()
    {
        _anima = GetComponent<Animator>();
        _navMesh = GetComponent<NavMeshAgent>();
        

        //상태들 캐싱
        _moveState = new PlayerMoveState();
        _idleState = new PlayerIdleState();
        _autoState = new PlayerAutoState();

        //상태 초기화
        _currentState = _idleState;
        _currentState.Enter(this);
    }
    void Update()
    {
        //오토모드가 활성화되어있고 입력이 안들어오면
        if (_isAutoMode && _moveInput.sqrMagnitude < 0.001f)
        {
            ChangeState(_autoState);
        }

        //오토모드가 켜져있으면서 입력들어오면
        else if (_isAutoMode && _moveInput.sqrMagnitude > 0.001f)
        {
            ChangeState(_moveState);
            Debug.Log("무브상태");
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
        Debug.Log(_isAutoMode);
    }


    public void ChangeState(IPlayerState<PlayerCtrl> newState)
    {
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

    //에디터 체크용 기즈모
    void OnDrawGizmos()
    {
        //이동방향
        Gizmos.color = Color.red;
        Vector3 dir = new Vector3(_moveInput.x, 0, _moveInput.y);
        Gizmos.DrawLine(transform.position, transform.position + dir);

        //탐지범위
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, _enemyFindRange);

        //공격범위
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, _attackRange);

    }
}
