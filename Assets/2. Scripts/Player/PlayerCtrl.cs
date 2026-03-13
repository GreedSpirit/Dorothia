using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour, IMonsterTarget, IResettable
{
    [SerializeField] private RectTransform joystickBase;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Canvas canvas;

    //프로퍼티
    public Vector2 MoveInput => _moveInput;
    public PlayerStats PlayerStats => _playerStats;
    public Animator Anima => _anima;
    public NavMeshAgent NavMesh => _navMesh;
    public IMonster CurrentTarget => _currentTarget;
    public bool IsAutoMode => _isAutoMode;
    public float EnemyFindRange => _enemyFindRange;
    public float AttackRange => _attackRange;

    //콤보 체크용변수
    public int ComboIndex { get; set; } = 0;

    //공격상태 체크용변수
    public bool IsAttack { get; set; } = false;

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
    [SerializeField] ParticleSystem _attackHitEffect2;
    [SerializeField] ParticleSystem _attackHitEffect3;

    private Vector3 _originPos3;
    private Quaternion _originRot3;
    private Vector3 _originHitPos3;
    private Quaternion _originHitRot3;

    PlayerStats _playerStats;
    OverDriveMode _odm;
    Animator _anima;
    NavMeshAgent _navMesh;

    //외부에서쓸 타겟변수
    IMonster _currentTarget;

    //오토모드 체크용변수
    bool _isAutoMode = false;

    int _maxComboIndex = 3;

    bool _isDead = false;

    bool _isDrag = false;



    //입력값 저장변수
    Vector2 _moveInput;
    Vector2 _currentInput;
    Vector2 _touchStart;
    Vector2 _joyStickPos;

    //상태
    IPlayerState<PlayerCtrl> _currentState;
    PlayerMoveState _moveState;
    PlayerIdleState _idleState;
    PlayerAutoState _autoState;
    PlayerDeadState _deadState;

    public event Action OnDead;

    private int _currentSkillIndex = 0;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _anima = GetComponent<Animator>();
        _navMesh = GetComponent<NavMeshAgent>();
        _odm = GetComponent<OverDriveMode>();

        //네비매쉬로 회전못하게
        _navMesh.updateRotation = false;


        //상태들 캐싱
        _moveState = new PlayerMoveState();
        _idleState = new PlayerIdleState();
        _autoState = new PlayerAutoState();
        _deadState = new PlayerDeadState();

        //상태 초기화
        _currentState = _idleState;
        _currentState.Enter(this);

        _originPos3 = _attackEffect3.transform.localPosition;
        _originRot3 = _attackEffect3.transform.localRotation;

        _originHitPos3 = _attackHitEffect3.transform.localPosition;
        _originHitRot3 = _attackHitEffect3.transform.localRotation;
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
        //if (Keyboard.current.oKey.wasPressedThisFrame)
        //{
        //ExecuteNextSkill();
        //}

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
            if (_navMesh.isOnNavMesh)
                _navMesh.ResetPath();
            _moveInput = Vector2.zero;
            //_touchStart = Vector2.zero;

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

        //_hitBox.enabled = false;
        //_hitBox3.enabled = false;
        //상태아웃시키고 전환
        _currentState.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }


    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            //터치한위치값 저장
            Vector2 touchStart = Touchscreen.current.primaryTouch.position.ReadValue();

            //터치한위치에 UI가 없다면
            if (!IsPointerOverUI(touchStart))
            {
                //드래그가능
                _isDrag = true;

                //캔버스로컬좌표로 담아둘 변수
                Vector2 localPoint;

                //터치시작 월드좌표를 캔버스 좌표로 변환해서 localPoint에 대입
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                touchStart,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint);

                //localPoint로 앵커포지션 바꿔놓고
                joystickBase.anchoredPosition = localPoint;
                //조이스틱 활성화
                joystickBase.gameObject.SetActive(true);
                //핸들은 가운데로
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }
        //터치가 끝났을때 초기화
        if (ctx.canceled)
        {
            _isDrag = false;
            _moveInput = Vector2.zero;
            joystickHandle.anchoredPosition = Vector2.zero;
            joystickBase.gameObject.SetActive(false);
        }
    }
    public void OnMove(InputAction.CallbackContext ctx)
    {

        if (ctx.performed)
        {
            //UI위에서 터치했으면 isDrag가 false니깐 리턴
            if (!_isDrag) return;
            //ctx.ReadValue<Vector2>();
            Vector2 currentPos = Touchscreen.current.primaryTouch.position.ReadValue();

            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                currentPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint);

            //베이스 기준으로 delta 계산
            Vector2 delta = localPoint - joystickBase.anchoredPosition;

            //최대 거리 제한
            float distance = Mathf.Min(delta.magnitude, _dragDistance);

            //이동 입력값
            _moveInput = delta.normalized * (distance / _dragDistance);

            //핸들 위치 적용
            joystickHandle.anchoredPosition = delta.normalized * distance;
        }
    }


    //IsPointerOverGameObject()로는 체크가 안되서
    //레이캐스트로 UI체크하는 함수 작성
    private bool IsPointerOverUI(Vector2 Pos)
    {
        //현재 포인터이벤트 객체 생성하고
        PointerEventData eventData = new PointerEventData(EventSystem.current);

        //매개변수로 받을 좌표값을 대입
        eventData.position = Pos;

        //레이케스트결과들 담을 리스트들
        List<RaycastResult> results = new List<RaycastResult>();

        //터치한좌표기준 모든 UI검사
        EventSystem.current.RaycastAll(eventData, results);

        //UI가 하나라도있으면 참 없으면 거짓 반환
        return results.Count > 0;
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
        Debug.LogWarning("콤보리셋");

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

    public void EnableAttackEffect(int index)
    {
        Transform[] effectTransforms = { _attackEffect1.transform, _attackEffect2.transform, _attackEffect3.transform };
        Transform[] hitEffectTransforms = { _attackHitEffect.transform, _attackHitEffect2.transform, _attackHitEffect3.transform };

        if (index < 1 || index > 3) return;
        int arrayIdx = index - 1;

        ResetEffect3Transform();

        if (_odm.IsModeOn)
        {
            _attackEffect3.transform.SetPositionAndRotation(
                effectTransforms[arrayIdx].position,
                effectTransforms[arrayIdx].rotation
            );
            _attackHitEffect3.transform.SetPositionAndRotation(
                hitEffectTransforms[arrayIdx].position,
                hitEffectTransforms[arrayIdx].rotation
            );

            _attackEffect3.Play();
            _attackHitEffect3.Play();
        }
        else
        {
            // 일반 모드: 각 인덱스에 맞는 이펙트 재생
            switch (index)
            {
                case 1: _attackEffect1.Play(); _attackHitEffect.Play(); break;
                case 2: _attackEffect2.Play(); _attackHitEffect2.Play(); break;
                case 3: _attackEffect3.Play(); _attackHitEffect.Play(); _attackHitEffect2.Play(); break;
            }
        }
    }

    // 이펙트 3번을 원래 태어났던(?) 자리로 되돌리는 함수
    private void ResetEffect3Transform()
    {
        _attackEffect3.transform.localPosition = _originPos3;
        _attackEffect3.transform.localRotation = _originRot3;

        _attackHitEffect3.transform.localPosition = _originHitPos3;
        _attackHitEffect3.transform.localRotation = _originHitRot3;
    }


    //public void ExecuteNextSkill()
    //{
    //    // 스킬 매니저에서 장착된 액티브 스킬 리스트를 가져옴
    //    var skills = SkillManager.Instance.testSkill;
    //    //var skills = SkillManager.Instance.activeSkillSlots;

    //    //if (skills == null || skills.Count == 0) return;

    //    // 인덱스 범위 체크 (장착 해제 등으로 리스트가 줄었을 경우 대비)
    //    //if (_currentSkillIndex >= skills.Count) _currentSkillIndex = 0;

    //    // 현재 순서의 스킬 데이터 가져오기
    //    //BaseSkill skillToUse = skills[_currentSkillIndex];
    //    SkillSOData skillToUse = skills;

    //    if (skillToUse != null)
    //    {
    //        // 1. 애니메이터에 스킬 트리거 전달 
    //        // (BaseSkill 데이터에 정의된 애니메이션 파라미터 이름이 있다면 그것을 사용)
    //        _anima.SetTrigger("Skill");
    //        _anima.SetInteger("Skill_Id", skillToUse.Job_Skill_Id);


    //        // 만약 스킬마다 애니메이션이 다르다면 skillToUse.AnimationName 등으로 구분
    //        Debug.Log($"스킬 실행: {skillToUse.name} (Index: {_currentSkillIndex})");

    //        // 2. 다음 스킬을 위해 인덱스 증가 (순환 구조)
    //        //_currentSkillIndex = (_currentSkillIndex + 1) % skills.Count;
    //        _currentSkillIndex = Math.Min(_currentSkillIndex++, 4);
    //    }
    //}

    //// 스킬 애니메이션의 특정 프레임에서 호출될 이벤트 함수
    //public void OnSkillEffectTrigger()
    //{
    //    SkillSOData skill = SkillManager.Instance.testSkill;
    //    EffectManager.Instance.PlayEffect(skill.Skill_Effect, transform.position, transform.rotation);

    //    //// 현재 실행 중인 스킬의 인덱스는 방금 증가했으므로, 
    //    //// 실제 실행 중인 스킬은 (_currentSkillIndex - 1)입니다. (음수 처리 포함)
    //    //int lastIdx = (_currentSkillIndex - 1 + SkillManager.Instance.activeSkillSlots.Count) % SkillManager.Instance.activeSkillSlots.Count;
    //    //BaseSkill currentSkill = SkillManager.Instance.activeSkillSlots[lastIdx];

    //    //if (currentSkill != null)
    //    //{
    //    //    // 여기서 스킬 고유의 이펙트를 생성하거나 재생합니다.
    //    //    // 스킬 데이터(BaseSkill)에 이펙트 프리팹 정보가 있다면 여기서 Instantiate 하거나
    //    //    // 전용 파티클 시스템을 재생하는 로직을 넣으세요.
    //    //    Debug.Log($"{currentSkill.name} 이펙트 발동!");

    //    //    // 예: 스킬 자체에 있는 실행 로직 호출 (데미지 계산 등)
    //    //    // currentSkill.Use(this); 
    //    //}
    //}

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
        if (_navMesh.isOnNavMesh)
            _navMesh.ResetPath();
    }
}
