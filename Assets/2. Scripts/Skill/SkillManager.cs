using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public struct SkillKey
{
    public int sid;
    public Skill_Type type;
    public Rarity rarity;
    public bool isScroll;

    public SkillKey(int sid, Skill_Type type, Rarity rarity, bool isScroll = false)
    {
        this.sid = sid;
        this.type = type;
        this.rarity = rarity;
        this.isScroll = isScroll;
    }

    public static bool operator ==(SkillKey left, SkillKey right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SkillKey left, SkillKey right)
    {
        return !left.Equals(right);
    }

    public bool Equals(SkillKey other)
    {
        return sid == other.sid && type == other.type && rarity == other.rarity && isScroll == other.isScroll;
    }

    public override bool Equals(object obj)
    {
        return obj is SkillKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(sid, type, rarity, isScroll);
    }
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("등급 별 머테리얼")]
    [SerializeField] private List<Sprite> grades = new List<Sprite>();
    [SerializeField] private Sprite newSprite;
    [SerializeField] public Sprite pickSprite;
    public BaseSkill[] ActiveSlots { get; private set; } = new BaseSkill[ACTIVE_SLOT_MAX];
    public BaseSkill[] PassiveSlots { get; private set; } = new BaseSkill[PASSIVE_SLOT_MAX];
    public BaseSkill UltimateSlot { get; private set; }

    public const int ACTIVE_SLOT_MAX = 3;
    public const int PASSIVE_SLOT_MAX = 5;

    // 데이터 저장소 (Key: SkillKey, Value: 보유 수량)
    public IReadOnlyDictionary<SkillKey, int> Inventory => _inventory;
    private Dictionary<SkillKey, int> _inventory = new Dictionary<SkillKey, int>();

    // 해금된 실제 스킬 객체들 (Logic 인스턴스)
    public IReadOnlyDictionary<SkillKey, BaseSkill> UnlockedSkills => _unlockedSkills;
    private Dictionary<SkillKey, BaseSkill> _unlockedSkills = new Dictionary<SkillKey, BaseSkill>();

    // 이미 확인(클릭)한 스킬들의 ID를 저장 (중복 방지를 위해 HashSet 사용)
    private HashSet<string> _confirmedSkillIds = new HashSet<string>();

    // UI 아이템 캐싱
    private Dictionary<SkillKey, SkillItem> _uiCache = new Dictionary<SkillKey, SkillItem>();

    // 신비 게이지 현황
    public const float MYSTERY_LIMIT = 10000;
    private float _mysteryGauge;
    public float MysteryGauge
    {
        get => _mysteryGauge;
        set
        {
            _mysteryGauge = value;
            if (_mysteryGauge > MYSTERY_LIMIT) _mysteryGauge = MYSTERY_LIMIT;
            OnMysteryGaugeChanged?.Invoke(_mysteryGauge);
        }
    }
    public event Action<float> OnMysteryGaugeChanged;
    public event Action<Skill_Type, int> OnEquipSkillChanged;

    // 이벤트: 데이터 변경 시 (키)
    public const int MAX_LEVEL = 100;
    public event Action<SkillKey> OnInventoryChanged;

    // 사이클 위치를 추적하기 위한 인덱스 (0, 1, 2: 액티브 / 3: 궁극기)
    private int _currentCycleIndex = 0;
    private const int TOTAL_CYCLE_STEPS = 4; // 액티브 3개 + 궁극기 1개

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
    }

    private void Update()
    {
        //신비게이지 테스트
        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            MysteryGauge += 1000;
        }

        float dt = Time.deltaTime;
        // 액티브 슬롯 쿨다운 업데이트
        for (int i = 0; i < ActiveSlots.Length; i++)
        {
            ActiveSlots[i]?.UpdateCooldown(dt);
        }

        // 궁극기 쿨다운 업데이트
        UltimateSlot?.UpdateCooldown(dt);
    }

    public List<SkillData> GetRandomScroll(int amount)
    {
        List<SkillData> list = new List<SkillData>();

        for (int i = 0; i < amount; i++)
        {
            SkillKey key = GetRandomkey();
            SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);
            list.Add(data);

            AddItem(key);
        }

        return list;
    }


    private SkillKey GetRandomkey()
    {
        var seed = DataManager.Instance.GetDict<SkillData>().ToList();
        int count = seed.Count;

        int id = UnityEngine.Random.Range(0, count);

        return new SkillKey(seed[id].Key, seed[id].Value.Skill_Type, Rarity.Normal, true);
    }



    #region Skill Cycle

    // 현재 준비된 스킬만 확인
    public BaseSkill PeekReadySkill()
    {
        for (int i = 0; i < TOTAL_CYCLE_STEPS; i++)
        {
            int checkIndex = (_currentCycleIndex + i) % TOTAL_CYCLE_STEPS;
            BaseSkill target = GetSkillByCycleIndex(checkIndex);
            if (target != null && target.IsReady)
                return target;
        }
        return null;
    }

    // 사용 확정 시에만
    public void ConsumeReadySkill()
    {
        for (int i = 0; i < TOTAL_CYCLE_STEPS; i++)
        {
            BaseSkill target = GetSkillByCycleIndex(_currentCycleIndex);
            _currentCycleIndex = (_currentCycleIndex + 1) % TOTAL_CYCLE_STEPS;
            if (target != null && target.IsReady)
                return;
        }
    }

    private BaseSkill GetSkillByCycleIndex(int index)
    {
        if (index >= 0 && index < ACTIVE_SLOT_MAX)
        {
            return ActiveSlots[index];
        }
        else if (index == 3) // 마지막 단계는 궁극기
        {
            return UltimateSlot;
        }
        return null;
    }

    public void ResetAllCooldown()
    {
        for (int i = 0; i < ActiveSlots.Length; i++)
        {
            ActiveSlots[i]?.ResetCoolDown();
        }

        // 궁극기 쿨다운 업데이트
        UltimateSlot?.ResetCoolDown();
    }
    #endregion

    // 전투 종료나 맵 이동 시 사이클을 초기화하고 싶다면 호출
    public void ResetSkillCycle()
    {
        _currentCycleIndex = 0;
    }

    #region Inventory Core Logic

    public void AddItem(SkillKey key, int count = 1)
    {
        if (!_inventory.ContainsKey(key))
        {
            _inventory[key] = 0;
        }

        _inventory[key] += count;
        //Debug.Log($"{key.sid}, {_inventory[key]}");
        OnInventoryChanged?.Invoke(key);
    }

    public BaseSkill GetSkill(SkillKey key) => UnlockedSkills.TryGetValue(key, out BaseSkill bs) ? bs : null;
    public Sprite GetSpriteByGrade(Rarity rarity) => grades[(int)rarity - 1];
    public int GetItemCount(SkillKey key) => _inventory.GetValueOrDefault(key, 0);
    public bool IsNewSkill(SkillKey key) => UnlockedSkills.ContainsKey(key);

    #endregion

    #region Craft & Merge

    // 주문서 3개 -> 노말 스킬 1개
    public void CraftSkill(SkillKey scrollKey, int craftCount = 1)
    {
        if (!scrollKey.isScroll || GetItemCount(scrollKey) < 3) return;

        int vaildCount = Inventory[scrollKey] / 3;
        if (vaildCount < craftCount) return;

        //int craftCount = _inventory[scrollKey] / 3;
        _inventory[scrollKey] -= (3 * craftCount);
        OnInventoryChanged?.Invoke(scrollKey);

        // 주문서와 같은 ID의 'Normal' 등급 스킬 생성
        SkillKey skillKey = new SkillKey(scrollKey.sid, scrollKey.type, Rarity.Normal, false);

        UnlockAndAddItem(skillKey, craftCount);
    }

    // 같은 등급 스킬 3개 -> 다음 등급 스킬 1개
    public bool MergeSkill(SkillKey skillKey, bool isMysteryOn = false)
    {
        // 기본 조건 체크 (전설 등급이나 스크롤은 합성 불가)
        if (skillKey.isScroll || skillKey.rarity == Rarity.Legendary) return false;

        int currentAmount = GetItemCount(skillKey);
        if (currentAmount < 3) return false;
        // 2. 데이터 및 확률 로드
        Skill_RankData rankData = DataManager.Instance.GetData<Skill_RankData>((int)skillKey.rarity);
        if (rankData == null) return false;

        float successProb = rankData.Skill_Success_Prob;
        if (isMysteryOn)
        {
            successProb = Mathf.Min(successProb * 2, 1.0f); // 확률 상한선 
            // 신비게이지 작동 시 게이지 선 소모
            MysteryGauge = 0;
        }

        // 합성 시도 횟수 계산 및 재료 선소모
        int attemptCount = currentAmount / 3;
        _inventory[skillKey] -= (attemptCount * 3);

        // UI에 재료 감소 즉시 반영
        OnInventoryChanged?.Invoke(skillKey);

        // 확률 계산
        int successCount = 0;
        for (int i = 0; i < attemptCount; i++)
        {
            if (UnityEngine.Random.value <= successProb)
            {
                successCount++;
            }
        }

        int failCount = attemptCount - successCount;

        if (failCount > 0)
        {
            MysteryGauge += rankData.Skill_Rank_Failure * failCount;
        }

        if (successCount > 0)
        {
            Rarity nextRarity = (Rarity)((int)skillKey.rarity + 1);
            SkillKey upgradedKey = new SkillKey(skillKey.sid, skillKey.type, nextRarity, false);

            Debug.Log($"{skillKey.sid} 합성: {attemptCount}회 시도 중 {successCount}회 성공!");
            UnlockAndAddItem(upgradedKey, successCount);
            return true;
        }
        else
        {
            Debug.Log($"{skillKey.sid} 합성 {attemptCount}회 시도했으나 모두 실패...");
            return false; // 시도는 했으나 결과물이 없음
        }
    }

    // 공통: 스킬 인스턴스 생성 및 인벤토리 추가 루틴
    private void UnlockAndAddItem(SkillKey skillKey, int count)
    {
        if (!_unlockedSkills.ContainsKey(skillKey))
        {
            var sData = DataManager.Instance.GetData<SkillData>(skillKey.sid);
            //var stData = DataManager.Instance.GetData<Skill_StatusData>(sData.Skill_Status_Id);

            BaseSkill newSkill = BaseSkill.Create(sData);
            newSkill.Rarity = skillKey.rarity; // 스킬 객체에도 등급 설정
            _unlockedSkills[skillKey] = newSkill;
        }

        AddItem(skillKey, count);
    }

    // UI 등에서 호출할 일괄 합성 버튼용
    public void AllCraftAndMerge()
    {
        var keys = _inventory.Keys.ToList();

        foreach (var key in keys)
        {
            if (key.isScroll)
            {
                int craftCount = Inventory[key] / 3;
                CraftSkill(key, craftCount);
            }

            // 스킬 일괄 합성이 있다면
            //else
            //    MergeSkill(key);
        }
    }

    #endregion

    #region Slot Management

    public bool IsEquipped(SkillKey key)
    {
        return ActiveSlots.Any(s => s != null && s.Data.Job_Skill_Id == key.sid && s.Rarity == key.rarity) ||
               PassiveSlots.Any(s => s != null && s.Data.Job_Skill_Id == key.sid && s.Rarity == key.rarity) ||
               (UltimateSlot != null && UltimateSlot.Data.Job_Skill_Id == key.sid && UltimateSlot.Rarity == key.rarity);
    }

    public int GetEquippedIndex(SkillKey key)
    {
        var sData = DataManager.Instance.GetData<SkillData>(key.sid);
        if (sData == null) return -1;

        Skill_Type type = sData.Skill_Type;

        if (type == Skill_Type.Active)
        {
            for (int i = 0; i < ActiveSlots.Length; i++)
            {
                if (ActiveSlots[i] != null &&
                    ActiveSlots[i].Data.Job_Skill_Id == key.sid &&
                    ActiveSlots[i].Rarity == key.rarity)
                {
                    return i;
                }
            }
        }
        else if (type == Skill_Type.Passive)
        {
            for (int i = 0; i < PassiveSlots.Length; i++)
            {
                if (PassiveSlots[i] != null &&
                    PassiveSlots[i].Data.Job_Skill_Id == key.sid &&
                    PassiveSlots[i].Rarity == key.rarity)
                {
                    return i;
                }
            }
        }
        else if (type == Skill_Type.Ultimate)
        {
            if (UltimateSlot != null &&
                UltimateSlot.Data.Job_Skill_Id == key.sid &&
                UltimateSlot.Rarity == key.rarity)
            {
                return 0;
            }
        }

        return -1; // 장착되어 있지 않음
    }
    public void AutoEquip()
    {
        // 모든 슬롯을 비우고
        ClearAllSlots();

        // 중복 장착 방지
        var equippedSids = new HashSet<int>();

        // 2. 장착 후보 리스트 추출 및 정렬 (희귀도 순 -> 레벨 순)
        var candidates = _unlockedSkills
            .Where(pair => !pair.Key.isScroll && GetItemCount(pair.Key) > 0)
            .OrderByDescending(pair => (int)pair.Key.rarity)
            .ThenByDescending(pair => pair.Value.Level)
            .ToList();

        // 최적의 스킬들로 배치
        foreach (var (key, skill) in candidates)
        {
            // 동일한 스킬 종류(sid)는 하나만 장착 가능
            if (equippedSids.Contains(key.sid)) continue;

            bool isEquipped = false;

            switch (skill.Data.Skill_Type)
            {
                case Skill_Type.Active:
                    int activeIdx = Array.FindIndex(ActiveSlots, s => s == null);
                    if (activeIdx != -1)
                    {
                        EquipSkill(key, activeIdx);
                        isEquipped = true;
                    }
                    break;

                case Skill_Type.Passive:
                    int passiveIdx = Array.FindIndex(PassiveSlots, s => s == null);
                    if (passiveIdx != -1)
                    {
                        EquipSkill(key, passiveIdx);
                        isEquipped = true;
                    }
                    break;

                case Skill_Type.Ultimate:
                    if (UltimateSlot == null)
                    {
                        EquipSkill(key, 0);
                        isEquipped = true;
                    }
                    break;
            }

            if (isEquipped)
            {
                equippedSids.Add(key.sid);
            }
        }
    }

    public void ClearAllSlots()
    {
        // 액티브 슬롯 초기화
        for (int i = 0; i < ActiveSlots.Length; i++)
        {
            if (ActiveSlots[i] != null) UnequipActive(i);
        }

        // 패시브 슬롯 초기화
        for (int i = 0; i < PassiveSlots.Length; i++)
        {
            if (PassiveSlots[i] != null) UnequipPassive(i);
        }

        // 궁극기 슬롯 초기화
        if (UltimateSlot != null)
        {
            UnequipUltimate();
        }

        // 필요 시 능력치 재계산이나 UI 갱신 호출
    }

    public void EquipSkill(SkillKey key, int targetIndex = -1)
    {
        if (!_unlockedSkills.TryGetValue(key, out BaseSkill skill)) return;
        if (IsEquipped(key)) return;

        Skill_Type type = skill.Data.Skill_Type;

        if (type == Skill_Type.Active)
        {
            EquipActive(skill, targetIndex);
        }
        else if (type == Skill_Type.Passive)
        {
            EquipPassive(skill, targetIndex);
        }
        else if (type == Skill_Type.Ultimate)
        {
            EquipUltimate(skill);
        }

        OnEquipSkillChanged?.Invoke(type, targetIndex);
    }

    private void EquipActive(BaseSkill skill, int targetIndex)
    {
        int index = (targetIndex >= 0) ? targetIndex : Array.FindIndex(ActiveSlots, s => s == null);

        if (index >= 0 && index < ACTIVE_SLOT_MAX)
        {
            if (ActiveSlots[index] != null) UnequipActive(index);
            ActiveSlots[index] = skill;
            AddressableManager.Instance.LoadAsset<Sprite>(skill.Data.Skill_Icon);

            // 액티브 이펙트 로드
            // 액티브 스킬 모션 로드
        }
    }

    private void EquipPassive(BaseSkill skill, int targetIndex)
    {
        int index = (targetIndex >= 0) ? targetIndex : Array.FindIndex(PassiveSlots, s => s == null);

        if (index >= 0 && index < PASSIVE_SLOT_MAX)
        {
            // 1. 기존 패시브가 있다면 해제 (스탯 원복 등)
            if (PassiveSlots[index] != null) UnequipPassive(index);

            // 2. 새 패시브 장착
            PassiveSlots[index] = skill;

            // 3. 패시브 효과 적용 (PlayerCtrl 등에 스탯 반영 알림)
            if (skill is PassiveSkill passive)
                passive.Apply();

            // 3. 패시브 효과 적용 (PlayerCtrl 등에 스탯 반영 알림)
            //StatManager.Instance.RefreshStats();

            AddressableManager.Instance.LoadAsset<Sprite>(skill.Data.Skill_Icon);
            // 패시브는 인게임 퀵슬롯(ingameSlots)에 들어가지 않으므로 UpdateInGameSlots 생략 가능
        }
    }

    private void EquipUltimate(BaseSkill skill)
    {
        if (UltimateSlot != null) UnequipUltimate();
        UltimateSlot = skill;
        AddressableManager.Instance.LoadAsset<Sprite>(skill.Data.Skill_Icon);

        // 궁극기 이펙트 로드
        // 궁극기 스킬 모션 로드
    }

    public void UnequipPassive(int index)
    {
        if (index < 0 || index >= PASSIVE_SLOT_MAX || PassiveSlots[index] == null) return;

        string iconAddr = PassiveSlots[index].Data.Skill_Icon;

        (PassiveSlots[index] as PassiveSkill).Undo();
        PassiveSlots[index] = null;

        AddressableManager.Instance.ReleaseAsset(iconAddr);
        OnEquipSkillChanged?.Invoke(Skill_Type.Passive, index);
    }

    public void UnequipActive(int index)
    {
        if (index < 0 || index >= ACTIVE_SLOT_MAX || ActiveSlots[index] == null) return;

        string iconAddr = ActiveSlots[index].Data.Skill_Icon;
        ActiveSlots[index] = null;

        AddressableManager.Instance.ReleaseAsset(iconAddr);

        // 액티브 이펙트 해제
        // 액티브 스킬 모션 해제

        OnEquipSkillChanged?.Invoke(Skill_Type.Active, index);
    }

    public void UnequipUltimate()
    {
        if (UltimateSlot == null) return;

        string iconAddr = UltimateSlot.Data.Skill_Icon;
        UltimateSlot = null;

        AddressableManager.Instance.ReleaseAsset(iconAddr);

        // 궁극기 이펙트 해제
        // 궁극기 스킬 모션 해제

        OnEquipSkillChanged?.Invoke(Skill_Type.Ultimate, 0);
    }

    #endregion

    #region Reinforce

    public int GetReinforceCost(SkillKey key)
    {
        if (!_unlockedSkills.TryGetValue(key, out BaseSkill skill)) return -1;

        Skill_Upgrade_GoldData upgradeData = DataManager.Instance.GetData<Skill_Upgrade_GoldData>((int)key.rarity);
        if (upgradeData == null) return -1;

        float baseCost = upgradeData.Skill_Rank_Gold;
        float levelWeight = upgradeData.Skill_Upgrade_CostRate;

        // 베이스 골드 * (가중치^스킬레벨)
        // 2->3레벨 강화 기준 : 605골드
        float totalCost = baseCost * Mathf.Pow(levelWeight, skill.Level);

        return Mathf.RoundToInt(totalCost);
    }
    public enum ReinforceResult { Success, NotFound, LevelMax, NotEnoughGold }

    public ReinforceResult ReinforceSkill(SkillKey key)
    {
        // 스킬 존재 여부 확인
        if (!_unlockedSkills.TryGetValue(key, out BaseSkill skill))
        {
            return ReinforceResult.NotFound;
        }

        if (skill.Level >= MAX_LEVEL)
        {
            return ReinforceResult.LevelMax;
        }

        // 데이터 존재 여부 먼저 확인
        Skill_Upgrade_GoldData upgradeData = DataManager.Instance.GetData<Skill_Upgrade_GoldData>((int)key.rarity);
        if (upgradeData == null)
        {
            Debug.LogError($"Skill_Upgrade_GoldData를 찾을 수 없습니다: {key.rarity}");
            return ReinforceResult.NotFound; // 또는 별도의 DataError 리턴값 추가
        }

        // 비용 계산
        int cost = GetReinforceCost(key);
        if (cost == -1) return ReinforceResult.NotFound;

        // 골드 차감 시도
        // todo : 장비 효과에 골드 차감효과 적용해야함
        if (!ExchangeManager.Instance.UseMoney(MoneyType.Gold, cost))
            return ReinforceResult.NotEnoughGold;

        // 강화 성공 처리
        skill.Level++;
        OnInventoryChanged?.Invoke(key);

        //Debug.Log($"{key.sid} 강화 완료 → Lv.{skill.Level} (비용: {cost}G)");
        return ReinforceResult.Success;
    }

    #endregion

    #region Save & Load

    public SkillSaveData GetSaveData()
    {
        var data = new SkillSaveData();

        // 인벤토리
        foreach (var (key, count) in _inventory)
        {
            data.inventory.Add(new InventoryEntry
            {
                key = new SerializableSkillKey(key),
                count = count
            });
        }

        // 해금 된 스킬(레벨포함)
        foreach (var (key, skill) in _unlockedSkills)
        {
            data.unlockedSkills.Add(new UnlockedSkillEntry
            {
                key = new SerializableSkillKey(key),
                level = skill.Level
            });
        }

        // 슬롯 관련
        for (int i = 0; i < ACTIVE_SLOT_MAX; i++)
        {
            data.activeSlots[i] = SlotToEntry(ActiveSlots[i]);
        }

        for (int i = 0; i < PASSIVE_SLOT_MAX; i++)
        {
            data.passiveSlots[i] = SlotToEntry(PassiveSlots[i]);
        }

        data.ultimateSlot = SlotToEntry(UltimateSlot);

        // 신비게이지
        data.mysteryGauge = _mysteryGauge;

        return data;
    }

    public void LoadFromSaveData(SkillSaveData data)
    {
        if (data == null) return;

        _inventory.Clear();
        _unlockedSkills.Clear();
        ClearAllSlots();

        // 해금 된 스킬 먼저 로드
        foreach (var entry in data.unlockedSkills)
        {
            SkillKey key = entry.key.ToSkillKey();
            var sData = DataManager.Instance.GetData<SkillData>(key.sid);
            if (sData == null) continue;

            BaseSkill skill = BaseSkill.Create(sData);
            skill.Rarity = key.rarity;
            skill.Level = entry.level;
            _unlockedSkills[key] = skill;
        }

        // 인벤토리 복원
        foreach (var entry in data.inventory)
        {
            SkillKey key = entry.key.ToSkillKey();
            _inventory[key] = entry.count;
            OnInventoryChanged?.Invoke(key);
        }

        // 슬롯 복원
        for (int i = 0; i < ACTIVE_SLOT_MAX; i++)
        {
            if (data.activeSlots[i] != null && !data.activeSlots[i].isEmpty)
                EquipSkill(data.activeSlots[i].key.ToSkillKey(), i);
        }

        for (int i = 0; i < PASSIVE_SLOT_MAX; i++)
        {
            if (data.passiveSlots[i] != null && !data.passiveSlots[i].isEmpty)
                EquipSkill(data.passiveSlots[i].key.ToSkillKey(), i);
        }

        if (data.ultimateSlot != null && !data.ultimateSlot.isEmpty)
            EquipSkill(data.ultimateSlot.key.ToSkillKey(), 0);

        // 신비게이지
        MysteryGauge = data.mysteryGauge;
    }

    private SlotEntry SlotToEntry(BaseSkill skill)
    {
        if (skill == null)
            return new SlotEntry { isEmpty = true };

        return new SlotEntry
        {
            isEmpty = false,
            key = new SerializableSkillKey(
                          new SkillKey(skill.Data.Job_Skill_Id, skill.Data.Skill_Type, skill.Rarity))
        };
    }

    #endregion
}