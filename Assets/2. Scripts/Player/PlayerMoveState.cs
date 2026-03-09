using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMoveState : IPlayerState<PlayerCtrl>
{    
    public void Enter(PlayerCtrl player)
    {
        Debug.Log("무브상태진입");
        //이전 상태 초기화
        player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", true);
        player.Anima.SetBool("Attack", false);
        player.ComboIndex = 0;
    }

    public void Execute(PlayerCtrl player)
    {
        //입력값 받아오기
        Vector2 input = player.MoveInput;


        /// <summary>
        /// 입력값 있는 경우
        /// </summary>
        if (input.sqrMagnitude > 0.001f)
        {

            //카메라 기준 이동 회전값반영해야하니깐 forward right
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;

            //y축 0으로 고정
            camForward.y = 0f;
            camRight.y = 0f;

            //대각선이동 방향 보정
            camForward.Normalize();
            camRight.Normalize();

            //이동방향 계산
            Vector3 moveDir = camForward * input.y + camRight * input.x;

            //대각선 입력값 1보다 크면 보정
            if (moveDir.sqrMagnitude > 1f)
            {
                moveDir.Normalize();
            }

            //바라보는 방향으로 회전값 계산
            Quaternion targetRot = Quaternion.LookRotation(moveDir);

            //부드럽게회전
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRot, Time.deltaTime * 10f);

            //이동
            float speed = (float)StatManager.Instance.stats[Status.MoveSpeed].FinalValue;
            //player.transform.position += moveDir * speed * Time.deltaTime;
            player.NavMesh.Move(moveDir * speed * Time.deltaTime);
        }


        
    }

    public void Exit(PlayerCtrl player)
    {
        player.NavMesh.ResetPath();
    }


    
}
