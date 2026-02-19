using System.Threading;
using UnityEngine;

public class StrikerGremlin : GremlinBase
{
    private float _timer;
    protected override void PerformAction()
    {
        _timer += Time.deltaTime;
        if(_timer >= currentActionCycle)
        {
            // 공격 로직 실행하면 됨
            _timer = 0f;
        }
    }
}
