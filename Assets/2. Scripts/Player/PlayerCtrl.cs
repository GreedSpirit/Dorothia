using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour
{

    //프로퍼티
    public Vector2 MoveInput => _moveInput;
    public PlayerStats PlayerStats => _playerStats;
    public Animator Anima => _anima;

    [SerializeField] PlayerStats _playerStats;
    //드래그 사거리
    [SerializeField] float dragDistance = 100f;

    Animator _anima;

    //입력값 저장변수
    Vector2 _moveInput;
    Vector2 _currentInput;
    Vector2 _touchStart;

    //상태
    IPlayerState<PlayerCtrl> _currentState;
    PlayerMoveState _moveState;
    PlayerIdleState _idleState;


    private void Awake()
    {
        _anima = GetComponent<Animator>();
        

        //상태들 캐싱
        _moveState = new PlayerMoveState();
        _idleState = new PlayerIdleState();

        //상태 초기화
        _currentState = _idleState;
        _currentState.Enter(this);
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        if (_moveInput.sqrMagnitude > 0.001f)
        {
            ChangeState(_moveState);
            Debug.Log("무브상태");
        }

        //키를 뗐다면
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            //초기화후 상태전환
            _moveInput = Vector2.zero;
            ChangeState(_idleState);
        }
        _currentState.Execute(this);
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
            float distance = Mathf.Min(delta.magnitude, dragDistance);

            //방향적용
            _moveInput = delta.normalized * (distance / dragDistance);
        }
    }

    //에디터상 체크용 기즈모
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 dir = new Vector3(_moveInput.x, 0, _moveInput.y);
        Gizmos.DrawLine(transform.position, transform.position + dir);
    }
}
