using UnityEngine;

public class BufferGremlin : GremlinBase
{
    private float _timer;  
    //TODO 나중에 플레이어로 바꿔야 함
    private Transform _player;

    public override void Init(string id, string name, Rarity tier, int level, float baseValue, Transform player)
    {
        base.Init(id, name, tier, level, baseValue, player);

        // if(player != null) _player = player.GetComponent<Player>();

        //TODO 초기화 되고 소환되면 바로 버프를 쓰도록 해두긴 했는데 악용할 수도 있으므로 추후 기획팀과 얘기
        _timer = currentActionCycle;
    }

    protected override void PerformAction()
    {
        _timer += Time.deltaTime;
        if(_timer >= currentActionCycle)
        {
            if(_player != null)
            {
                ApplyBuff();
                _timer = 0f;    
            }
        }
    }

    private void ApplyBuff()
    {
        float buffValue = GetFinalStat();

        //TODO 플레이어한테 버프 주기

        Debug.Log($"{gremlinName}이 플레이어에게 {buffValue}만큼의 버프 시전");
    }
}
