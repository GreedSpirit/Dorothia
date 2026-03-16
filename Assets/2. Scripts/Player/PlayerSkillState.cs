using UnityEngine;

public class PlayerSkillState : IPlayerState<PlayerCtrl>
{
    private BaseSkill _targetSkill;
    public BaseSkill TargetSkill => _targetSkill;

    private bool _skillAnimStarted = false; // 스킬 애니가 실제로 시작됐는지 추적

    public void SetSkill(BaseSkill skill) => _targetSkill = skill;

    public void Enter(PlayerCtrl player)
    {
        if (_targetSkill == null)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        _skillAnimStarted = false;

        // 트랜지션 차단
        player.Anima.SetBool("IsSkill", true);

        // 기존 공격 상태 완전 정리
        player.ExecuteFullReset();
        player.Anima.ResetTrigger("Attack");

        //player.Anima.SetBool("Attack", false);
        //player.Anima.SetInteger("Combo", 0);
        //player.Anima.ResetTrigger("Attack");
        //player.IsAttack = false;
        //player.DisableAllAttackColliders();

        // 스킬 트리거 발동
        player.Anima.SetTrigger("Skill");

        // 스킬 게임로직 실행 (데미지/이펙트 타이밍은 애니 이벤트로)
        _targetSkill.Execute(player);
    }

    public void Execute(PlayerCtrl player)
    {
        var stateInfo = player.Anima.GetCurrentAnimatorStateInfo(0);
        bool isTransitioning = player.Anima.IsInTransition(0);

        // 스킬 애니 레이어에 진입했는지 첫 프레임 확인
        if (!_skillAnimStarted)
        {
            if (stateInfo.IsTag("Skill"))
                _skillAnimStarted = true;
            else
                return; // 아직 트랜지션 진입 전 → 대기
        }

        // 스킬 애니가 끝나고 트랜지션도 완료됐을 때만 상태 전환
        bool skillFinished = stateInfo.IsTag("Skill") &&
                             stateInfo.normalizedTime >= 0.95f &&
                             !isTransitioning;

        // 혹은 이미 Skill 태그를 벗어난 경우 (Has Exit Time 으로 자연 전환된 경우)
        bool leftSkillAnim = _skillAnimStarted && !stateInfo.IsTag("Skill") && !isTransitioning;

        if (skillFinished || leftSkillAnim)
        {
            player.ChangeState(player.IdleState);
        }
    }

    public void Exit(PlayerCtrl player)
    {
        // 스킬 종료 시 IsSkill 해제 → Attack 트랜지션 다시 허용
        player.Anima.SetBool("IsSkill", false);
        _targetSkill = null;
        _skillAnimStarted = false;
    }
}