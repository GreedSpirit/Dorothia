using System;
using UnityEngine;
using static GremlinGrowthConfig;

public class GremlinInstance : MonoBehaviour
{
    [SerializeField] private GremlinMovement _movement;            // 움직임 제어
    [SerializeField] private GremlinVisual _visual;                // 시각적 제어
    [SerializeField] private GremlinBehaviour _behaviour;          // 행동 제어

    public void Init(Gremlin gremlin, Transform player)
    {
        if(gremlin._gremlinData.Type == Gremlin_Type.공격형)
        {
            gameObject.AddComponent<StrikerGremlin>();
            StrikerGremlin striker = GetComponent<StrikerGremlin>();
            striker.Init(DataManager.Instance.GetData<Gremlin_StatusData>(gremlin._gremlinData.PetID),
                GremlinManager.Instance.PlayerTransform, gremlin._rarity);
            striker.finalAttack = (striker.attackDamage * DataManager.Instance.GetData<Gremlin_TierData>((int)gremlin._rarity).Gremlin_Tier_Multiplier)
                + (gremlin._currentLevel * DataManager.Instance.GetData<Gremlin_AtkerData>((int)gremlin._rarity).Gremlin_Level_Bonus);
            _behaviour = striker;
        }
        else if(gremlin._gremlinData.Type == Gremlin_Type.지원형)
        {
            gameObject.AddComponent<BufferGremlin>();
            BufferGremlin buffer = GetComponent<BufferGremlin>();
            buffer.Init(DataManager.Instance.GetData<Gremlin_StatusData>(gremlin._gremlinData.PetID),
                GremlinManager.Instance.PlayerTransform, gremlin._rarity);
            buffer.finalValue = (buffer._buffValue * DataManager.Instance.GetData<Gremlin_TierData>((int)gremlin._rarity).Gremlin_Tier_Multiplier)
                + (gremlin._currentLevel * DataManager.Instance.GetData<Gremlin_BufferData>((int)gremlin._rarity).Gremlin_Level_Bonus);
        }
    }
}
