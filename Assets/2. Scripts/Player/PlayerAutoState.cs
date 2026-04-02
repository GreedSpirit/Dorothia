using UnityEngine;

public class PlayerAutoState : IPlayerState<PlayerCtrl>
{
    private float _findInterval = 0.2f;
    private float _nextFindTime;

    public void Enter(PlayerCtrl player)
    {
        _nextFindTime = 0f; // 진입 시 즉시 탐색
        player.FindEnemy();
    }

    public void Execute(PlayerCtrl player)
    {
        if (Time.time >= _nextFindTime)
        {
            player.FindEnemy();
            _nextFindTime = Time.time + _findInterval;
        }

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

            if (player.IsSkillPending) return; // 스킬 중이면 대기
            if (player.Anima.IsInTransition(0)) return;

            BaseSkill readySkill = SkillManager.Instance.PeekReadySkill();
            if (readySkill != null)
            {
                SkillManager.Instance.ConsumeReadySkill();
                player.PerformSkill(readySkill); // SkillState로 전환
            }
            else
            {
                player.ChangeState(player.AttackState); // 스킬 없으면 일반 공격
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