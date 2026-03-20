using UnityEngine;

public class PlayerSkillState : IPlayerState<PlayerCtrl>
{
    private BaseSkill _targetSkill;
    private bool _skillAnimStarted = false;
    private bool _waitingForClip = false; // ★ 클립 로딩 대기 플래그

    public BaseSkill TargetSkill => _targetSkill;

    public void SetSkill(BaseSkill skill) => _targetSkill = skill;

    public void Enter(PlayerCtrl player)
    {
        if (_targetSkill == null)
        {
            player.ChangeState(player.IdleState);
            return;
        }

        _skillAnimStarted = false;
        _waitingForClip = true; // 클립 로딩 전까지 Execute 대기

        player.Anima.SetBool("IsSkill", true);
        player.ExecuteFullReset();
        player.Anima.ResetTrigger("Attack");

        // SkillData.Skill_Animation_Patch 로 클립 로드 후 교체
        string animAddr = _targetSkill.Data.Skill_Animation_Patch;

        if (string.IsNullOrEmpty(animAddr))
        {
            // 주소 없으면 클립 교체 없이 바로 트리거
            OnClipReady(player);
        }
        else
        {
            AddressableManager.Instance.LoadAsset<AnimationClip>(animAddr, clip =>
            {
                // 애니메이터 컨트롤러의 "DanceOfBlade" 슬롯에 교체
                player.OverrideController["DanceOfBlade"] = clip;
                OnClipReady(player);
            });
        }
        player.IsInvincible = true;
    }

    // 클립 교체 완료 후 공통 실행
    private void OnClipReady(PlayerCtrl player)
    {
        _waitingForClip = false;

        // 스킬 트리거 발동
        player.Anima.SetTrigger("Skill");

        // 스킬 게임 로직 실행 (타격/이펙트는 애니 이벤트로)
        _targetSkill.Execute(player);
    }

    public void Execute(PlayerCtrl player)
    {
        // 클립 로딩 중 또는 D스킬 정점 대기 중 스킵
        if (_waitingForClip || player.Anima.speed == 0f) return;

        var stateInfo = player.Anima.GetCurrentAnimatorStateInfo(0);
        bool isTransitioning = player.Anima.IsInTransition(0);

        if (!_skillAnimStarted)
        {
            if (stateInfo.IsTag("Skill")) _skillAnimStarted = true;
            else return;
        }

        bool skillFinished = stateInfo.IsTag("Skill") &&
                             stateInfo.normalizedTime >= 0.95f &&
                             !isTransitioning;

        bool leftSkillAnim = _skillAnimStarted &&
                             !stateInfo.IsTag("Skill") &&
                             !isTransitioning;

        if (skillFinished || leftSkillAnim)
            player.ChangeState(player.IdleState);
    }

    public void Exit(PlayerCtrl player)
    {
        player.IsInvincible = false;
        player.Anima.speed = 1f;
        player.Anima.SetBool("IsSkill", false);
        player.Anima.ResetTrigger("Skill");
        _targetSkill = null;
        _skillAnimStarted = false;
        _waitingForClip = false;

        player.SetRenderersEnabled(true);
    }
}