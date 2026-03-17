using UnityEngine;

public class PlayerAttackState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player)
    {
        player.IsAttack = true;
        player.ComboIndex = 1;
        player.Anima.SetInteger("Combo", 1);
        player.Anima.SetBool("Attack", true);

        // IsSkill이 false임을 명시적으로 보장
        player.Anima.SetBool("IsSkill", false);
        LookAtTarget(player);
    }

    public void Execute(PlayerCtrl player)
    {
        if (player.IsAutoMode)
        {
            BaseSkill readySkill = SkillManager.Instance.GetReadySkill();
            if (readySkill != null)
            {
                player.ExecuteFullReset();       // 공격 상태 초기화
                player.PerformSkill(readySkill); // 스킬 상태로 전환
                return;
            }
        }

        // 조이스틱을 아주 강하게 밀었을 때만 공격 캔슬
        if (player.MoveInput.sqrMagnitude > 0.2f)
        {
            // 애니메이터의 Combo 파라미터와 IsAttack을 확실히 0/false로 밀어버림
            player.ExecuteFullReset();
            player.ChangeState(player.MoveState);
            return;
        }

        // 공격 중 타겟 바라보기 유지
        if (player.CurrentTarget != null)
        {
            Vector3 dis = player.CurrentTarget.Transform.position - player.transform.position;
            dis.y = 0;

            if (dis.sqrMagnitude > player.AttackRange * player.AttackRange)
            {
                player.ResetCombo();
                return;
            }
            LookAtTarget(player, true);
        }
    }

    public void Exit(PlayerCtrl player)
    {
        // Exit에서는 최소한의 정리만 수행 (실제 정리는 ResetCombo가 담당)
        player.DisableAllAttackColliders();
    }

    private void LookAtTarget(PlayerCtrl player, bool isSmooth = false)
    {
        IMonster target = player.CurrentTarget ?? player.FindEnemy();
        if (target == null) return;

        Vector3 dir = target.Transform.position - player.transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            if (isSmooth)
                player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, Time.deltaTime * 10f);
            else
                player.transform.rotation = targetRot;
        }
    }
}