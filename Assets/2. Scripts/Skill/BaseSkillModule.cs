using UnityEngine;

public class SkillContext
{
    public IMonster LockedTarget;
    public Vector3 DashDirection;
    public Vector3 TeleportPosition;
    public bool IsCharacterHidden; // D스킬 소멸 상태
}

public interface ISkillModule
{
    void OnExecute(PlayerCtrl player, SkillContext ctx);
    void OnHit(PlayerCtrl player, SkillContext ctx); 
    void OnDash(PlayerCtrl player, SkillContext ctx);
    void OnEffect(PlayerCtrl player, SkillContext ctx);
    void OnJumpPeak(PlayerCtrl player, SkillContext ctx);
    void OnJumpLand(PlayerCtrl player, SkillContext ctx);
}

public abstract class BaseSkillModule : ISkillModule
{
    protected int HitCount { get; private set; } = 1;
    protected string EffectName { get; private set; } = string.Empty;
    protected float EffectDuration { get; private set; } = 0f;

    public void SetParamData(ModuleParamData p)
    {
        if (p == null) return;
        HitCount = Mathf.Max(1, p.Hit_Count);
        EffectName = p.Skill_Effect_Name ?? string.Empty;
        EffectDuration = p.Skill_Effect_Time; 
    }

    public virtual void OnEffect(PlayerCtrl player, SkillContext ctx)
    {
        if (string.IsNullOrEmpty(EffectName)) return;

        EffectManager.Instance.PlayEffect(
            EffectName,
            EffectDuration, 
            player.transform.position,
            player.transform.rotation);
    }

    public virtual void OnExecute(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnHit(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnDash(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnJumpPeak(PlayerCtrl player, SkillContext ctx) { }
    public virtual void OnJumpLand(PlayerCtrl player, SkillContext ctx) { }
}