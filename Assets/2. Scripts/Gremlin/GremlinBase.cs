using System;
using UnityEngine;

public abstract class GremlinBase : MonoBehaviour
{
    protected string id;
    protected string gremlinName;
    protected Rarity currentTier;
    protected int currentLevel;

    protected float baseValue;
    protected float currentActionCycle;

    [SerializeField] protected GremlinGrowthConfig growthConfig;
    [SerializeField] protected Transform visualModel;

    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 followOffset = new Vector3(1.5f, 2.0f, -1.0f);
    [SerializeField] private float smoothTime = 0.3f;

    [SerializeField] private float floatStrength = 0.5f;
    [SerializeField] private float floatSpeed = 2.0f;

    private Vector3 currentVelocity;
    private Vector3 visualModelStartLocalPos;

    //그렘린을 갈아끼거나 할 때 부를 초기화 함수
    public virtual void Init(string id, string name, Rarity tier, int level, float baseValue, Transform target)
    {
        this.id = id;
        this.gremlinName = name;
        this.currentTier = tier;
        this.currentLevel = level;
        this.baseValue = baseValue;
        this.followTarget = target;

        currentActionCycle = growthConfig.GetTierData(tier).actionCycleTime;
        visualModelStartLocalPos = visualModel.localPosition;
    }

    // 여기선 그렘린 공통 행동 구현
    protected virtual void Update()
    {
        HandleMovement();
        HandleFloating();
        PerformAction();
    }

    // 관성 있게 추적하는 기능
    private void HandleMovement()
    {
        if(followTarget == null) return;
        Vector3 targetPosition = followTarget.TransformPoint(followOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * 5f);
    }

    // 부유 모션 로직
    private void HandleFloating()
    {
        if(visualModel == null) return;
        // 기존 y값에 Sin을 이용해서 -1 ~ 1의 흐름을 time으로 만들고 빠르기는 speed로 부유 높낮이는 strength로 조절하는 아이디어
        float newY = visualModelStartLocalPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatStrength);

        visualModel.localPosition = new Vector3(visualModelStartLocalPos.x, newY, visualModelStartLocalPos.z);
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
