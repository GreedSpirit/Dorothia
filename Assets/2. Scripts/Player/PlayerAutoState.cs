using UnityEngine;

public class PlayerAutoState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player)
    {
        player.FindEnemy();
    }

    public void Execute(PlayerCtrl player)
    {
        player.FindEnemy();

        // 타겟 유효성 검사 (주변에 적이 아예 없는 경우)
        if (player.CurrentTarget == null || !player.CurrentTarget.IsAlive)
        {
            player.Anima.SetBool("Run", false);
            if (player.NavMesh.isOnNavMesh) player.NavMesh.ResetPath();
            return;
        }

        Vector3 targetPos = player.CurrentTarget.Transform.position;
        float distance = Vector3.Distance(player.transform.position, targetPos);

        // 사거리 확인
        if (distance <= player.AttackRange)
        {
            // 공격 사거리 안이라면 추적 중지
            if (player.NavMesh.isOnNavMesh) player.NavMesh.ResetPath();

            // 공격 방향 주시
            LookAtTarget(player, targetPos);

            // 스킬 우선 고려 후 평타
            BaseSkill readySkill = SkillManager.Instance.GetReadySkill();
            if (readySkill != null)
            {
                player.PerformSkill(readySkill);
            }
            else
            {
                player.ChangeState(player.AttackState);
            }
        }
        else
        {
            // 사거리 밖이면 추격
            player.Anima.SetBool("Run", true);

            if (player.NavMesh.isOnNavMesh)
            {
                // 실시간으로 갱신된 가장 가까운 타겟의 위치로 목적지 설정
                player.NavMesh.SetDestination(targetPos);

                // 이동 방향 회전
                LookAtTarget(player, targetPos);
            }
        }
    }

    private void LookAtTarget(PlayerCtrl player, Vector3 targetPos)
    {
        Vector3 dir = targetPos - player.transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, Time.deltaTime * 12f);
        }
    }

    public void Exit(PlayerCtrl player)
    {
        if (player.NavMesh.isOnNavMesh) player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", false);
    }
}