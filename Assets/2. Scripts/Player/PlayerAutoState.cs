using UnityEngine;
using UnityEngine.AI;

public class PlayerAutoState : IPlayerState<PlayerCtrl>
{
    NavMeshAgent _navMesh;
    TestEnemy _target;
    float _timer = 0f;
    float _resetTimer = 1f;
    bool _isAttack = false;

    public void Enter(PlayerCtrl player)
    {
        //설정 초기화
        _timer = 0f;
        _resetTimer = 1f;
        _target = null;
        _navMesh = player.NavMesh;
        _navMesh.ResetPath();
    }

    public void Execute(PlayerCtrl player)
    {

        _timer -= Time.deltaTime;
            
        //타이머돌면
        if (_timer <= 0f)
        {
            //적찾기
            _target = FindEnemy(player);
            _timer = _resetTimer;
        }

        //타겟 없으면
        if (_target == null)
        {
            //경로 초기화
            _navMesh.ResetPath();
            return;
        }

        //널체크 후 타겟과 플레이어 거리
        float targetDistance = Vector3.Distance(player.transform.position, _target.transform.position);

        //공격범위안에 있으면 공격
        if (targetDistance <= player.AttackRange)
        {
            _navMesh.ResetPath();

            //타겟방향으로 회전
            Vector3 dir = (_target.transform.position - player.transform.position).normalized;
            player.transform.rotation = Quaternion.LookRotation(dir);


            player.Anima.SetTrigger("Attack");
            //히트박스콜라이더 온
        }

        //위상황들 다 통과하면 타겟찾기
        else
        {
            //타겟으로 이동
            _navMesh.SetDestination(_target.transform.position);

            player.Anima.SetBool("Run", true);
        }
    }

    public void Exit(PlayerCtrl player)
    {
        _navMesh.ResetPath();
    }

    


    
    private TestEnemy FindEnemy(PlayerCtrl player)
    {
        //탐지범위안에 있는 콜라이더 가져오기
        Collider[] colliders = Physics.OverlapSphere(player.transform.position, player.EnemyFindRange);

        //초기값셋팅
        TestEnemy nearest = null;
        float minDistance = player.EnemyFindRange;

        //탐지된 콜라이더에서 enemy컴포넌트확인하고
        foreach (Collider col in colliders)
        {
            TestEnemy enemy = col.GetComponent<TestEnemy>();

            //적이 존재하고 살아있으면
            if (enemy != null && !enemy.isdead)
            {
                //플레이어 적 사이 거리 계산
                float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

                //저장되있던 최소거리보다 가까우면
                if (distance < minDistance)
                {
                    //최소거리 갱신하고
                    minDistance = distance;
                    //가까운적 갱신
                    nearest = enemy;
                }
            }
        }
        //가까운적 리턴
        //없으면 널리턴
        return nearest;
    }
}
