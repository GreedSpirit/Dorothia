using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

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
    [SerializeField] private int _baseSlotCount = 16;               // 플레이어 주변 슬롯 수
    [SerializeField] private float _baseRadius = 0.7f;                // 플레이어 중심에서 슬롯까지 거리
    [SerializeField] private float _randomOffset = 0f;              // 슬롯 퍼짐 정도

    [Header("Adaptive Slot (Rings)")]
    [SerializeField] private bool _useAdaptive = true;
    [SerializeField] private int _maxRings = 4;                     //링 최대 개수(120마리 대비)
    [SerializeField] private int[] _slotsPerRingList = new int[] { 16, 24, 32, 40 };
    [SerializeField] private float _ringSpacing = 0.6f;             //링 간 거리
    [SerializeField] private int _maxSlots = 120;

    [Header("Inner Ring Limit")]
    [SerializeField]
    private int _innerRingLimit = 16;

    [Header("Angle Bias")]
    [SerializeField] private bool _useAngleBias = false;

    [Header("Slot Refresh")]
    [SerializeField] private float _rebuildInterval = 1.2f;         //목표 이동에 맞춰 슬롯 재배치 간격
    [SerializeField] private float _rebuildMoveThreshold = 1f;      //플레이어 이동량이 이 이상이면 재배치

    private Dictionary<IMonster, bool> _arrived = new();

    private bool _isInitialized = false;

    // 내부 캐시/풀
    private struct SlotData
    {
        public Transform tr;
        public bool used;
        public int ring;
    }

    private SlotData[] _slotData;
    private readonly Dictionary<IMonster, int> _occupied = new(128); //어떤 몬스터가 어떤 슬롯을 점유하는지
    private bool[] _used;

    //플레이어 이동/방향 기반 슬롯 재빌드
    private Vector3 _lastRebuildWorldPos;
    private float _nextRebuildTime;

    //랜덤 오프셋 고정(슬롯마다 시드) - "튀는" 랜덤 방지
    //같은 슬롯을 점유해도 매 프레임 랜덤이 바뀌면 부자연스러움
    private Vector2[] _slotOffsetSeed;

    private void Start()
    {
        StartCoroutine(InitSlots());
    }

    private IEnumerator InitSlots()
    {
        yield return null; // 1프레임 대기
        BuildSlots(true);

        _isInitialized = true;
    }

    private void LateUpdate()
    {
        //Slot Refresh (Rebuild)
        // - 플레이어가 일정 거리 이상 이동하면 슬롯 원형을 재정렬
        // - 너무 자주 하면 몬스터 목표가 계속 흔들림 -> interval로 제한
        transform.position = _targetPlayer.position;
        transform.rotation = Quaternion.identity;

        UpdateSlotPositions();

        if (Time.time < _nextRebuildTime)
            return;

        float sqrMove = (transform.position - _lastRebuildWorldPos).sqrMagnitude;

        if (sqrMove >= _rebuildMoveThreshold * _rebuildMoveThreshold)
        {
            BuildSlots(false);
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

        int totalSlots = 0;
        for (int i = 0; i < _slotsPerRingList.Length; i++)
            totalSlots += _slotsPerRingList[i]; // 총 슬롯 수 계산

        int desiredSlots = Mathf.Min(_maxSlots, totalSlots); //총 슬롯 개수 결정

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
                go.transform.SetParent(RuntimeRootManager.Slots); // 월드좌표 기준

                _slotData[i] = new SlotData
                {
                    tr = go.transform,
                    used = false,
                    ring = 0
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

        Vector3 playerPos = _targetPlayer.position; // 기준을 항상 targetPlayer로

        int total = _slotData.Length;
        int idx = 0;

        //링별 슬롯 생성
        for (int ring = 0; ring < _slotsPerRingList.Length && idx < desiredSlots; ring++)
        {
            float radius = _baseRadius + _ringSpacing * ring;

            int ringSlots = Mathf.Min(_slotsPerRingList[ring], desiredSlots - idx);

            for (int j = 0; j < ringSlots && idx < desiredSlots; j++, idx++)
            {
                float t = (float)j / ringSlots;
                float angle = t * Mathf.PI * 2f;

                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 basePos = dir * radius;

                Vector3 offset = ComputeStableOffset(idx, dir, _randomOffset);

                Vector3 worldPos = playerPos + basePos + offset; // 월드 좌표 직접계산

                _slotData[idx].ring = ring;
                _slotData[idx].tr.position = worldPos;
            }
        }

        //for (int i = 0; i < _slotData.Length; i++)
        //    _slotData[i].used = _used[i];

        //실제 점유 기반으로 used 재구성
        System.Array.Clear(_used, 0, _used.Length);

        foreach (var kv in _occupied)
        {
            int index = kv.Value;

            if (index >= 0 && index < _used.Length)
            {
                _used[index] = true;
                _slotData[index].used = true;
            }
        }
    }

    
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
        if (monster.Stats.Rank == Monster_Type.Boss) // 보스제외
            return null;

        //이미 점유 중이면 반환 (forceRefresh가 true면 더 좋은 슬롯으로 교체 가능)
        if (_occupied.TryGetValue(monster, out int currentIndex))
        {
            int currentRing = _slotData[currentIndex].ring;

            //항상 더 안쪽 슬롯 탐색
            int better = FindBestSlotIndex(monster, false, currentRing);

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
            int best = FindBestSlotIndex(monster, false, int.MaxValue);
            if (best == -1)
                return null;

            _occupied.Add(monster, best);
            _used[best] = true;
            _slotData[best].used = true;

            return _slotData[best].tr;
        }
    }

    /// <summary>
    /// 최적 슬롯 선택
    /// </summary>
    /// <param name="monster"></param>
    /// <param name="allowUsed"></param>
    /// <param name="currentRing"></param>
    /// <returns></returns>
    private int FindBestSlotIndex(IMonster monster, bool allowUsed, int currentRing)
    {
        Vector3 monsterPos = monster.Transform.position;

        int bestIndex = -1;
        float bestScore;

        for (int ring = 0; ring < _slotsPerRingList.Length; ring++)
        {
            //바깥으로 이동 금지
            if (currentRing != int.MaxValue && ring > currentRing)
                continue;

            if (ring == 0)
            {
                int count = 0;

                for (int i = 0; i < _slotData.Length; i++)
                {
                    if (_slotData[i].ring == 0 && _used[i])
                        count++;
                }

                if (count >= _innerRingLimit)
                    continue;
            }

            bestScore = float.MaxValue;
            bestIndex = -1;

            //해당 링만 검사
            for (int i = 0; i < _slotData.Length; i++)
            {
                if (_slotData[i].ring != ring)
                    continue;

                //빈 슬롯 우선
                if (!_used[i])
                {
                    float dist = (monsterPos - _slotData[i].tr.position).sqrMagnitude;

                    if (dist < bestScore)
                    {
                        bestScore = dist;
                        bestIndex = i;
                    }
                    continue;
                }

                //사용중 슬롯 -> 더 가까우면 뺏기
                var owner = GetMonsterBySlot(i);

                if (owner != null && IsBetterCandidate(monster, owner, _slotData[i].tr))
                {
                    ReleaseSlot(owner);
                    return i;
                }
            }

            //이 링에서 하나라도 찾았으면 즉시 반환
            if (bestIndex != -1)
                return bestIndex;
        }

        return bestIndex;
    }

    private IMonster GetMonsterBySlot(int index)
    {
        foreach (var kv in _occupied)
        {
            if (kv.Value == index)
                return kv.Key;
        }
        return null;
    }

    private bool IsBetterCandidate(IMonster a, IMonster b, Transform slot)
    {
        float distA = (a.Transform.position - slot.position).sqrMagnitude;
        float distB = (b.Transform.position - slot.position).sqrMagnitude;

        bool aArrived = _arrived.ContainsKey(a) && _arrived[a];
        bool bArrived = _arrived.ContainsKey(b) && _arrived[b];

        //도착한 놈 vs 안한 놈 -> 무조건 도착한 놈 승
        if (aArrived && !bArrived) return true;
        if (!aArrived && bArrived) return false;

        //둘 다 도착했으면 -> 거리 비교
        if (aArrived && bArrived)
            return distA < distB;

        //둘 다 미도착 -> 더 가까운 놈
        return distA < distB;
    }

    public void NotifyArrived(IMonster monster, bool arrived)
    {
        _arrived[monster] = arrived;
    }

    private void UpdateSlotPositions()
    {
        if (_slotData == null) return;

        Vector3 playerPos = _targetPlayer.position;

        int total = _slotData.Length;
        int idx = 0;

        //링별 구조 동일하게 유지
        for (int ring = 0; ring < _slotsPerRingList.Length && idx < total; ring++)
        {
            float radius = _baseRadius + _ringSpacing * ring;

            int ringSlots = Mathf.Min(_slotsPerRingList[ring], total - idx);

            for (int j = 0; j < ringSlots && idx < total; j++, idx++)
            {
                float t = (float)j / ringSlots;
                float angle = t * Mathf.PI * 2f;

                Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                Vector3 basePos = dir * radius;

                Vector3 offset = ComputeStableOffset(idx, dir, _randomOffset);

                Vector3 worldPos = playerPos + basePos + offset;

                _slotData[idx].tr.position = worldPos;
            }
        }
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
            _arrived.Remove(monster);
        }
    }

    public bool IsValidSlot(Transform slot)
    {
        for (int i = 0; i < _slotData.Length; i++)
        {
            if (_slotData[i].tr == slot)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 슬롯 전체 초기화
    /// </summary>
    public void ClearAllSlots()
    {
        if (!_isInitialized)
        {
            Debug.Log("[PlayerCombatSlots] 아직 초기화 안됨");
            return;
        }

        _occupied.Clear();

        for (int i = 0; i < _used.Length; i++)
        {
            _used[i] = false;
            _slotData[i].used = false;
        }

        _arrived.Clear();
    }

    /// <summary>
    /// 가장 가까운 슬롯만 최초 점유
    /// </summary>
    /// <param name="monster"></param>
    /// <returns></returns>
    public Transform AcquireNearestSlot(IMonster monster)
    {
        int bestIndex = -1;
        float bestScore = float.MaxValue;

        for (int i = 0; i < _slotData.Length; i++)
        {
            if (_used[i]) continue;

            float dist = (monster.Transform.position - _slotData[i].tr.position).sqrMagnitude;

            if (dist < bestScore)
            {
                bestScore = dist;
                bestIndex = i;
            }
        }

        if (bestIndex == -1)
            return null;

        _occupied[monster] = bestIndex;
        _used[bestIndex] = true;
        _slotData[bestIndex].used = true;

        return _slotData[bestIndex].tr;
    }

    public bool IsInsideSlotArea(Vector3 pos)
    {
        float maxRadius = _baseRadius + _ringSpacing * (_slotsPerRingList.Length - 1);
        return (pos - _targetPlayer.position).sqrMagnitude <= maxRadius * maxRadius;
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
