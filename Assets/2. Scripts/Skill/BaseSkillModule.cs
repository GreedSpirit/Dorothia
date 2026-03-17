using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SkillContext
{
    public IMonster LockedTarget;
    public Vector3 DashDirection;
    public Vector3 TeleportPosition;
    public bool IsCharacterHidden;
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

    protected int GetHitCount(int i) => HitCounts[Mathf.Clamp(i, 0, HitCounts.Length - 1)];
    protected int GetRepeatCount(int i) => RepeatCount;
    protected float GetRepeatInterval(int i) => RepeatIntervals[Mathf.Clamp(i, 0, RepeatIntervals.Length - 1)];
    protected float GetAoeRadius(int i) => AoeRadius[Mathf.Clamp(i, 0, AoeRadius.Length - 1)];
    public void SetParamData(ModuleParamData p)
    {
        if (p == null) return;
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
    }
    protected void PlayMyEffect(PlayerCtrl player, Vector3 centerPoint = default)
    {
        if (string.IsNullOrEmpty(EffectName)) {
            Debug.Log($"{EffectName}======================");
        return;
        }

        Debug.Log($"{EffectName},{ModuleIndex},{centerPoint}");

        Vector3 effectPoint = centerPoint == default ? player.transform.position : centerPoint;

        EffectManager.Instance.PlayEffect(
            EffectName, EffectDuration,
            effectPoint,
            player.transform.rotation);
    }
    protected IEnumerator RepeatRoutine(PlayerCtrl player, SkillContext ctx, int hitIndex,
        Action<PlayerCtrl, SkillContext, int> action)
    {
        for (int i = 0; i < RepeatCount; i++)
        {
            action(player, ctx, hitIndex);

            if (i < RepeatCount - 1)
                yield return new WaitForSeconds(GetRepeatInterval(hitIndex));
        }
    }

    public virtual void OnExecute(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnHit(PlayerCtrl player, SkillContext ctx, int hitIndex) { }
    public virtual void OnDash(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnTeleport(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnHide(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnAppear(PlayerCtrl player, SkillContext ctx) { }
}
