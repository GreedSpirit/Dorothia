using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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

    [Header("UI References")]
    [SerializeField] private Transform scrollInven;
    [SerializeField] private Transform skillInven;
    [SerializeField] private GameObject skillItemPrefab;

    // 슬롯 관리 (PlayerCtrl이 참조할 실시간 리스트)
    public List<ActiveSkill> ActiveSkillSlots { get; private set; } = new List<ActiveSkill>();
    public List<PassiveSkill> PassiveSkillSlots { get; private set; } = new List<PassiveSkill>();

    // 데이터 저장소 (Key: SkillKey, Value: 보유 수량)
    private Dictionary<SkillKey, int> _inventory = new Dictionary<SkillKey, int>();

    // 해금된 실제 스킬 객체들 (Logic 인스턴스)
    public IReadOnlyDictionary<SkillKey, BaseSkill> UnlockedSkills => _unlockedSkills;
    private Dictionary<SkillKey, BaseSkill> _unlockedSkills = new Dictionary<SkillKey, BaseSkill>();

    // UI 아이템 캐싱
    private Dictionary<SkillKey, SkillItem> _uiCache = new Dictionary<SkillKey, SkillItem>();

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
            GetRandomScroll();
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
            CreateUIItem(key); // 처음 얻을 때만 UI 생성
        }

        _inventory[key] += count;
        //Debug.Log($"{key.sid}, {_inventory[key]}");
        OnInventoryChanged?.Invoke(key, _inventory[key]);
    }

    private void CreateUIItem(SkillKey key)
    {
        // 주문서면 주문서 인벤, 스킬이면 스킬 인벤에 생성
        Transform parent = key.isScroll ? scrollInven : skillInven;
        GameObject go = Instantiate(skillItemPrefab, parent);

        if (go.TryGetComponent(out SkillItem item))
        {
            item.Setup(key);
            _uiCache[key] = item;
        }
    }

    public int GetItemCount(SkillKey key) => _inventory.GetValueOrDefault(key, 0);

    #endregion

    #region Craft & Merge

    // 주문서 3개 -> 노말 스킬 1개
    public void CraftSkill(SkillKey scrollKey)
    {
        if (!scrollKey.isScroll || GetItemCount(scrollKey) < 3) return;

        int craftCount = _inventory[scrollKey] / 3;
        _inventory[scrollKey] %= 3;
        OnInventoryChanged?.Invoke(scrollKey, _inventory[scrollKey]);

        // 주문서와 같은 ID의 'Normal' 등급 스킬 생성
        SkillKey skillKey = new SkillKey(scrollKey.sid, Rarity.Normal, false);

        UnlockAndAddItem(skillKey, craftCount);
    }

    // 같은 등급 스킬 3개 -> 다음 등급 스킬 1개
    public void MergeSkill(SkillKey skillKey)
    {
        // 스킬이어야 하고, 최고 등급이 아니어야 하며, 3개 이상 보유해야 함
        if (skillKey.isScroll || skillKey.rarity == Rarity.Legendary || GetItemCount(skillKey) < 3) return;

        int upgradeCount = _inventory[skillKey] / 3;
        _inventory[skillKey] %= 3;
        OnInventoryChanged?.Invoke(skillKey, _inventory[skillKey]);

        // 다음 등급 결정
        Rarity nextRarity = (Rarity)((int)skillKey.rarity + 1);
        SkillKey upgradedKey = new SkillKey(skillKey.sid, nextRarity, false);

        Debug.Log($"{skillKey.sid} 스킬 {skillKey.rarity} -> {nextRarity} 등급업!");
        UnlockAndAddItem(upgradedKey, upgradeCount);
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
        // Dictionary 수정 중 반복문을 돌면 에러가 나므로 리스트로 복사해서 사용
        var keys = _inventory.Keys.ToList();

        foreach (var key in keys)
        {
            if (key.isScroll)
                CraftSkill(key);
            else
                MergeSkill(key);
        }
    }

    #endregion

    #region Slot Management

    // 플레이어가 스킬을 장착할 때 호출
    public void EquipSkill(SkillKey key)
    {
        if (!_unlockedSkills.TryGetValue(key, out BaseSkill skill)) return;

        if (skill is ActiveSkill active && !ActiveSkillSlots.Contains(active))
        {
            if (ActiveSkillSlots.Count < 5) // 최대 슬롯 제한
            {
                ActiveSkillSlots.Add(active);
                Debug.Log($"{active.Data.Skill_Name} 장착 완료");
            }
        }
        else if (skill is PassiveSkill passive && !PassiveSkillSlots.Contains(passive))
        {
            PassiveSkillSlots.Add(passive);
            passive.Execute(); // 패시브는 장착 즉시 효과 적용 (Undo는 해제 시)
        }
    }

    public void UnequipSkill(ActiveSkill skill)
    {
        if (ActiveSkillSlots.Contains(skill))
            ActiveSkillSlots.Remove(skill);
    }

    #endregion
}