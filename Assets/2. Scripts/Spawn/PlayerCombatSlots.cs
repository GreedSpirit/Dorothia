using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 주변에 근접 몬스터가 
/// 접근할 위치 슬롯을 생성 및 관리하는 시스템
/// 
/// 업그레이드 Combat Slot 시스템
/// - Adaptive Slot: (슬롯 수 + 링 + 반경) 동적
/// - Slot Refresh: 목표 이동/혼잡 시 슬롯 리빌드/재할당
/// - Angle Bias: 플레이어 진행 방향(혹은 몬스터 접근 방향) 기반 가중치
/// </summary>
public class PlayerCombatSlots : MonoBehaviour
{
    [SerializeField] private Transform _targetPlayer;

    [Header("Base Slot Layout")]
    [SerializeField] private int _baseSlotCount = 12;               // 플레이어 주변 슬롯 수
    [SerializeField] private float _baseRadius = 1f;              // 플레이어 중심에서 슬롯까지 거리
    [SerializeField] private float _randomOffset = 0.35f;           // 슬롯 퍼짐 정도

    [Header("Adaptive Slot (Rings)")]
    [SerializeField] private bool _useAdaptive = true;
    [SerializeField] private int _maxRings = 3;                     //링 최대 개수(120마리 대비)
    [SerializeField] private float _ringSpacing = 0.9f;             //링 간 거리
    [SerializeField] private int _slotsPerRing = 12;                //링당 슬롯 수(기본 12)
    [SerializeField] private int _maxSlots = 48;                    //총 슬롯 최대(예: 12 * 4 = 48) / 근접 공격 가능 수 제한

    [Header("Angle Bias")]
    [SerializeField] private bool _useAngleBias = true;
    [SerializeField] private float _forwardBias = 0.65f;            //플레이어 전방 선호(0~1)
    [SerializeField] private float _sideBias = 0.20f;               //측면 선호
    [SerializeField] private float _backBias = 0.15f;               //후방 선호
    [SerializeField] private float _biasSharpness = 2.0f;           //가중치 곡선(클수록 특정 방향에 더 몰림)

    [Header("Slot Refresh")]
    [SerializeField] private float _rebuildInterval = 2.0f;        //목표 이동에 맞춰 슬롯 재배치 간격
    [SerializeField] private float _rebuildMoveThreshold = 1f;   //플레이어 이동량이 이 이상이면 재배치
    [SerializeField] private bool _rotateSlotsWithFacing = false;    //플레이어 진행방향에 따라 슬롯 원형을 회전

    // 내부 캐시/풀
    private struct SlotData
    {
        public Transform tr;
        public Vector3 baseLocalPos;      // 오프셋 전 기본 위치
        public Vector3 currentLocalPos;   // 랜덤 오프셋 포함 최종 위치
        public bool used;
        public int ring;
        public float angleRad;
    }

    private SlotData[] _slotData;

    //어떤 몬스터가 어떤 슬롯을 점유하는지
    private readonly Dictionary<IMonster, int> _occupied = new(128); // 최대 120 기준 pre-alloc

    private bool[] _used;

    //플레이어 이동/방향 기반 슬롯 재빌드
    private Vector3 _lastRebuildWorldPos;
    private float _nextRebuildTime;

    //랜덤 오프셋 고정(슬롯마다 시드) - "튀는" 랜덤 방지
    //같은 슬롯을 점유해도 매 프레임 랜덤이 바뀌면 부자연스러움
    private Vector2[] _slotOffsetSeed;

    private void Awake()
    {
        BuildSlots(initial: true); // 빌더 호출
    }

    private void LateUpdate()
    {
        //Slot Refresh (Rebuild)
        // - 플레이어가 일정 거리 이상 이동하면 슬롯 원형을 재정렬
        // - 너무 자주 하면 몬스터 목표가 계속 흔들림 -> interval로 제한
        if (Time.time < _nextRebuildTime)
            return;

        if (_targetPlayer != null)
            transform.position = _targetPlayer.position;

        float sqrMove = (transform.position - _lastRebuildWorldPos).sqrMagnitude;
        if (sqrMove >= _rebuildMoveThreshold * _rebuildMoveThreshold)
        {
            BuildSlots(initial: false);
            _nextRebuildTime = Time.time + _rebuildInterval;
        }
    }

    /// <summary>
    /// 슬롯 생성/재생성 (Adaptive + Angle Bias)
    /// </summary>
    /// <param name="initial"></param>
    private void BuildSlots(bool initial)
    {
        //플레이어 이동/회전 기반 오프셋을 재계산
        _lastRebuildWorldPos = transform.position;

        //총 슬롯 개수 결정 (Adaptive)
        int desiredSlots = _useAdaptive ? Mathf.Min(_maxSlots, _slotsPerRing * _maxRings) : _baseSlotCount;
        if (desiredSlots <= 0) desiredSlots = 1;

        //초기 1회: 배열/Transform 풀 구성
        if (_slotData == null || _slotData.Length != desiredSlots)
        {
            //기존 슬롯 오브젝트 정리(에디터/리런 대비)
            if (_slotData != null)
            {
                for (int i = 0; i < _slotData.Length; i++)
                {
                    if (_slotData[i].tr != null)
                        Destroy(_slotData[i].tr.gameObject);
                }
            }

            _slotData = new SlotData[desiredSlots];
            _used = new bool[desiredSlots];
            _slotOffsetSeed = new Vector2[desiredSlots];

            for (int i = 0; i < desiredSlots; i++)
            {
                var go = new GameObject($"Slot_{i}");
                go.transform.SetParent(transform, false);

                _slotData[i] = new SlotData
                {
                    tr = go.transform,
                    used = false,
                    ring = 0,
                    angleRad = 0f,
                    baseLocalPos = Vector3.zero,
                    currentLocalPos = Vector3.zero,
                };

                // 슬롯마다 고정 랜덤 시드(오프셋)
                _slotOffsetSeed[i] = Random.insideUnitCircle;
            }

            // 점유 정보도 초기화 (상황에 따라 외부에서 리셋/씬 시작)
            _occupied.Clear();
        }

        //점유 중인 몬스터가 있는 경우:
        //슬롯 위치만 재정렬(Transform 움직임) -> 몬스터는 destination 갱신 로직으로 자연스럽게 따라옴
        //used 플래그는 유지해야 함
        for (int i = 0; i < _used.Length; i++)
            _used[i] = _slotData[i].used; // 안전 동기화

        //플레이어 진행 방향(또는 이동 방향) 기반 회전 각
        float yawOffsetRad = 0f;

        //각 슬롯 위치 계산
        int total = _slotData.Length;

        if (!_useAdaptive)
        {
            //고정 슬롯(기존 방식 확장)
            for (int i = 0; i < total; i++)
            {
                float angleDeg = (360f / total) * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
                Vector3 basePos = dir * _baseRadius;

                Vector3 offset = ComputeStableOffset(i, dir, _randomOffset);

                _slotData[i].ring = 0;
                _slotData[i].angleRad = angleRad;
                _slotData[i].baseLocalPos = basePos;
                _slotData[i].currentLocalPos = basePos + offset;

                _slotData[i].tr.localPosition = _slotData[i].currentLocalPos;
            }
        }
        else
        {
            //Adaptive Rings
            //링 0: 가장 가까운 링 (선호)
            //링 1,2...: 외곽 링
            //각 링마다 _slotsPerRing 개 배치
            int idx = 0;
            for (int ring = 0; ring < _maxRings && idx < total; ring++)
            {
                float radius = _baseRadius + _ringSpacing * ring;
                int ringSlots = Mathf.Min(_slotsPerRing, total - idx);

                for (int j = 0; j < ringSlots && idx < total; j++, idx++)
                {
                    //링마다 슬롯 개수로 원형 배치
                    float t = (float)j / ringSlots;
                    float angleRad = (t * Mathf.PI * 2f) + yawOffsetRad;

                    Vector3 dir = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad));
                    Vector3 basePos = dir * radius;

                    Vector3 offset = ComputeStableOffset(idx, dir, _randomOffset * (1f + ring * 0.15f));

                    _slotData[idx].ring = ring;
                    _slotData[idx].angleRad = angleRad;
                    _slotData[idx].baseLocalPos = basePos;
                    _slotData[idx].currentLocalPos = basePos + offset;

                    _slotData[idx].tr.localPosition = _slotData[idx].currentLocalPos;
                }
            }
        }

        //used 상태 동기화
        for (int i = 0; i < _slotData.Length; i++)
            _slotData[i].used = _used[i];
    }

    /// <summary>
    /// "고정 랜덤 오프셋" 계산
    /// - 슬롯 점유/해제 때마다 랜덤이 튀지 않도록
    /// - dir 기반으로 살짝 뒤/옆으로 분산해 겹침 감소
    /// </summary>
    /// <param name="slotIndex"></param>
    /// <param name="dir"></param>
    /// <param name="magnitude"></param>
    /// <returns></returns>
    private Vector3 ComputeStableOffset(int slotIndex, Vector3 dir, float magnitude)
    {
        //슬롯별 고정 시드
        Vector2 seed = _slotOffsetSeed[slotIndex];
        Vector3 rand = new Vector3(seed.x, 0f, seed.y);

        //원형 바깥/안쪽으로 튀는 것보다 "접선 방향" 성분을 살려서 자연스러운 분산
        Vector3 tangent = new Vector3(-dir.z, 0f, dir.x);

        //rand를 접선/반지름 방향으로 혼합
        Vector3 offset = (tangent * rand.x + dir * (rand.y * 0.35f)) * magnitude;

        return offset;
    }

    /// <summary>
    /// MonsterController에서 호출하는 "최종 슬롯 획득/리프레시" API
    /// - Angle Bias 적용
    /// - 슬롯이 부족하면 null (그 경우 몬스터는 플레이어로 fallback)
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="forceRefresh"></param>
    /// <returns></returns>
    public Transform AcquireOrRefreshSlot(IMonster monster, bool forceRefresh)
    {
        //이미 점유 중이면 반환 (forceRefresh가 true면 더 좋은 슬롯으로 교체 가능)
        if (_occupied.TryGetValue(monster, out int currentIndex))
        {
            if (!forceRefresh)
                return _slotData[currentIndex].tr;

            //forceRefresh: 더 좋은 슬롯이 있으면 교체(자연스러운 재포지셔닝)
            int better = FindBestSlotIndex(monster, allowUsed: false);
            if (better != -1 && better != currentIndex)
            {
                //기존 슬롯 반납
                _used[currentIndex] = false;
                _slotData[currentIndex].used = false;

                //신규 슬롯 점유
                _occupied[monster] = better;
                _used[better] = true;
                _slotData[better].used = true;

                return _slotData[better].tr;
            }

            return _slotData[currentIndex].tr;
        }
        else
        {
            //신규 획득
            int best = FindBestSlotIndex(monster, allowUsed: false);
            if (best == -1)
                return null;

            _occupied.Add(monster, best);
            _used[best] = true;
            _slotData[best].used = true;

            return _slotData[best].tr;
        }
    }

    /// <summary>
    /// Angle Bias + 거리 기반 점수로 최적 슬롯 선택
    /// - allowUsed=false: 빈 슬롯만 선택
    /// - 점수는 낮을수록 좋음
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="allowUsed"></param>
    /// <returns></returns>
    private int FindBestSlotIndex(IMonster monster, bool allowUsed)
    {
        Vector3 monsterPos = monster.Transform.position;

        //플레이어 "진행 방향" (혹은 facing)
        Vector3 forward = _targetPlayer.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;
        else
            forward.Normalize();

        //플레이어 위치
        Vector3 playerPos = transform.position;

        int bestIndex = -1;
        float bestScore = float.MaxValue;

        for (int i = 0; i < _slotData.Length; i++)
        {
            if (!allowUsed && _used[i])
                continue;

            Vector3 slotWorld = _slotData[i].tr.position;

            //거리 점수(가까운 슬롯 선호)
            float distScore = (monsterPos - slotWorld).sqrMagnitude;

            //링 패널티(내측 링 선호)
            float ringPenalty = _slotData[i].ring * 1.15f; // 링이 바깥일수록 더 큰 패널티

            //Angle Bias: 슬롯 방향이 플레이어 forward 기준 어디냐에 따라 가중치
            float bias = 1f;
            if (_useAngleBias)
            {
                Vector3 dirToSlot = slotWorld - playerPos;
                dirToSlot.y = 0f;
                if (dirToSlot.sqrMagnitude > 0.0001f)
                    dirToSlot.Normalize();
                else
                    dirToSlot = forward;

                //forward와의 내적: 1(정면) ~ -1(후면)
                float dot = Vector3.Dot(forward, dirToSlot);

                //dot을 0~1 구간으로 맵핑(정면=1, 후면=0)
                float front01 = Mathf.Clamp01((dot + 1f) * 0.5f);

                //측면 성분: dot이 0 근처일수록 측면
                float side01 = 1f - Mathf.Abs(dot);

                //가중치 조합(유저 설정 값)
                float w = (_forwardBias * Mathf.Pow(front01, _biasSharpness)) +
                          (_sideBias * Mathf.Pow(side01, _biasSharpness)) +
                          (_backBias * Mathf.Pow(1f - front01, _biasSharpness));

                //w가 클수록 "선호" -> score는 낮을수록 좋으니 bias는 1/w 형태로
                bias = 1f / Mathf.Max(0.001f, w);
            }

            float score = distScore * bias + ringPenalty;

            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public Transform RequestSlot(IMonster monster)
    {
        return AcquireOrRefreshSlot(monster, forceRefresh: false);
    }

    public bool IsInnerRing(Transform slot)
    {
        for (int i = 0; i < _slotData.Length; i++)
        {
            if (_slotData[i].tr == slot)
                return _slotData[i].ring == 0;
        }
        return false;
    }

    public void ReleaseSlot(IMonster monster)
    {
        if (_occupied.TryGetValue(monster, out int index))
        {
            _occupied.Remove(monster);

            _used[index] = false;
            _slotData[index].used = false;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_slotData == null)
            return;

        //슬롯 시각화
        for (int i = 0; i < _slotData.Length; i++)
        {
            if (_slotData[i].tr == null) continue;

            Gizmos.color = _slotData[i].used ? Color.red : Color.cyan;
            Gizmos.DrawSphere(_slotData[i].tr.position, 0.06f);
        }
    }
#endif
}
