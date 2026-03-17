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

    //  프로퍼티
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

    public int ComboIndex { get; set; } = 0;
    public bool IsAttack { get; set; } = false;

    // 캐싱
    private PlayerStats _playerStats;
    private OverDriveMode _odm;
    private Animator _anima;
    private AnimatorOverrideController _overrideController;
    private NavMeshAgent _navMesh;
    private IMonster _currentTarget;

    // 상태 인스턴스
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerAttackState AttackState { get; private set; }
    public PlayerSkillState SkillState { get; private set; }
    public PlayerAutoState AutoState { get; private set; }
    public PlayerDeadState DeadState { get; private set; }

    private IPlayerState<PlayerCtrl> _currentState;

    // 내부 변수 
    private bool _isDead = false;
    private bool _isAutoMode = false;
    private bool _isDrag = false;
    private Vector2 _moveInput;

    private Vector3 _originPos3, _originHitPos3;
    private Quaternion _originRot3, _originHitRot3;

    public event Action OnDead;

    // ══════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        _anima = GetComponent<Animator>();
        _overrideController = new AnimatorOverrideController(_anima.runtimeAnimatorController);
        _anima.runtimeAnimatorController = _overrideController;
        _navMesh = GetComponent<NavMeshAgent>();
        _odm = GetComponent<OverDriveMode>();
        _navMesh.updateRotation = false;

        IdleState = new PlayerIdleState();
        MoveState = new PlayerMoveState();
        AttackState = new PlayerAttackState();
        SkillState = new PlayerSkillState();
        AutoState = new PlayerAutoState();
        DeadState = new PlayerDeadState();

        _currentState = IdleState;
        _currentState.Enter(this);

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

        if (Keyboard.current.sKey.wasPressedThisFrame)
            TryUseSkillById(10001);

        UpdateGlobalState();
        _currentState.Execute(this);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region State Machine

    private void UpdateGlobalState()
    {
        if (_currentState == SkillState) return;

        bool hasMoveInput = _moveInput.sqrMagnitude > 0.001f;
        if (IsAttack) return;

        if (hasMoveInput) ChangeState(MoveState);
        else if (_isAutoMode) ChangeState(AutoState);
        else if (_currentState != IdleState) ChangeState(IdleState);
    }

    public void ChangeState(IPlayerState<PlayerCtrl> newState)
    {
        if (_currentState == newState || _isDead) return;

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

    #endregion

    // ══════════════════════════════════════════════════════
    #region Skill - 진입점

    // ID로 스킬 생성 후 사용 (테스트 / 퀵슬롯 연동용)
    public void TryUseSkillById(int skillId)
    {
        if (_currentState == SkillState || _isDead) return;

        SkillData data = DataManager.Instance.GetData<SkillData>(skillId);
        BaseSkill skill = BaseSkill.Create(data);

        TryUseSkill(skill);
    }

    // 이미 생성된 스킬 인스턴스로 사용 (SkillManager 슬롯 연동용)
    public void TryUseSkill(BaseSkill skill)
    {
        if (_currentState == SkillState || _isDead) return;
        if (skill == null || !skill.IsReady) return;

        SkillState.SetSkill(skill);
        ChangeState(SkillState);
    }

    // AutoState 등 내부에서 바로 전환 (IsReady 체크 생략)
    public void PerformSkill(BaseSkill skill)
    {
        if (skill == null || _isDead) return;

        SkillState.SetSkill(skill);
        ChangeState(SkillState);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Skill - 애니메이션 이벤트 수신

    // 애니메이터 이벤트에서 직접 호출 ─ ModularSkill에 전달
    private ModularSkill GetModularSkill()
        => SkillState?.TargetSkill as ModularSkill;

    public void OnSkillHit() => GetModularSkill()?.NotifyHit(this);
    public void OnSkillDash() => GetModularSkill()?.NotifyDash(this);
    public void OnSkillEffect() => GetModularSkill()?.NotifyEffect(this);
    public void OnSkillJumpPeak() => GetModularSkill()?.NotifyJumpPeak(this);
    public void OnSkillJumpLand() => GetModularSkill()?.NotifyJumpLand(this);

    #endregion

    // ══════════════════════════════════════════════════════
    #region Skill - 데미지 / 히트 루틴

    // MeleeAttackModule, JumpAttackModule 에서 접근
    internal float CalculateSkillDamage(BaseSkill skill)
    {
        if (skill?.Data == null) return 0f;
        return (float)StatManager.Instance.stats[skill.Data.Affection_Skill].FinalValue
               * skill.Data.Affection_Skill_Value;
    }

    // 단일 타겟 연타 (MeleeAttackModule 단일 모드)
    internal IEnumerator SingleHitRoutine(IMonster target, int hitCount, float totalDamage)
    {
        float dmgPerHit = totalDamage / Mathf.Max(hitCount, 1);

        for (int i = 0; i < hitCount; i++)
        {
            if (target == null || !target.IsAlive) yield break;

            target.TakeDamage((int)dmgPerHit);
            // EffectManager.Instance.PlayEffect(...);

            yield return new WaitForSeconds(0.08f);
        }
    }

    // 다수 타겟 연타 (MeleeAttackModule AOE 모드 / JumpAttackModule)
    internal IEnumerator MultiHitRoutine(Collider[] targets, int hitCount, float totalDamage)
    {
        float dmgPerHit = totalDamage / Mathf.Max(hitCount, 1);

        for (int i = 0; i < hitCount; i++)
        {
            foreach (var col in targets)
            {
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                IMonster monster = col.GetComponentInParent<IMonster>();
                if (monster != null && monster.IsAlive)
                    monster.TakeDamage((int)1);
                    //monster.TakeDamage((int)dmgPerHit);
            }

            yield return new WaitForSeconds(0.08f);
        }
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Skill - 렌더러 / 애니메이터 제어 (JumpAttackModule 전용)

    // 캐릭터 메시 전체 표시 / 숨김
    public void SetRenderersEnabled(bool isEnabled)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = isEnabled;
    }

    // 애니메이션 일시정지 / 재개 (D스킬 정점 대기)
    public void PauseAnimation() => _anima.speed = 0f;
    public void ResumeAnimation() => _anima.speed = 1f;

    #endregion

    // ══════════════════════════════════════════════════════
    #region Combo / Attack

    public void StartCombo()
    {
        IMonster target = FindEnemy();
        if (target != null && target.IsAlive &&
            Vector3.Distance(transform.position, target.Transform.position) <= _attackRange)
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
        if (_anima.IsInTransition(0) || _anima.GetInteger("Combo") > ComboIndex) return;
        ExecuteFullReset();
    }

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

    #endregion

    // ══════════════════════════════════════════════════════
    #region Collider / Effect

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

    #endregion

    // ══════════════════════════════════════════════════════
    #region Utility

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

    public void SetupNavMesh(bool isAuto)
    {
        if (isAuto)
        {
            _navMesh.acceleration = 60f;
            _navMesh.stoppingDistance = 0f;
        }
        else
        {
            _navMesh.acceleration = 12f;
            _navMesh.stoppingDistance = _attackRange - 0.2f;
        }
    }

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

    #endregion

    // ══════════════════════════════════════════════════════
    #region Input

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
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out localPoint);
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
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out localPoint);

            Vector2 delta = localPoint - joystickBase.anchoredPosition;
            float distance = Mathf.Min(delta.magnitude, _dragDistance);
            _moveInput = delta.normalized * (distance / _dragDistance);
            joystickHandle.anchoredPosition = delta.normalized * distance;
        }
    }

    private bool IsPointerOverUI(Vector2 pos)
    {
        var eventData = new PointerEventData(EventSystem.current) { position = pos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _enemyFindRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}