using UnityEngine;

public class PlayerAutoState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player) => player.FindEnemy();

    public void Execute(PlayerCtrl player)
    {
        player.FindEnemy();
        if (player.CurrentTarget == null || !player.CurrentTarget.IsAlive)
        {
            player.Anima.SetBool("Run", false);
            if (player.NavMesh.isOnNavMesh) player.NavMesh.ResetPath();
            return;
        }

        Vector3 targetPos = player.CurrentTarget.Transform.position;
        float distance = Vector3.Distance(player.transform.position, targetPos);

        if (distance <= player.AttackRange)
        {
            if (player.NavMesh.isOnNavMesh) player.NavMesh.ResetPath();
            LookAtTarget(player, targetPos);

            // 스킬 사용 가능 상태일 때만 Peek + Consume
            if (player.CurrentState != player.SkillState)
            {
                BaseSkill readySkill = SkillManager.Instance.PeekReadySkill();
                if (readySkill != null)
                {
                    SkillManager.Instance.ConsumeReadySkill(); // 사용 확정 후 인덱스 전진
                    player.PerformSkill(readySkill);
                }
                else
                {
                    player.ChangeState(player.AttackState);
                }
            }
        }
        else
        {
            player.Anima.SetBool("Run", true);
            if (player.NavMesh.isOnNavMesh)
            {
                player.NavMesh.SetDestination(targetPos);
                LookAtTarget(player, targetPos);
            }
        }
    }

    public void Exit(PlayerCtrl player)
    {
        if (player.NavMesh.isOnNavMesh) player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", false);
    }

    private void LookAtTarget(PlayerCtrl player, Vector3 targetPos)
    {
        Vector3 dir = targetPos - player.transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            player.transform.rotation = Quaternion.Slerp(
                player.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 12f);
        }
    }
}