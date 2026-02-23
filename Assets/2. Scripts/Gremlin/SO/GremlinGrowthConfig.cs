using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GremlinGrowthConfig", menuName = "Data/GremlinGrowthConfig")]
public class GremlinGrowthConfig : ScriptableObject
{
    public List<TierData> tierSettings;

    [System.Serializable]
    public class TierData
    {
        public Rarity tier;
        public float tierMultiplier;
        public float levelBonus;
        public float actionCycleTime;
    }

    // 등급에 맞는 데이터 가져오기
    public TierData GetTierData(Rarity tier)
    {
        int index = (int)tier;
        if (index >= 1 && index < tierSettings.Count)
        {
            return tierSettings[index];
        }
        return null;
    }
}
