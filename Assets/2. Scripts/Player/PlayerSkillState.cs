using UnityEngine;
public class PlayerSkillState : IPlayerState<PlayerCtrl>
{
    private BaseSkill _targetSkill;

    public void SetSkill(BaseSkill skill) => _targetSkill = skill;

    public void Enter(PlayerCtrl player)
    {
        if (_targetSkill == null) return;

        player.Anima.SetTrigger("Skill");
        player.Anima.SetInteger("Skill_Id", _targetSkill.Data.Job_Skill_Id);
    }

    public void Execute(PlayerCtrl player)
    {
        // 애니메이션이 종료되면 자동으로 Idle 또는 Auto로 복귀하는 로직
    }

    public void Exit(PlayerCtrl player) => _targetSkill = null;
}