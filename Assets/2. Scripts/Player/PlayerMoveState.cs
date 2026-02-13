using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player)
    {
        player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", true);
    }

    public void Execute(PlayerCtrl player)
    {
        //입력값 받아오기
        Vector2 input = player.MoveInput;

        //이동 방향 계산
        Vector3 moveDir = player.transform.forward * input.y + player.transform.right * input.x;

        //대각선이동시 1보다커짐
        if (moveDir.sqrMagnitude > 1f)
        {
            //길이 1로 수정
            moveDir.Normalize();
        }

        //플레이어 이동속도 계산
        float speed = player.PlayerStats.Speed;
        moveDir *= speed;

        //이동속도 적용
        player.transform.position += moveDir * Time.deltaTime;
    }

    public void Exit(PlayerCtrl player)
    {
        player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", false);
    }
}
