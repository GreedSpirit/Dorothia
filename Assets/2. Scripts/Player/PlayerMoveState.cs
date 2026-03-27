using UnityEngine;

public class PlayerMoveState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player)
    {
        player.Anima.SetBool("Run", true);
        player.Anima.SetBool("Attack", false);
    }

    public void Execute(PlayerCtrl player)
    {
        // 조이스틱 방향으로 벡터 생성
        Vector3 moveDir = new Vector3(player.MoveInput.x, 0, player.MoveInput.y);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDir);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, lookRotation, Time.deltaTime * 10f);

            float currentSpeed = Mathf.Min((float)StatManager.Instance.GetStat(Status.MoveSpeed), 3f);

            Vector3 movement = moveDir * currentSpeed * Time.deltaTime;

            player.NavMesh.Move(movement);
        }
    }

    public void Exit(PlayerCtrl player)
    {
        player.Anima.SetBool("Run", false);
        player.NavMesh.velocity = Vector3.zero;
    }
}