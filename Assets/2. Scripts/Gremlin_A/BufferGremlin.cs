using UnityEngine;

public class BufferGremlin : GremlinBehaviour
{
    public Status _buffStatus;      // 버프로 영향을 줄 스테이터스
    public float _buffCooltime;     // 버프의 쿨타임
    public float _buffDuration;     // 버프의 지속시간
    private Rarity _rarity;         // 등급

    public float _buffValue { get; private set; }       // 버프로 올라가는 스텟의 값

    private float _timer;
    private PlayerCtrl _player;

    public float finalValue { get; set; }

    public void Init(Gremlin_StatusData data, Transform transform, Rarity rarity)
    {
        _buffStatus = data.Gremlin_Buff;

        _rarity = rarity;
        _buffCooltime = data.Gremlin_Atk;

        _buffValue = 5f; // 임시값
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer >= _buffCooltime)
        {
            if (_player != null)
            {
                _timer = 0f;
                ApplyBuff();
            }
        }
    }

    private void ApplyBuff()
    {
        float buffValue = finalValue;

        //TODO 플레이어한테 버프 주기

        Debug.Log($"그렘린이 플레이어에게 {buffValue}만큼의 버프 시전");
    }
}
