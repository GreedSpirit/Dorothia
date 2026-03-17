using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerCtrl : MonoBehaviour, IMonsterTarget, IResettable
{
    [Header("조이스틱 & UI 설정")]
    [SerializeField] private RectTransform joystickBase;
    [SerializeField] private RectTransform joystickHandle;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private Canvas canvas;

    [Header("전투 설정")]
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _dragDistance = 100f;
    [SerializeField] private float _enemyFindRange = 20f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private int _maxComboIndex = 3;

    [Header("히트박스 & 이펙트")]
    [SerializeField] private BoxCollider _hitBox;
    [SerializeField] private BoxCollider _hitBox3;
    [SerializeField] private ParticleSystem _attackEffect1, _attackEffect2, _attackEffect3;
    [SerializeField] private ParticleSystem _attackHitEffect, _attackHitEffect2, _attackHitEffect3;

    // 프로퍼티
    public Vector2 MoveInput => _moveInput;
    public PlayerStats PlayerStats => _playerStats;
    public Animator Anima => _anima;
    public AnimatorOverrideController OverrideController => _overrideController;
    public NavMeshAgent NavMesh => _navMesh;
    public IMonster CurrentTarget => _currentTarget;
    public bool IsAutoMode => _isAutoMode;
    public float AttackRange => _attackRange;
    public float EnemyFindRange => _enemyFindRange;
    public Transform Transform => transform;
    public bool IsAlive => !_isDead;

    // 전투 관련 상태 변수 : 1부터 시작
    public int ComboIndex { get; set; } = 0;
    public bool IsAttack { get; set; } = false;

    // 캐싱용 컴포넌트
    private PlayerStats _playerStats;
    private OverDriveMode _odm;
    private Animator _anima;
    private AnimatorOverrideController _overrideController;
    private NavMeshAgent _navMesh;
    private IMonster _currentTarget;

    // 상태 클래스 인스턴스
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerSkillState SkillState { get; private set; }
    public PlayerAutoState AutoState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    private IPlayerState<PlayerCtrl> _currentState;

    // 기타 내부 변수
    private bool _isDead = false;
    private bool _isAutoMode = false;
    private bool _isDrag = false;
    private Vector2 _moveInput;

    // 이펙트 복구용 데이터
    private Vector3 _originPos3, _originHitPos3;
    private Quaternion _originRot3, _originHitRot3;

    public event Action OnDead;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _anima = GetComponent<Animator>();
        _overrideController = new AnimatorOverrideController(_anima.runtimeAnimatorController);
        _anima.runtimeAnimatorController = _overrideController;
        _navMesh = GetComponent<NavMeshAgent>();
        _odm = GetComponent<OverDriveMode>();
        _navMesh.updateRotation = false;

        // 상태 초기화
        IdleState = new PlayerIdleState();
        MoveState = new PlayerMoveState();
        AttackState = new PlayerAttackState();
        SkillState = new PlayerSkillState();
        AutoState = new PlayerAutoState();
        DeadState = new PlayerDeadState();

        _currentState = IdleState;
        _currentState.Enter(this);

        // 이펙트 데이터 백업
        _originPos3 = _attackEffect3.transform.localPosition;
        _originRot3 = _attackEffect3.transform.localRotation;
        _originHitPos3 = _attackHitEffect3.transform.localPosition;
        _originHitRot3 = _attackHitEffect3.transform.localRotation;
    }

    private void OnEnable() => _playerStats.OnDead += ChangeDead;
    private void OnDisable() => _playerStats.OnDead -= ChangeDead;

    private void Update()
    {
        if (_isDead) return;

        //// 스킬 입력 예시 (S키)
        //if (Keyboard.current.sKey.wasPressedThisFrame)
        //{
        //    // 실제로는 퀵슬롯이나 스킬 매니저에서 가져옴
        //    TryUseSkill(10001);
        //}

        UpdateGlobalState();
        _currentState.Execute(this);
    }

    public void TryUseSkill(int skillId)
    {
        // 이미 스킬 중이면 중복 사용 방지 (필요 시)
        if (_currentState == SkillState) return;

        SkillData data = DataManager.Instance.GetData<SkillData>(skillId);
        Skill_StatusData statusData = DataManager.Instance.GetData<Skill_StatusData>(data.Skill_Status_Id);

        BaseSkill skill = BaseSkill.Create(data, statusData);
        SkillState.SetSkill(skill);
        ChangeState(SkillState);
    }

    private void UpdateGlobalState()
    {
        // [핵심] 스킬 상태일 때는 이동이나 Idle로의 전이를 원천 차단
        if (_currentState == SkillState) return;

        bool hasMoveInput = _moveInput.sqrMagnitude > 0.001f;
        if (IsAttack) return; // 기본공격 중에도 대기

        if (hasMoveInput) ChangeState(MoveState);
        else if (_isAutoMode) ChangeState(AutoState);
        else if (_currentState != IdleState) ChangeState(IdleState);
    }

    public void ChangeState(IPlayerState<PlayerCtrl> newState)
    {
        if (_currentState == newState || _isDead) return;

        Debug.Log($"{newState.ToString()}");

        _currentState.Exit(this);
        _currentState = newState;
        SetupNavMesh(_isAutoMode);
        _currentState.Enter(this);
    }

    private void OnTouchReleased()
    {
        if (_navMesh.isOnNavMesh) _navMesh.ResetPath();
        _moveInput = Vector2.zero;

        if (!_isAutoMode && !IsAttack && _currentState != SkillState)
            ChangeState(IdleState);
    }


    #region Skill
    public void PerformSkill(BaseSkill skill)
    {
        if (skill == null) return;
        SkillState.SetSkill(skill);

        ChangeState(SkillState);
    }
    public void RequestUseSkill(BaseSkill skill)
    {
        // 현재 스킬을 쓸 수 있는 상태인지 체크
        if (_currentState == SkillState || _isDead) return;

        // 스킬 준비 상태 체크 (쿨타임 등)
        if (skill != null && skill.IsReady)
        {
            PerformSkill(skill);
        }
    }
    public void OnSkillEffect()
    {
        if (SkillState.TargetSkill == null) return;
        string addr = SkillState.TargetSkill.Data.Skill_Effect_Patch;
        EffectManager.Instance.PlayEffect(addr, 2f, transform.position, transform.rotation);
    }

    public void OnSkillHit(int hitCount)
    {
        if (SkillState.TargetSkill == null) return;

        // 1. 스킬 데이터에서 범위(Radius) 가져오기 (없으면 기본값 3.0f)
        //float radius = SkillState.TargetSkill.Data.Skill_Range;
        float radius = 5;

        // 2. 물리 오버랩으로 범위 내 콜라이더 검출 (몬스터 레이어만 체크 권장)
        int monsterLayer = LayerMask.GetMask("Monster");
        Collider[] targets = Physics.OverlapSphere(transform.position, radius, monsterLayer);

        if (targets.Length > 0)
        {
            //hitCount = SkillState.TargetSkill.Data.hitCount;
            hitCount = 4;
            StartCoroutine(MultiHitRoutine(targets, hitCount));
        }
    }

    private IEnumerator MultiHitRoutine(Collider[] targets, int hitCount)
    {
        var skill = SkillState.TargetSkill;
        if (skill == null) yield break;

        // 총 데미지를 타수(hitCount)로 나눔
        float totalDamage = CalculateSkillDamage(skill);
        float damagePerHit = totalDamage / hitCount;

        // 크리티컬 여부 계산 (예시: 20% 확률)
        //bool isCritical = UnityEngine.Random.value < 0.2f;

        for (int i = 0; i < hitCount; i++)
        {
            foreach (var target in targets)
            {
                // 이미 파괴되었거나 비활성화된 경우 제외
                if (target == null || !target.gameObject.activeInHierarchy) continue;

                IMonster monster = target.GetComponentInParent<IMonster>();
                if (monster != null && monster.IsAlive)
                {
                    // 1. 실제 데미지 적용
                    //monster.TakeDamage((int)damagePerHit);
                    monster.TakeDamage((int)1);

                    // 2. 데미지 텍스트 출력 (앞서 만든 DamageTextManager 활용)
                    // 타격 위치에 약간의 오프셋을 주어 텍스트가 겹치지 않게 함
                    //DamageTextManager.Instance.ShowDamage((int)damagePerHit, target.transform.position, isCritical);

                    // 3. 히트 이펙트 재생 (옵션)
                    // EffectManager.Instance.PlayEffect(skill.Data.Skill_Hit_Effect, 1f, target.transform.position, Quaternion.identity);
                }
            }

            // 연타 사이의 짧은 간격
            yield return new WaitForSeconds(0.08f);
        }
    }

    private float CalculateSkillDamage(BaseSkill skill)
    {
        // (스테이터스 최종 값 * 스킬 퍼센트) * (등급에따른스테이터스상승값) ∗ (강화증가폭∗ 강화횟수) <- 뭔지 물어봐야함
        float damage = (float)StatManager.Instance.stats[skill.Status.Affection_Skill].FinalValue * skill.Status.Affection_Skill_Value;

        // 계산된 최종 데미지
        return damage;
    }
    #endregion

    public void StartCombo()
    {
        // 타겟이 있고 사거리 안에 있을 때만 다음 콤보 예약
        IMonster target = FindEnemy();
        if (target != null && target.IsAlive && Vector3.Distance(transform.position, target.Transform.position) <= _attackRange)
        {
            ComboIndex = (ComboIndex % _maxComboIndex) + 1;
            _anima.SetInteger("Combo", ComboIndex);
        }
        else
        {
            ResetCombo();
        }
    }

    public void ResetCombo()
    {
        // 다음 공격으로 전이 중이거나 예약된 숫자가 현재보다 크면 초기화 방지
        if (_anima.IsInTransition(0) || _anima.GetInteger("Combo") > ComboIndex) return;

        ExecuteFullReset();
    }

    // 상태 강제 초기화 (이동 캔슬이나 마지막 타격 후 호출)
    public void ExecuteFullReset()
    {
        ComboIndex = 0;
        IsAttack = false;
        _anima.SetInteger("Combo", 0);
        _anima.SetBool("Attack", false);
        DisableAllAttackColliders();

        if (!_isDead && _currentState != SkillState)
            ChangeState(IdleState);
    }

    // --- 콜라이더 및 이펙트 관리 ---

    public void EnableAttackCollider() => _hitBox.enabled = true;
    public void DisableAttackCollider() => _hitBox.enabled = false;
    public void EnableFinalAttackCollider() => _hitBox3.enabled = true;
    public void DisableFinalAttackCollider() => _hitBox3.enabled = false;

    public void DisableAllAttackColliders()
    {
        _hitBox.enabled = false;
        _hitBox3.enabled = false;
    }

    public void EnableAttackEffect(int index)
    {
        ResetEffect3Transform();

        if (_odm.IsModeOn)
        {
            _attackEffect3.Play();
            _attackHitEffect3.Play();
            return;
        }

        switch (index)
        {
            case 1: _attackEffect1.Play(); _attackHitEffect.Play(); break;
            case 2: _attackEffect2.Play(); _attackHitEffect2.Play(); break;
            case 3:
                _attackEffect3.Play();
                _attackHitEffect.Play();
                _attackHitEffect2.Play();
                _attackHitEffect3.Play();
                break;
        }
    }

    private void ResetEffect3Transform()
    {
        _attackEffect3.transform.localPosition = _originPos3;
        _attackEffect3.transform.localRotation = _originRot3;
        _attackHitEffect3.transform.localPosition = _originHitPos3;
        _attackHitEffect3.transform.localRotation = _originHitRot3;
    }

    // --- 유틸리티 및 인터페이스 구현 ---

    public IMonster FindEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _enemyFindRange, _enemyLayer);
        IMonster nearest = null;
        float minSqrDistance = _enemyFindRange * _enemyFindRange;

        foreach (var col in colliders)
        {
            IMonster monster = col.GetComponentInParent<IMonster>();
            if (monster == null || !monster.IsAlive) continue;

            float sqrDist = (monster.Transform.position - transform.position).sqrMagnitude;
            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                nearest = monster;
            }
        }
        _currentTarget = nearest;
        return nearest;
    }

    public void ChangeAutoMode() => _isAutoMode = !_isAutoMode;

    private void ChangeDead()
    {
        if (_isDead) return;
        _isDead = true;
        ExecuteFullReset();
        ChangeState(DeadState);
        OnDead?.Invoke();
    }

    public void ApplyDamage(int amount) { if (!_isDead) _playerStats.TakeDamage(amount); }

    public void ResetState()
    {
        _isDead = false;
        _playerStats.ResetHPToMax();
        ExecuteFullReset();
        if (_navMesh.isOnNavMesh) _navMesh.ResetPath();
    }
    public void SetupNavMesh(bool isManual)
    {
        if (isManual)
        {
            // 즉각적인 반응을 위해 높은 가속도
            _navMesh.acceleration = 60f;
            _navMesh.stoppingDistance = 0f;
        }
        else
        {
            // 부드러운 이동
            _navMesh.acceleration = 12f;
            // 사거리보다 약간 앞에서 멈춤
            _navMesh.stoppingDistance = _attackRange - 0.2f;
        }
    }

    public void OnClick(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            Vector2 touchStart = Touchscreen.current.primaryTouch.position.ReadValue();
            if (!IsPointerOverUI(touchStart))
            {
                _isDrag = true;
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, touchStart,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint);

                joystickBase.anchoredPosition = localPoint;
                joystickBase.gameObject.SetActive(true);
                joystickHandle.anchoredPosition = Vector2.zero;
            }
        }
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
        if (ctx.performed && _isDrag)
        {
            Vector2 currentPos = Touchscreen.current.primaryTouch.position.ReadValue();
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, currentPos,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, out localPoint);

            Vector2 delta = localPoint - joystickBase.anchoredPosition;
            float distance = Mathf.Min(delta.magnitude, _dragDistance);
            _moveInput = delta.normalized * (distance / _dragDistance);
            joystickHandle.anchoredPosition = delta.normalized * distance;
        }
    }

    private bool IsPointerOverUI(Vector2 Pos)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Pos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _enemyFindRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}