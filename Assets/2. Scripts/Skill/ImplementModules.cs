// ═══════════════════════════════════════════════════════
// 1. TargetLockModule - 반복 시 매번 타겟 재탐색
// ═══════════════════════════════════════════════════════
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    {
        //대지의분노, 난무, 폭풍난무, 제노사이드, 히트쉐이크
        if (TargetType == Skill_Target.MySelf)
        {
            PlayMyEffect(player);
        }
        //탈론스크래치
        else
        {
            PlayMyEffect(ctx.LockedTarget.Transform.gameObject, player.transform.rotation);
        }

        if (RepeatCount > 0)
        {
            player.StartCoroutine(RepeatRoutine(player, ctx, hitIndex, HitOnce));
        }
        else
        {
            HitOnce(player, ctx, hitIndex);
        }
    }

    private void HitOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        //if (hitIndex == 0 && RepeatCount > 0)
        //{
        //    //player.SkillState.TargetSkill.Data.Skill_Type
        //    PlayMyEffect(ctx.LockedTarget.Transform.gameObject);
        //}

        //delay>0 아래코드 실행
        //선딜있을경우 사용
        //if(Delay> 0){ player.StartCombo(DelayedHit(player,hitIndex,Delay)}

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
public class ProjectileModule : BaseSkillModule
{
    public override void OnExecute(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, FireOnce));

    private void FireOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        if (ctx.LockedTarget == null) return;

        AddressableManager.Instance.LoadAsset<GameObject>(ProjectileName, prefab =>
        {
            var go = Object.Instantiate(
                prefab,
                player.transform.position,
                player.transform.rotation);

            if (go.TryGetComponent<SkillProjectile>(out var projectile))
                projectile.Init(ctx.LockedTarget, GetHitCount(0), ProjectileSpeed, player);
        });
    }
}

// ═══════════════════════════════════════════════════════
// 5. TeleportModule
// ═══════════════════════════════════════════════════════
public class TeleportModule : BaseSkillModule
{
    // 이번 스킬 사이클에서 순환할 대상 목록
    private List<IMonster> _cycleTargets = new();
    private int _cycleIndex = 0;

    public override void OnExecute(PlayerCtrl player, SkillContext ctx)
    {
        // 스킬 시작 시 순환 목록 초기화
        _cycleTargets.Clear();
        _cycleIndex = 0;
        HitTargets.Clear();
    }

    public override void OnTeleport(PlayerCtrl player, SkillContext ctx)
        => player.StartCoroutine(RepeatRoutine(player, ctx, 0, TeleportOnce));

    private void TeleportOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        // 순보베기, 암살자의 발걸음, 필살, 크로스 슬래시

        IMonster target = GetNextTarget(player, ctx);
        if (target == null) return;

        ctx.LockedTarget = target;

        // 이동
        Vector3 targetPos = target.Transform.position;
        Vector3 teleportPos = targetPos - ctx.DashDirection * BehindOffset;

        // 워프 직전 위치 저장
        ctx.PreTeleportPosition = player.transform.position;

        // 중간점을 이펙트 위치로 미리 계산해 저장
        ctx.OverrideEffectPosition = (ctx.PreTeleportPosition + targetPos) * 0.5f;

        if (NavMesh.SamplePosition(teleportPos, out var navHit, 2f, NavMesh.AllAreas))
            player.NavMesh.Warp(navHit.position);

        Vector3 lookDir = targetPos - player.transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            player.transform.rotation = Quaternion.LookRotation(lookDir);

        PlayMyEffect(player);

        // 피해
        float radius = GetAoeRadius(hitIndex);
        if (radius > 0)
        {
            int count = GetHitCount(hitIndex);
            float dmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);
            Collider[] hits = Physics.OverlapSphere(
                player.transform.position, radius, LayerMask.GetMask("Monster"));

            if (hits.Length > 0)
                player.StartCoroutine(player.MultiHitRoutine(hits, count, dmg));
        }

        // 맞은 대상 기록 (순환 풀이 3명 이하이면 기록 안 해서 재사용 허용)
        if (_cycleTargets.Count > 3)
            HitTargets.Add(target);
    }

    // ── 다음 대상 선택 ────────────────────────────────────────────────

    private IMonster GetNextTarget(PlayerCtrl player, SkillContext ctx)
    {
        RefreshCycleTargets(player);
        if (_cycleTargets.Count == 0) return null;

        // 3마리 이하 → 순환 (직전에 때린 대상은 건너뜀)
        if (_cycleTargets.Count <= 3)
        {
            // 순환 목록을 한 바퀴 돌며 직전 대상이 아닌 첫 번째 선택
            for (int i = 0; i < _cycleTargets.Count; i++)
            {
                IMonster candidate = _cycleTargets[_cycleIndex % _cycleTargets.Count];
                _cycleIndex++;

                // 단 1마리뿐이면 같은 대상을 허용
                if (candidate != ctx.LockedTarget || _cycleTargets.Count == 1)
                    return candidate;
            }
            // 전부 같은 대상인 경우(방어) → 그냥 반환
            return _cycleTargets[_cycleIndex % _cycleTargets.Count];
        }

        // 3마리 초과 → 맞지 않은 가장 먼 적
        return FindFarthestUnhit(player);
    }

    private void RefreshCycleTargets(PlayerCtrl player)
    {
        //if (_cycleTargets.Count > 0) return; // 이미 수집됨
        Debug.Log(CastRange > 0 ? CastRange : player.EnemyFindRange);

        Collider[] cols = Physics.OverlapSphere(
            player.transform.position, CastRange > 0 ? CastRange : player.EnemyFindRange,
            LayerMask.GetMask("Monster"));


        foreach (var col in cols)
        {
            if (col.TryGetComponent<IMonster>(out var m))
            {
                if (!HitTargets.Contains(m))
                {
                    _cycleTargets.Add(m);
                }
            }
        }
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
        => player.StartCoroutine(DashRoutine(player, ctx, hitIndex));

    private IEnumerator DashRoutine(PlayerCtrl player, SkillContext ctx, int hitIndex)
    {
        //
        // 타겟 존재 여부 확인 후 중앙 위치 계산
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


        float elapsed = 0f;
        Vector3 startPos = player.transform.position;
        Vector3 destination = startPos + ctx.DashDirection * DashDistance;

        float radius = GetAoeRadius(hitIndex);
        int hitCount = GetHitCount(hitIndex);

        if (NavMesh.SamplePosition(destination, out var hit, 3f, NavMesh.AllAreas))
            destination = hit.position;

        //PlayMyEffect(player, centerPoint);
        PlayMyEffect(player.gameObject, player.transform);

        while (elapsed < DashDuration)
        {
            elapsed += Time.deltaTime;
            float easedT = 1f - Mathf.Pow(1f - elapsed / DashDuration, 3f);
            player.NavMesh.Warp(Vector3.Lerp(startPos, destination, easedT));

            HitEnemiesInRadius(player, radius, HitTargets);
            yield return null;
        }

        player.NavMesh.Warp(destination);

        // 공격용 돌진기라면
        if (hitCount > 0)
        {
            float dmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

            if (HitTargets.Count > 0)
                player.StartCoroutine(player.MultiHitRoutine(HitTargets, hitCount, dmg));

            player.DisableAllAttackColliders();
        }
    }
}

// ═══════════════════════════════════════════════════════
// 7. HideAppearModule
// ═══════════════════════════════════════════════════════
public class HideAppearModule : BaseSkillModule
{

    public override void OnHide(PlayerCtrl player, SkillContext ctx)
    {
        PlayMyEffect(player);
        player.SetRenderersEnabled(false);
        //player.PauseAnimation();
        ctx.IsCharacterHidden = true;
    }
    public override void OnAppear(PlayerCtrl player, SkillContext ctx)
    {
        player.SetRenderersEnabled(true);
        //player.ResumeAnimation();
        ctx.IsCharacterHidden = false;
        PlayMyEffect(player);
    }
    //public override void OnHide(PlayerCtrl player, SkillContext ctx)
    //    => player.StartCoroutine(RepeatRoutine(player, ctx, 0, HideOnce));
    //public override void OnAppear(PlayerCtrl player, SkillContext ctx)
    //    => player.StartCoroutine(RepeatRoutine(player, ctx, 0, AppearOnce));

    //private void HideOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    //{
    //    PlayMyEffect(player);
    //    player.SetRenderersEnabled(false);
    //    player.PauseAnimation();
    //    ctx.IsCharacterHidden = true;
    //}

    //private void AppearOnce(PlayerCtrl player, SkillContext ctx, int hitIndex)
    //{
    //    player.SetRenderersEnabled(true);
    //    player.ResumeAnimation();
    //    ctx.IsCharacterHidden = false;
    //    PlayMyEffect(player);
    //}
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
        // 전 모듈 순서가 텔레포트->힛이펙트라면 무조건 아래 실행됨(주의)
        if (ctx.OverrideEffectPosition != Vector3.zero)
        {
            PlayMyEffect(player, ctx.OverrideEffectPosition);
        }
        else
        {
            PlayMyEffect(player);
        }

        int count = GetHitCount(hitIndex);
        //count==0이면 effect만 실행
        if (count == 0) return;

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