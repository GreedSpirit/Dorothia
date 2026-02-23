using System.Threading;
using UnityEngine;

public class StrikerGremlin : GremlinBase
{
    [SerializeField] private float _attackRange = 10f;
    [SerializeField] private LayerMask _enemyLayer;
    private float _timer;
    //TODO 나중에 Player 클래스로 바꾸기
    private Transform _player;

    public override void Init(string id, string name, Rarity tier, int level, float baseValue, Transform player)
    {
        base.Init(id, name, tier, level, baseValue, player);

        // if(player != null) _player = player.GetComponent<Player>();
    }
    protected override void PerformAction()
    {
        _timer += Time.deltaTime;
        if(_timer >= currentActionCycle)
        {
            Transform targetEnemy = FindTarget();

            if(targetEnemy != null)
            {
                ExecuteAttack(targetEnemy);
                _timer = 0f;
            }
        }
    }

    private Transform FindTarget()
    {
        // 플레이어가 타겟중인 적
        if(_player != null) // 플레이어가 현재 타겟하는 적이 있는지까지 같이 확인
        {
            //플레이어의 타겟이 사거리 내에 있는지 확인 후 
            return _player; //_player.currenttarget
        }

        // 플레이어와 가장 가까운 적
        Collider[] hitColliders = Physics.OverlapSphere(followTarget.position, _attackRange, _enemyLayer);
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            float distance = Vector3.Distance(followTarget.position, hitCollider.transform.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = hitCollider.transform;
            }
        }
        
        return closestEnemy;
    }

    private void ExecuteAttack(Transform targetEnemy)
    {
        float finalDamage = GetFinalStat();

        //TODO 투사체 발사, 즉발 데미지 처리, 이펙트 혹은 사운드 같은 기능 구현 여기다가!
        Debug.Log($"{gremlinName}이 {targetEnemy}에게 {finalDamage}를 입힙니다.");
    }


}
