using UnityEngine;

public class PlayerIdleState : IPlayerState<PlayerCtrl>
{
    TestEnemy _target;
    public void Enter(PlayerCtrl player)
    {
        Debug.Log("아이들상태진입");
        player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", false);
        player.Anima.SetBool("Attack", false);
        player.ComboIndex = 0;
    }

    public void Execute(PlayerCtrl player)
    {
        //적 탐지
        _target = FindEnemy(player);

        //타겟없으면 리턴
        if (_target == null)
        {
            //공격중일수도있으니깐 꺼주기
            player.Anima.SetBool("Attack", false);
            return;
        }

        //타겟과 플레이어 거리
        float targetDistance = Vector3.Distance(player.transform.position, _target.transform.position);

        //공격범위안에 있으면 공격
        if (targetDistance <= player.AttackRange)
        {
            //타겟 방향으로 회전
            Vector3 dir = (_target.transform.position - player.transform.position).normalized;
            dir.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(dir);

            //공격
            player.Anima.SetBool("Attack", true);
            player.Anima.SetInteger("Combo", player.ComboIndex);
        }



    }

    public void Exit(PlayerCtrl player)
    {
        player.ComboIndex = 0;
    }




    //TestEnemy 임시클래스명
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
