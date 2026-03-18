// ═══════════════════════════════════════════════════════
// 1. TargetLockModule - 반복 시 매번 타겟 재탐색
// ═══════════════════════════════════════════════════════
using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.AI;

public class TargetLockModule : BaseSkillModule
{
    public override void OnExecute(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, ExecuteOnce));

    private void ExecuteOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        IMonster target = player.FindEnemy();
        if (target == null || !target.IsAlive)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        ctx.LockedTarget = target;
        //ctx.TeleportPosition = target.Transform.position;
        Vector3 dir = target.Transform.position - player.transform.position;
        dir.y = 0;
        ctx.DashDirection = dir.normalized;

        if (dir != Vector3.zero)
            player.transform.rotation = Quaternion.LookRotation(dir);
    }
}

// ═══════════════════════════════════════════════════════
// 2. MeleeModule
// ═══════════════════════════════════════════════════════
public class MeleeModule : BaseSkillModule
{
    public override void OnHit(PlayerCtrl player, SkillContext ctx, int hitIndex)
        => player.StartCoroutine(RepeatRoutine(player, ctx, hitIndex, HitOnce));

    private void HitOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        PlayMyEffect(player);

        if (ctx.LockedTarget == null || !ctx.LockedTarget.IsAlive) return;

        int count = GetHitCount(hitIndex);
        float dmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

        player.StartCoroutine(player.SingleHitRoutine(ctx.LockedTarget, count, dmg));
    }
}

// ═══════════════════════════════════════════════════════
// 3. MeleeAoeModule
// ═══════════════════════════════════════════════════════
public class MeleeAoeModule : BaseSkillModule
{
    public override void OnHit(PlayerCtrl player, SkillContext ctx, int hitIndex)
        => player.StartCoroutine(RepeatRoutine(player, ctx, hitIndex, HitOnce));

    private void HitOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        PlayMyEffect(player);

        int count = GetHitCount(hitIndex);
        float radius = GetAoeRadius(hitIndex);
        float dmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

        Collider[] hits = Physics.OverlapSphere(
            player.transform.position, radius, LayerMask.GetMask("Monster"));

        if (hits.Length > 0)
            player.StartCoroutine(player.MultiHitRoutine(hits, count, dmg));
    }
}

// ═══════════════════════════════════════════════════════
// 4. ProjectileModule
// ═══════════════════════════════════════════════════════
//public class ProjectileModule : BaseSkillModule
//{
//    public override void OnExecute(PlayerCtrl player, SkillContext ctx)
//        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, FireOnce));

//    private void FireOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
//    {
//        if (ctx.LockedTarget == null) return;

//        AddressableManager.Instance.LoadAsset<GameObject>(ProjectileName, prefab =>
//        {
//            var go = Object.Instantiate(
//                prefab,
//                player.transform.position + Vector3.up,
//                player.transform.rotation);

//            if (go.TryGetComponent<SkillProjectile>(out var projectile))
//                projectile.Init(ctx.LockedTarget, GetHitCount(0), ProjectileSpeed, player);
//        });
//    }
//}

// ═══════════════════════════════════════════════════════
// 5. TeleportModule
// ═══════════════════════════════════════════════════════
public class TeleportModule : BaseSkillModule
{
    public override void OnTeleport(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, TeleportOnce));

    private void TeleportOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        if (ctx.LockedTarget == null)
        {
            return;
        }

        Vector3 targetPos = ctx.LockedTarget.Transform.position;
        Vector3 teleportPos = targetPos - ctx.DashDirection * BehindOffset;

        if (NavMesh.SamplePosition(teleportPos, out var hit, 2f, NavMesh.AllAreas))
            player.NavMesh.Warp(hit.position);

        Vector3 lookDir = targetPos - player.transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            player.transform.rotation = Quaternion.LookRotation(lookDir);

        PlayMyEffect(player);
    }
}

// ═══════════════════════════════════════════════════════
// 6. DashModule
// ═══════════════════════════════════════════════════════
public class DashModule : BaseSkillModule
{
    public override void OnDash(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, DashOnce));

    private void DashOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
        => player.StartCoroutine(DashRoutine(player, ctx));

    private IEnumerator DashRoutine(PlayerCtrl player, SkillContext ctx)
    {
        // 1. 타겟 존재 여부 확인 후 중앙 위치 계산
        Vector3 centerPoint = player.transform.position; // 기본값은 플레이어 위치

        if (ctx.LockedTarget != null)
        {
            // 산술 평균 방식: (A + B) / 2
            centerPoint = (player.transform.position + ctx.LockedTarget.Transform.position) * 0.5f;

            // 또는 Lerp 방식 (위와 동일한 결과)
            // centerPoint = Vector3.Lerp(player.transform.position, ctx.LockedTarget.transform.position, 0.5f);
        }

        //todo : 1. 돌진 시, 힛박스를 켜서 스킬데미지를 넘겨주던가
        //todo : 2. 돌진 시, 돌진하는동안 피직스오버랩으로 적찾고 주변 적 데미지 주던가

        player.EnableAttackCollider();

        PlayMyEffect(player, centerPoint);

        float elapsed = 0f;
        Vector3 startPos = player.transform.position;
        Vector3 destination = startPos + ctx.DashDirection * DashDistance;

        if (NavMesh.SamplePosition(destination, out var hit, 3f, NavMesh.AllAreas))
            destination = hit.position;

        while (elapsed < DashDuration)
        {
            elapsed += Time.deltaTime;
            float easedT = 1f - Mathf.Pow(1f - elapsed / DashDuration, 3f);
            player.NavMesh.Warp(Vector3.Lerp(startPos, destination, easedT));
            yield return null;
        }

        player.NavMesh.Warp(destination);
        player.DisableAllAttackColliders();
    }
}

// ═══════════════════════════════════════════════════════
// 7. HideAppearModule
// ═══════════════════════════════════════════════════════
public class HideAppearModule : BaseSkillModule
{
    public override void OnHide(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, HideOnce));

    public override void OnAppear(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, AppearOnce));

    private void HideOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        PlayMyEffect(player);
        player.SetRenderersEnabled(false);
        player.PauseAnimation();
        ctx.IsCharacterHidden = true;
    }

    private void AppearOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        player.SetRenderersEnabled(true);
        player.ResumeAnimation();
        ctx.IsCharacterHidden = false;
        PlayMyEffect(player);
    }
}

// ═══════════════════════════════════════════════════════
// 8. EffectHitModule
// ═══════════════════════════════════════════════════════
public class EffectHitModule : BaseSkillModule
{
    public override void OnHit(PlayerCtrl player, SkillContext ctx, int hitIndex)
        => player.StartCoroutine(RepeatRoutine(player, ctx, hitIndex, HitOnce));

    private void HitOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        PlayMyEffect(player);

        int count = GetHitCount(hitIndex);
        float radius = GetAoeRadius(hitIndex);
        float dmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

        Collider[] hits = Physics.OverlapSphere(
            player.transform.position,
            radius > 0 ? radius : 1f,
            LayerMask.GetMask("Monster"));

        if (hits.Length > 0)
            player.StartCoroutine(player.MultiHitRoutine(hits, count, dmg));
    }
}