using System.Collections.Generic;

public class ModularSkill : BaseSkill
{
    private readonly List<ISkillModule> _modules = new List<ISkillModule>();
    public SkillContext Context { get; private set; }

    public ModularSkill AddModule(ISkillModule module)
    {
        _modules.Add(module);
        return this;
    }

    public override void Execute(PlayerCtrl player)
    {
        Context = new SkillContext();
        StartCooldown();
        foreach (var m in _modules) m.OnExecute(player, Context);
    }

    // ★ OnHit 파라미터 제거
    public void NotifyHit(PlayerCtrl player) => _modules.ForEach(m => m.OnHit(player, Context));
    public void NotifyDash(PlayerCtrl player) => _modules.ForEach(m => m.OnDash(player, Context));
    public void NotifyEffect(PlayerCtrl player) => _modules.ForEach(m => m.OnEffect(player, Context));
    public void NotifyJumpPeak(PlayerCtrl player) => _modules.ForEach(m => m.OnJumpPeak(player, Context));
    public void NotifyJumpLand(PlayerCtrl player) => _modules.ForEach(m => m.OnJumpLand(player, Context));
}