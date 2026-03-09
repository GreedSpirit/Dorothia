using System;
using UnityEngine;

public abstract class GremlinBase : MonoBehaviour
{
    protected int id;                 // 아이디값
    protected string gremlinName;        // 그렘린 이름
    protected Rarity currentTier;        // 현재등급
    protected int currentLevel;          // 현재레벨

    protected float baseValue;           //기본값?
    protected float currentActionCycle;  // 현재 동작의 사이클 주기?

    [SerializeField] protected GremlinGrowthConfig growthConfig;        // ??
    [SerializeField] protected Transform visualModel;                   // 시각적 모델

    [SerializeField] protected Transform followTarget;                  // 추적할 대상
    [SerializeField] private Vector3 followOffset = new Vector3(1.5f, 2.0f, -1.0f);         // 추적할 때의 오프셋. 거리 조절용.
    [SerializeField] private float smoothTime = 0.3f;                   // 부드럽게 이동하기 위한 시간

    [SerializeField] private float floatStrength = 0.5f;                // 떠 있는 힘
    [SerializeField] private float floatSpeed = 2.0f;                   // 떠다니는 움직임 속도

    private Vector3 currentVelocity;                                    // 플레이어의 움직임을 따라가기 위한, 현재의 위치?
    private Vector3 visualModelStartLocalPos;                           // 플레이어와의 움직임과는 별개로 공중에 떠 있는 애니메이션을 위한 VisualModel.


    //그렘린을 갈아끼거나 할 때 부를 초기화 함수
    public virtual void Init(int id, Rarity tier, int level, float baseValue, Transform player)
    {
        this.id = id;
        this.gremlinName = DataManager.Instance.GetData<GremlinData>(id).Gremlin_Name;
        this.currentTier = tier;
        this.currentLevel = level;
        this.baseValue = baseValue;
        this.followTarget = player;

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
        //추적할 대상이 없는 경우 반환합니다.
        if (followTarget == null)
        {
            return;
        }
        //대상의 위치를 확인, 그 대상의 위치에 오프셋만큼을 추가하여 이동 위치를 잡습니다.
        Vector3 targetPosition = followTarget.TransformPoint(followOffset);

        //smoothDamp를 활용한, 부드러운 추적. 그리고 Slerp를 통한 부드러운 회전.
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.rotation, Time.deltaTime * 5f);
    }

    // 부유 모션 로직
    private void HandleFloating()
    {
        //시각적 모델이 없으면 반환합니다.
        if (visualModel == null)
        {
            return;
        }
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
