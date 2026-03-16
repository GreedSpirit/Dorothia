// 타겟 탐색 + 회전 모듈 (거의 모든 스킬에 포함)
using System.Collections;
using UnityEngine;

public class TargetLockModule : BaseSkillModule
{
    public override void OnExecute(PlayerCtrl player, SkillContext ctx)
    {
        IMonster target = player.FindEnemy();
        if (target == null || !target.IsAlive)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        ctx.LockedTarget = target;

        // 타겟 방향 즉시 회전
        Vector3 dir = (target.Transform.position - player.transform.position);
        dir.y = 0;
        ctx.DashDirection = dir.normalized;

        if (dir != Vector3.zero)
            player.transform.rotation = Quaternion.LookRotation(dir);
    }
}

// 근접 공격 모듈
public class MeleeAttackModule : BaseSkillModule
{
    private readonly bool _isAoe;
    private readonly float _aoeRadius;

    public MeleeAttackModule() { _isAoe = false; }
    public MeleeAttackModule(float radius) { _isAoe = true; _aoeRadius = radius; }

    // ★ 파라미터 없음 - 테이블의 HitCount 사용
    public override void OnHit(PlayerCtrl player, SkillContext ctx)
    {
        float totalDmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

        if (_isAoe)
        {
            Collider[] hits = Physics.OverlapSphere(
                player.transform.position, _aoeRadius, LayerMask.GetMask("Monster"));

            if (hits.Length > 0)
                player.StartCoroutine(player.MultiHitRoutine(hits, HitCount, totalDmg));
        }
        else
        {
            if (ctx.LockedTarget == null || !ctx.LockedTarget.IsAlive) return;
            player.StartCoroutine(player.SingleHitRoutine(ctx.LockedTarget, HitCount, totalDmg));
        }
    }
}

// 투사체 발사 모듈
//public class ProjectileModule : BaseSkillModule
//{
//    private readonly string _projectilePrefabAddr; // Addressable 주소
//    private readonly float _speed;

//    public ProjectileModule(string prefabAddr, float speed = 15f)
//    {
//        _projectilePrefabAddr = prefabAddr;
//        _speed = speed;
//    }

//    public override void OnHit(PlayerCtrl player, SkillContext ctx, int hitCount)
//    {
//        if (ctx.LockedTarget == null) return;

//        // 투사체 생성 (Addressable / Pool 방식에 맞게 교체)
//        AddressableManager.Instance.LoadAsset<GameObject>(_projectilePrefabAddr, prefab =>
//        {
//            var go = Object.Instantiate(prefab, player.transform.position + Vector3.up, Quaternion.identity);
//            var projectile = go.GetComponent<SkillProjectile>();
//            projectile.Init(ctx.LockedTarget, hitCount, _speed, player);
//        });
//    }
//}

// 순간이동 모듈
public class TeleportModule : BaseSkillModule
{
    private readonly float _behindOffset;

    public TeleportModule(float behindOffset = 1.2f)
    {
        _behindOffset = behindOffset;
    }

    public override void OnExecute(PlayerCtrl player, SkillContext ctx)
    {
        if (ctx.LockedTarget == null) return;

        Vector3 targetPos = ctx.LockedTarget.Transform.position;
        Vector3 teleportPos = targetPos - ctx.DashDirection * _behindOffset;

        if (UnityEngine.AI.NavMesh.SamplePosition(teleportPos, out var hit, 2f,
            UnityEngine.AI.NavMesh.AllAreas))
        {
            player.NavMesh.Warp(hit.position);
        }

        // 순간이동 후 타겟 방향 재정렬
        Vector3 lookDir = (targetPos - player.transform.position);
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            player.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}

// 대쉬 모듈
public class DashModule : BaseSkillModule
{
    private readonly float _distance;
    private readonly float _duration;

    public DashModule(float distance = 5f, float duration = 0.3f)
    {
        _distance = distance;
        _duration = duration;
    }

    public override void OnDash(PlayerCtrl player, SkillContext ctx)
    {
        player.StartCoroutine(DashRoutine(player, ctx));
    }

    private IEnumerator DashRoutine(PlayerCtrl player, SkillContext ctx)
    {
        float elapsed = 0f;
        Vector3 startPos = player.transform.position;
        Vector3 destination = startPos + ctx.DashDirection * _distance;

        if (UnityEngine.AI.NavMesh.SamplePosition(destination, out var hit, 3f,
            UnityEngine.AI.NavMesh.AllAreas))
            destination = hit.position;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _duration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // EaseOut Cubic

            player.NavMesh.Warp(Vector3.Lerp(startPos, destination, easedT));
            yield return null;
        }

        player.NavMesh.Warp(destination);
    }
}

// 점프 공격 모듈
public class JumpAttackModule : BaseSkillModule
{
    private readonly float _effectDuration;

    public JumpAttackModule(float effectDuration = 1.5f)
    {
        _effectDuration = effectDuration;
    }

    public override void OnJumpPeak(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(PeakRoutine(player, ctx));

    private IEnumerator PeakRoutine(PlayerCtrl player, SkillContext ctx)
    {
        player.PauseAnimation();
        player.SetRenderersEnabled(false);
        ctx.IsCharacterHidden = true;

        // ★ BaseSkillModule.EffectName 사용
        if (!string.IsNullOrEmpty(EffectName))
            EffectManager.Instance.PlayEffect(
                EffectName, EffectDuration,
                player.transform.position,
                player.transform.rotation);

        yield return new WaitForSeconds(EffectDuration);

        player.SetRenderersEnabled(true);
        ctx.IsCharacterHidden = false;
        player.ResumeAnimation();
    }

    public override void OnJumpLand(PlayerCtrl player, SkillContext ctx)
    {
        Collider[] hits = Physics.OverlapSphere(
            player.transform.position, 4f, LayerMask.GetMask("Monster"));
        float totalDmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

        if (hits.Length > 0)
            player.StartCoroutine(player.MultiHitRoutine(hits, HitCount, totalDmg));
    }
}