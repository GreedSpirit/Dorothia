using UnityEngine;

public class PlayerAutoState : IPlayerState<PlayerCtrl>
{
    IMonster _target;
    float _chaseRange = 5f;
    float _timer = 0f;
    float _resetTimer = 1f;

    public void Enter(PlayerCtrl player)
    {
        Debug.Log("자동상태진입");
        //설정 초기화
        _timer = 0f;
        _resetTimer = 1f;
        _target = null;
        player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", false);
        player.Anima.SetBool("Attack", false);
        player.Anima.SetInteger("Combo", 0);
        
    }

    public void Execute(PlayerCtrl player)
    {

        _timer -= Time.deltaTime;
            
        //타이머돌면
        if (_timer <= 0f)
        {
            //적찾기
            _target = player.FindEnemy();
            _timer = _resetTimer;
        }

        //타겟 없으면
        if (_target == null || !_target.IsAlive)
        {
            _target = null;
            //경로 초기화
            player.NavMesh.ResetPath();
            return;
        }

        //널체크 후 타겟과 플레이어 거리
        float targetDistance = Vector3.Distance(player.transform.position, _target.Transform.position);

        //추적범위 벗어나면 타겟해제 하고 리턴
        if (targetDistance > _chaseRange)
        {
            _target = null;
            player.NavMesh.ResetPath();
            return;
        }

        //공격범위안에 있으면 공격
        if (targetDistance <= player.AttackRange)
        {
            //Debug.Log("공격시작");
            player.NavMesh.ResetPath();

            //타겟방향으로 회전
            Vector3 dir = (_target.Transform.position - player.transform.position).normalized;
            dir.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(dir);

            //공격중이 아니라면
            if (player.IsAttack == false)
            {                
                player.Anima.SetBool("Run", false);
                player.Anima.SetBool("Attack", true);
                player.Anima.SetInteger("Combo", player.ComboIndex);
               
            }
            return;
        }

        //위상황들 다 통과하면 타겟으로 이동
        else
        {
            //타겟으로 이동
            player.NavMesh.SetDestination(_target.Transform.position);

            //Debug.Log("달리기");
            player.Anima.SetBool("Run", true);
        }
    }

    public void Exit(PlayerCtrl player)
    {
        player.NavMesh.ResetPath();        
    }
}
