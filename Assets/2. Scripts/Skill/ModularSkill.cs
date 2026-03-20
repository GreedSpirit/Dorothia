using System.Collections.Generic;
public class ModularSkill : BaseSkill
{
    private readonly List<ISkillModule> _modules = new List<ISkillModule>();
    public SkillContext Context { get; private set; }

    public ModularSkill AddModule(ISkillModule module)
    {
        module.ModuleIndex = _modules.Count;
        _modules.Add(module);
        return this;
    }

    public override void Execute(PlayerCtrl player)
    {
        Context = new SkillContext();
        StartCooldown();
        // TargetLock만 즉시 실행 (방향/타겟은 스킬 시작 시 필요)
        foreach (var m in _modules)
            if (m is TargetLockModule)
                m.OnExecute(player, Context);
    }

    private ISkillModule GetModule(int moduleIndex)
    {
        return _modules.Find(m => m.ModuleIndex == moduleIndex);
    }

    // ─── 애니 이벤트 수신 ───────────────────────────────
    // encoded = moduleIndex * 100 + hitIndex
    // ex) module 2, hit 0 → 200 / module 3, hit 1 → 301
    public void NotifyHit(PlayerCtrl player, int encoded)
    {
        int moduleIndex = encoded / 100;
        int hitIndex = encoded % 100;


        GetModule(moduleIndex)?.OnHit(player, Context, hitIndex);
    }

    public void NotifyExecute(PlayerCtrl player, int moduleIndex)
        => GetModule(moduleIndex)?.OnExecute(player, Context);

    public void NotifyDash(PlayerCtrl player, int moduleIndex)
        => GetModule(moduleIndex)?.OnDash(player, Context);

    public void NotifyTeleport(PlayerCtrl player, int moduleIndex)
    {
        GetModule(moduleIndex)?.OnTeleport(player, Context);
    }

    public void NotifyHide(PlayerCtrl player, int moduleIndex)
        => GetModule(moduleIndex)?.OnHide(player, Context);

    public void NotifyAppear(PlayerCtrl player, int moduleIndex)
        => GetModule(moduleIndex)?.OnAppear(player, Context);
}