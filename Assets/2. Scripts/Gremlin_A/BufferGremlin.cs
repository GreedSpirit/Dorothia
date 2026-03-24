using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferGremlin : GremlinBehaviour
{
    public float _buffCooltime;     // 버프의 쿨타임
    public float _buffDuration;     // 버프의 지속시간
    private Rarity _rarity;         // 등급
    public Dictionary<Status, float> ActiveStatus {  get; private set; }
    public Dictionary<Status, float> PassiveStatus {  get; private set; }

    public Action onActing;

    private float _timer;
    [SerializeField]private PlayerCtrl _player;

    public float finalValue { get; set; }

    private void Awake()
    {
        //플레이어 찾기
        _player = FindAnyObjectByType<PlayerCtrl>();
        onActing += ApplyBuff;
    }

    private void Update()
    {
        if(_buffCooltime > 0)
        {
            Tick();
        }
    }
    public void Init(List<Gremlin_StatusData> data, Transform transform, Rarity rarity)
    {
        ActiveStatus = new Dictionary<Status, float>();
        PassiveStatus = new Dictionary<Status, float>();

        foreach(var statusdata in data)
        {
            if(statusdata.Effect_Type == Effect_Type.Active)
            {
                ActiveStatus.Add(statusdata.Gremlin_Buff, statusdata.Buff_Value);
                _buffCooltime = statusdata.Gremlin_Cooltime;
                Debug.Log($"{statusdata.Gremlin_Buff}를 {statusdata.Buff_Value}만큼 건드는 액티브 추가");
            }
            else if(statusdata.Effect_Type==Effect_Type.Passive)
            {
                PassiveStatus.Add(statusdata.Gremlin_Buff, statusdata.Buff_Value);
                Debug.Log($"{statusdata.Gremlin_Buff}를 {statusdata.Buff_Value}만큼 건드는 패시브 추가");
            }
        }
        _rarity = rarity;
        StatManager.Instance.RefreshStats();
    }

    public override void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer >= _buffCooltime)
        {
            if (_player != null)
            {
                OnTick?.Invoke();
                onActing?.Invoke();
                _timer = 0f;
            }
        }
    }

    private void ApplyBuff()
    {
        //현재로서는 체력 회복만. 액티브 스킬의 버프로 스탯 상승은 구현되지 않았으며, 확장 가능성이 매우 낮다고 함.
        if(ActiveStatus.TryGetValue(Status.HP, out float value))
        {
            int healamount = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.HP) * value);
            int maxHealAmount = Mathf.FloorToInt(_player.PlayerStats._maxHp - _player.PlayerStats._currentHp);

            if(healamount > maxHealAmount)
            {
                _player.ApplyDamage(-maxHealAmount);
            }
            else
            {
                _player.ApplyDamage(-healamount);
            }
        }
    }
}
