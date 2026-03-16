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

            // 스킬 우선, 없으면 평타
            BaseSkill readySkill = SkillManager.Instance.GetReadySkill();
            if (readySkill != null) player.PerformSkill(readySkill);
            else player.ChangeState(player.AttackState);
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