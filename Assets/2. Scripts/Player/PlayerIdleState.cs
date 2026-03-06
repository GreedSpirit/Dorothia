using UnityEngine;

public class PlayerIdleState : IPlayerState<PlayerCtrl>
{
    IMonster _target;
    public void Enter(PlayerCtrl player)
    {
        Debug.Log("아이들상태진입");
        if (player.NavMesh != null &&
            player.NavMesh.isActiveAndEnabled &&
            player.NavMesh.isOnNavMesh)
        {
            player.NavMesh.ResetPath();
        }
        player.Anima.SetBool("Run", false);
        player.Anima.SetBool("Attack", false);
        player.ComboIndex = 0;
    }

    public void Execute(PlayerCtrl player)
    {
        //적 탐지
        _target = player.FindEnemy();

        //타겟없으면 리턴
        if (_target == null)
        {
            //공격중일수도있으니깐 꺼주기
            player.ComboIndex = 0;
            player.Anima.SetBool("Attack", false);
            player.Anima.SetInteger("Combo", 0);
            return;
        }

        //타겟과 플레이어 거리
        float targetDistance = Vector3.Distance(player.transform.position, _target.Transform.position);

        //공격범위안에 있으면 공격
        if (targetDistance <= player.AttackRange)
        {
            //타겟 방향으로 회전
            Vector3 dir = (_target.Transform.position - player.transform.position).normalized;
            dir.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(dir);

            //공격
            player.Anima.SetBool("Attack", true);
            player.Anima.SetInteger("Combo", player.ComboIndex);
        }



    }

    public void Exit(PlayerCtrl player)
    {
        
    }
    
}
