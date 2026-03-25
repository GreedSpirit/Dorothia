using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SkillContext
{
    public IMonster LockedTarget;
    public Vector3 DashDirection;
    public Vector3 TeleportPosition;
    public bool IsCharacterHidden;
    public Vector3 PreTeleportPosition;   // 텔레포트 직전 위치
    public Vector3 OverrideEffectPosition; // 이펙트 생성 위치 (0이면 기본값 사용)
}

public interface ISkillModule
{
    int ModuleIndex { get; set; }
    void OnExecute(PlayerCtrl player, SkillContext ctx);
    void OnHit(PlayerCtrl player, SkillContext ctx, int hitIndex);
    void OnDash(PlayerCtrl player, SkillContext ctx);
    void OnTeleport(PlayerCtrl player, SkillContext ctx);
    void OnHide(PlayerCtrl player, SkillContext ctx);
    void OnAppear(PlayerCtrl player, SkillContext ctx);
}

public abstract class BaseSkillModule : ISkillModule
{
    public int ModuleIndex { get; set; }

    protected HashSet<IMonster> HitTargets = new();

    protected string EffectName { get; private set; } = string.Empty;
    protected float EffectDuration { get; private set; } = 0f;
    protected int[] HitCounts { get; private set; } = { 1 };
    protected int RepeatCount { get; private set; } = 1;
    protected float[] RepeatIntervals { get; private set; } = { 0f };
    protected float[] AoeRadius { get; private set; } = { 0f };
    protected float BehindOffset { get; private set; } = 1.2f;
    protected float DashDistance { get; private set; } = 5f;
    protected float DashDuration { get; private set; } = 0.3f;
    protected string ProjectileName { get; private set; } = string.Empty;
    protected float ProjectileSpeed { get; private set; } = 15f;
    protected float CastRange { get; private set; } = 0f;
    protected float Delay { get; private set; } = 0f;
    protected Skill_Target TargetType { get; private set; } = Skill_Target.MySelf;
 
    protected int GetHitCount(int i) => HitCounts[Mathf.Clamp(i, 0, HitCounts.Length - 1)];
    protected int GetRepeatCount(int i) => RepeatCount;
    protected float GetRepeatInterval(int i) => RepeatIntervals[Mathf.Clamp(i, 0, RepeatIntervals.Length - 1)];
    protected float GetAoeRadius(int i) => AoeRadius[Mathf.Clamp(i, 0, AoeRadius.Length - 1)];
    public void SetParamData(ModuleParamData p)
    {
        if (p == null || p.Module_Param_Id == 0) return;
        EffectName = p.Skill_Effect_Name ?? null;
        EffectDuration = p.Skill_Effect_Time;
        HitCounts = p.Hit_Count_Array;
        RepeatCount = p.Repeat_Count > 0 ? p.Repeat_Count : 1;
        RepeatIntervals = p.Repeat_Interval;
        AoeRadius = p.Aoe_Radius;
        BehindOffset = p.Behind_Offset > 0 ? p.Behind_Offset : 1.2f;
        DashDistance = p.Dash_Distance > 0 ? p.Dash_Distance : 5f;
        DashDuration = p.Dash_Duration > 0 ? p.Dash_Duration : 0.3f;
        ProjectileName = p.Projectile_Name ?? string.Empty;
        ProjectileSpeed = p.Projectile_Speed > 0 ? p.Projectile_Speed : 15f;
        CastRange = p.SkillCast_Range;
        Delay = p.First_Delay > 0 ? p.First_Delay : 0f;
        TargetType = p.Skill_Target;
    }

    protected void PlayMyEffect(GameObject target, Transform parent = null)
    {
        if (string.IsNullOrEmpty(EffectName))
        {
            return;
        }

        EffectManager.Instance.PlayEffect(
            EffectName, EffectDuration,
            target.transform.position,
            target.transform.rotation,
            parent);
    }

    protected void PlayMyEffect(GameObject target, Quaternion rot = default)
    {
        if (string.IsNullOrEmpty(EffectName))
        {
            return;
        }

        EffectManager.Instance.PlayEffect(
            EffectName, EffectDuration,
            target.transform.position,
            rot);
    }


    protected void PlayMyEffect(PlayerCtrl player, Vector3 centerPoint = default)
    {
        if (string.IsNullOrEmpty(EffectName))
        {
            return;
        }

        //Debug.Log($"{EffectName},{ModuleIndex},{centerPoint}");

        Vector3 effectPoint = centerPoint == default ? player.transform.position : centerPoint;
        Quaternion targetRot =
                                centerPoint != Vector3.zero ?
                                Quaternion.LookRotation(centerPoint - player.transform.position)
                                : player.transform.rotation;

        EffectManager.Instance.PlayEffect(
            EffectName, EffectDuration,
            effectPoint,
            targetRot);
    }
    protected IMonster FindFarthestUnhit(PlayerCtrl player)
    {
        IMonster best = null;
        float maxDist = -1f;
        float range = CastRange > 0 ? CastRange : player.EnemyFindRange;
        Collider[] cols = Physics.OverlapSphere(
            player.transform.position, range, LayerMask.GetMask("Monster"));

        foreach (var col in cols)
        {
            if (!col.TryGetComponent<IMonster>(out var m)) continue;
            if (HitTargets.Contains(m)) continue;

            float dist = Vector3.Distance(player.transform.position, col.transform.position);
            if (dist > maxDist)
            {
                maxDist = dist;
                best = m;
            }
        }
        return best;
    }

    protected IEnumerator RepeatRoutine(PlayerCtrl player, SkillContext ctx, int startHitIndex,
        Action<PlayerCtrl, SkillContext, int> action)
    {
        for (int i = 0; i < RepeatCount; i++)
        {
            int currentHitIndex = startHitIndex + i;

            action?.Invoke(player, ctx, currentHitIndex);

            if (i < RepeatCount - 1)
            {
                float interval = GetRepeatInterval(currentHitIndex);
                //Debug.Log($"<color=#ff0000>{currentHitIndex},{interval}</color>");
                yield return new WaitForSeconds(interval);
            }
        }
    }
    protected IEnumerator DelayedHit(PlayerCtrl player, int hitIndex, float delay)
    {
        yield return new WaitForSeconds(delay);

        // 지연 시간 후 데이터 참조
        int count = GetHitCount(hitIndex);
        float radius = GetAoeRadius(hitIndex);
        float dmg = player.CalculateSkillDamage(player.SkillState.TargetSkill);

        Collider[] hits = Physics.OverlapSphere(
            player.transform.position, radius, LayerMask.GetMask("Monster"));

        if (hits.Length > 0)
            player.StartCoroutine(player.MultiHitRoutine(hits, count, dmg));
    }

    public virtual void OnExecute(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnHit(PlayerCtrl player, SkillContext ctx, int hitIndex) { }
    public virtual void OnDash(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnTeleport(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnHide(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnAppear(PlayerCtrl player, SkillContext ctx) { }
}
