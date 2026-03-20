using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// ProjectileModule이 Init()으로 초기화하는 투사체.
/// 타겟을 향해 유도되며, 충돌 또는 도달 시 hitCount만큼 데미지를 적용한다.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class SkillProjectile : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private TrailRenderer _trail;       // 선택: 꼬리 이펙트
    [SerializeField] private GameObject    _hitFxPrefab; // 선택: 충돌 이펙트

    // ── 런타임 데이터 ────────────────────────────────────────────────
    private IMonster _target;
    private PlayerCtrl  _caster;
    private int         _hitCount;
    private float       _speed;

    private bool  _initialized;
    private bool  _isHit;

    // 타겟 소멸 대비: 마지막으로 알려진 위치 저장
    private Vector3 _lastKnownTargetPos;

    // 타겟에 도달했다고 판정할 거리
    private const float ArrivalThreshold = 0.35f;
    // Init 없이 너무 오래 날아가는 것 방지
    private const float MaxLifetime = 10f;

    // ── 초기화 ───────────────────────────────────────────────────────
    /// <summary>ProjectileModule에서 Instantiate 직후 호출</summary>
    public void Init(
        IMonster target,
        int         hitCount,
        float       speed,
        PlayerCtrl  caster)
    {
        _target      = target;
        _hitCount    = Mathf.Max(1, hitCount);
        _speed       = speed;
        _caster      = caster;
        _isHit       = false;
        _initialized = true;

        _lastKnownTargetPos = target?.Transform != null
            ? target.Transform.position
            : transform.position + transform.forward * 5f;

        // 콜라이더를 트리거로 설정
        if (TryGetComponent<Collider>(out var col))
            col.isTrigger = true;

        // 수명 제한
        StartCoroutine(LifetimeRoutine());
    }

    // ── 이동 ─────────────────────────────────────────────────────────
    private void Update()
    {
        if (!_initialized || _isHit) return;

        // 타겟이 살아있으면 위치 추적
        if (_target?.Transform != null && (_target is not IMonster dmg || dmg.IsAlive))
            _lastKnownTargetPos = _target.Transform.position/* + Vector3.up * 0.8f*/;

        Vector3 dir      = (_lastKnownTargetPos - transform.position);
        float   distance = dir.magnitude;

        // 도달 판정: 거리 임계값 이하
        if (distance <= ArrivalThreshold)
        {
            OnArrival();
            return;
        }

        // 유도 이동
        transform.position += dir.normalized * (_speed * Time.deltaTime);
        transform.rotation  = Quaternion.LookRotation(dir.normalized);
    }

    // ── 충돌 감지 ────────────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (_isHit) return;

        // 시전자 자신은 무시
        if (_caster != null && other.gameObject == _caster.gameObject) return;

        if (other.TryGetComponent<IMonster>(out var damageable))
        {
            ApplyDamage(damageable, other.ClosestPoint(transform.position));
        }
    }

    // ── 도달 처리 (트리거가 없는 경우 보조) ─────────────────────────
    private void OnArrival()
    {
        if (_isHit) return;

        if (_target is IMonster damageable)
            ApplyDamage(damageable, _lastKnownTargetPos);
        else
            DestroySelf();
    }

    // ── 데미지 적용 ──────────────────────────────────────────────────
    private void ApplyDamage(IMonster damageable, Vector3 hitPoint)
    {
        if (_isHit) return;
        _isHit = true;

        if (damageable.IsAlive)
        {
            // hitCount만큼 연속 타격
            for (int i = 0; i < _hitCount; i++)
            {
                float damage = _caster.CalculateSkillDamage(_caster.SkillState.TargetSkill);
                damageable.TakeDamage((int)damage);
            }
        }

        SpawnHitFx(hitPoint);
        DestroySelf();
    }

    // ── 수명 코루틴 ──────────────────────────────────────────────────
    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(MaxLifetime);
        if (!_isHit) DestroySelf();
    }

    // ── 정리 ─────────────────────────────────────────────────────────
    private void DestroySelf()
    {
        // Trail이 있으면 분리 후 자동 소멸되도록 처리
        if (_trail != null)
        {
            _trail.transform.SetParent(null);
            _trail.autodestruct = true;
        }
        Destroy(gameObject);
    }

    private void SpawnHitFx(Vector3 pos)
    {
        if (_hitFxPrefab == null) return;
        var fx = Instantiate(_hitFxPrefab, pos, Quaternion.identity);
        Destroy(fx, 3f);
    }

    //private float CalculateDamage() => _caster != null ? _caster.Stat.SkillDamage : 10f;

    private bool RollCritical() => _caster != null && Random.value < _caster.PlayerStats._crt_prob;
}
