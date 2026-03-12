using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct SkillKey
{
    public int sid;
    public Rarity rarity;
    public bool isScroll;

    public SkillKey(int sid, Rarity rarity, bool isScroll = false)
    {
        this.sid = sid;
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
        return sid == other.sid && rarity == other.rarity && isScroll == other.isScroll;
    }

    public override bool Equals(object obj)
    {
        return obj is SkillKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(sid, rarity, isScroll);
    }
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    // 슬롯 관리 (PlayerCtrl이 참조할 실시간 리스트)
    [SerializeField] private List<Image> ingameSlots = new List<Image>();
    [SerializeField] private Image ingameUltiSlots;
    private HashSet<string> releaseIconStr = new HashSet<string>();

    public List<BaseSkill> SkillSlots { get; private set; } = new List<BaseSkill>();
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
    public const float MYSTERY_LIMIT = 100000;
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
    public event Action OnEquipSkillChanged;

    // 이벤트: 데이터 변경 시 (키, 변경된 수량)
    public event Action<SkillKey, int> OnInventoryChanged;

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
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            for (int i = 0; i < 30; i++) GetRandomScroll();
        }

        // 신비게이지 테스트
        //if (Keyboard.current.sKey.wasPressedThisFrame)
        //{
        //    MysteryGauge += 1000;
        //}

        float dt = Time.deltaTime;
        for (int i = 0; i < SkillSlots.Count; i++)
        {
            SkillSlots[i].UpdateCooldown(dt);
        }
    }

    public void GetRandomScroll()
    {
        AddItem(GetRandomkey());
    }


    private SkillKey GetRandomkey()
    {
        var seed = DataManager.Instance.GetDict<SkillData>().ToList();
        int count = seed.Count;

        int id = UnityEngine.Random.Range(0, count);

        return new SkillKey(seed[id].Key, Rarity.Normal, true);
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
        OnInventoryChanged?.Invoke(key, _inventory[key]);
    }

    public int GetItemCount(SkillKey key) => _inventory.GetValueOrDefault(key, 0);
    public bool IsNewSkill(SkillKey key) => _unlockedSkills.ContainsKey(key);
    public void MarkAsConfirmed(SkillKey key)
    {
        if (!_confirmedSkillIds.Contains(key.sid.ToString()))
        {
            _confirmedSkillIds.Add(key.sid.ToString());
            //SaveConfirmedSkills(); // 데이터 보존을 위해 저장

            // 필요한 경우 UI 갱신을 위해 이벤트 발생 가능
            // OnInventoryChanged?.Invoke(key, GetItemCount(key));
        }
    }
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
        OnInventoryChanged?.Invoke(scrollKey, _inventory[scrollKey]);

        // 주문서와 같은 ID의 'Normal' 등급 스킬 생성
        SkillKey skillKey = new SkillKey(scrollKey.sid, Rarity.Normal, false);

        UnlockAndAddItem(skillKey, craftCount);
    }

    // 같은 등급 스킬 3개 -> 다음 등급 스킬 1개
    public bool MergeSkill(SkillKey skillKey, bool isMysteryOn = false)
    {
        // 기본 조건 체크
        if (skillKey.isScroll || skillKey.rarity == Rarity.Legendary) return false;

        int currentAmount = GetItemCount(skillKey);
        if (currentAmount < 3) return false;

        // 데이터 및 확률 로드
        Skill_RankData rankData = DataManager.Instance.GetData<Skill_RankData>((int)skillKey.rarity);
        float successProb = rankData.Skill_Success_Prob;

        if (isMysteryOn) successProb *= 2;

        // 합성 시도 횟수 계산
        int attemptCount = currentAmount / 3;
        int successCount = 0;

        // 재료 선소모 (3의 배수만큼 모두 소모)
        _inventory[skillKey] -= (attemptCount * 3);

        // 각 횟수마다 개별 확률 계산
        for (int i = 0; i < attemptCount; i++)
        {
            float randomValue = UnityEngine.Random.value;
            if (randomValue <= successProb)
            {
                successCount++;
            }
        }

        // 인벤토리 UI 갱신 (재료 감소 반영)
        OnInventoryChanged?.Invoke(skillKey, _inventory[skillKey]);

        // 성공 결과 지급
        if (successCount > 0)
        {
            Rarity nextRarity = (Rarity)((int)skillKey.rarity + 1);
            SkillKey upgradedKey = new SkillKey(skillKey.sid, nextRarity, false);

            Debug.Log($"{skillKey.sid} 합성 시도: {attemptCount}회 | 성공: {successCount}회!");
            UnlockAndAddItem(upgradedKey, successCount);
            return true;
        }
        else
        {
            int failCount = attemptCount - successCount;
            MysteryGauge += rankData.Skill_Rank_Failure;

            Debug.Log($"{skillKey.sid} 합성 {attemptCount}회 모두 실패...");
            return false;
        }
    }

    // 공통: 스킬 인스턴스 생성 및 인벤토리 추가 루틴
    private void UnlockAndAddItem(SkillKey skillKey, int count)
    {
        if (!_unlockedSkills.ContainsKey(skillKey))
        {
            var sData = DataManager.Instance.GetData<SkillData>(skillKey.sid);
            var stData = DataManager.Instance.GetData<Skill_StatusData>(sData.Skill_Status_Id);

            BaseSkill newSkill = BaseSkill.Create(sData, stData);
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

    public bool EquipedSkill(SkillKey key) => SkillSlots.Exists(x => x.Data.Job_Skill_Id == key.sid && x.Rarity == key.rarity);

    public void AutoEquip()
    {

    }

    public void EquipSkill(SkillKey key)
    {
        // 1. 해금 여부 확인
        if (!_unlockedSkills.TryGetValue(key, out BaseSkill skill)) return;

        // 2. 이미 장착 중인지 확인
        if (EquipedSkill(key)) return;

        Skill_Type type = skill.Data.Skill_Type;
        int currentCount = SkillSlots.Count(x => x.Data.Skill_Type == type);

        // 3. 슬롯 제한 확인
        bool canEquip = false;
        switch (type)
        {
            case Skill_Type.Active:
                if (currentCount < ACTIVE_SLOT_MAX) canEquip = true;
                break;
            case Skill_Type.Passive:
                if (currentCount < PASSIVE_SLOT_MAX) canEquip = true;
                break;
            case Skill_Type.Ultimate:
                // 얼티밋은 보통 1개 제한이거나 별도 로직이 필요할 수 있습니다.
                canEquip = true;
                break;
        }

        if (canEquip)
        {
            SkillSlots.Add(skill);
            UpdateInGameSlots();
            OnEquipSkillChanged?.Invoke();
        }
    }

    public void UnequipSkill(SkillKey key)
    {
        // 1. 해금 목록에 있는지 확인 (TryGetValue 성공 시 진행해야 하므로 ! 제거)
        if (!_unlockedSkills.TryGetValue(key, out BaseSkill skill)) return;

        // 2. 현재 장착 중인 스킬인지 확인 (가지고 있지 않으면 종료)
        if (!EquipedSkill(key)) return;

        // 3. 리스트에서 제거 (오버라이드된 Equals 덕분에 작동함)
        // 혹은 더 확실하게: SkillSlots.RemoveAll(x => x.Data.Job_Skill_Id == key.sid && x.Rarity == key.rarity);
        if (SkillSlots.Remove(skill))
        {
            UpdateInGameSlots();

            OnEquipSkillChanged?.Invoke();
        }
    }

    #endregion

    private void UpdateInGameSlots()
    {
        foreach (var addr in releaseIconStr)
        {
            AddressableManager.Instance.ReleaseAsset(addr);
        }

        var activeSkills = SkillSlots.Where(x => x.Data.Skill_Type == Skill_Type.Active).ToList();
        var ultiSkill = SkillSlots.FirstOrDefault(x => x.Data.Skill_Type == Skill_Type.Ultimate);

        string address = string.Empty;

        for (int i = 0; i < activeSkills.Count; i++)
        {
            address = activeSkills[i].Data.Skill_Icon;
            if (!releaseIconStr.Contains(address))
            {
                releaseIconStr.Add(address);
            }

            Image slot = ingameSlots[i];

            AddressableManager.Instance.LoadAsset<Sprite>(address, x => slot.sprite = x);
        }

        address = ultiSkill.Data.Skill_Icon;
        if (!releaseIconStr.Contains(address))
        {
            releaseIconStr.Add(address);
        }

        AddressableManager.Instance.LoadAsset<Sprite>(address, x => ingameUltiSlots.sprite = x);
    }
}