using UnityEngine;

public abstract class GremlinBase : MonoBehaviour
{
    protected string id;
    protected string gremlinName;
    protected Rarity currentTier;
    protected int currentLevel;

    [SerializeField] protected GremlinGrowthConfig growthConfig;

    protected float baseValue;
    protected float currentActionCycle;

    public virtual void Init(string id, string name, Rarity tier, int level, float baseValue)
    {
        this.id = id;
        this.gremlinName = name;
        this.currentTier = tier;
        this.currentLevel = level;
        this.baseValue = baseValue;

        currentActionCycle = growthConfig.GetTierData(tier).actionCycleTime;
    }

    public float GetFinalStat()
    {
        if(growthConfig == null)
        {
            Debug.LogError("growthConfig 연결 비어있습니다!");
            return 0;
        }

        var tierData = growthConfig.GetTierData(currentTier);
        if(tierData == null)
        {
            Debug.LogError("growthConfig 데이터에 뭔가 문제가 있습니다.");
            return 0;
        }

        float finalValue = (baseValue * tierData.tierMultiplier) + (currentLevel * tierData.levelBonus);
        return finalValue;
    }

    public void LevelUp()
    {
        currentLevel++;
    }

    protected abstract void PerformAction();
}
