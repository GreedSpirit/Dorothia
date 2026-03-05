using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("UI 관련")]
    [SerializeField] private Transform scrollInven;
    [SerializeField] private Transform skillInven;
    [SerializeField] private GameObject skillItemPrefab;

    // 패시브 스킬 슬롯
    private const int PASSIVESLOT_COUNT = 5;
    public List<PassiveSkill> passiveSkillSlots = new List<PassiveSkill>();

    // 액티브 스킬 슬롯
    //private const int ACTIVESLOT_COUNT = 5;
    public List<BaseSkill> activeSkillSlots = new List<BaseSkill>();

    // 순수 데이터
    public Dictionary<SkillKey, int> scrollCounts = new Dictionary<SkillKey, int>();
    public Dictionary<SkillKey, int> skillCounts = new Dictionary<SkillKey, int>();

    public IReadOnlyDictionary<SkillKey, BaseSkill> UnlockedSkills => unlockedSkills;
    private Dictionary<SkillKey, BaseSkill> unlockedSkills = new Dictionary<SkillKey, BaseSkill>();

    private Dictionary<Rarity, float> rarityGauges = new Dictionary<Rarity, float>();

    // UI 참조 저장소
    private Dictionary<SkillKey, SkillItem> scrollUI = new Dictionary<SkillKey, SkillItem>();
    private Dictionary<SkillKey, SkillItem> skillUI = new Dictionary<SkillKey, SkillItem>();

    public event Action<SkillKey, int> OnItemCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame) TestGetScrolls();
    }


    public void AddScroll(SkillKey key, int count = 1)
    {
        if (!key.isScroll) return; 

        // 데이터 업데이트
        if (scrollCounts.ContainsKey(key))
        {
            scrollCounts[key] += count;
        }
        else
        {
            scrollCounts[key] = count;

            GameObject go = Instantiate(skillItemPrefab, scrollInven);
            SkillItem newItem = go.GetComponent<SkillItem>();
            newItem.Setup(key);
            skillUI.Add(key, newItem);
        }

        // UI 업데이트
        //UpdateUI(key, scrollCounts[key], scrollInven, scrollUI);

        OnItemCountChanged?.Invoke(key, scrollCounts[key]);
    }

    public void AddSkill(SkillKey key, int count = 1)
    {
        if (key.isScroll) return;

        if (skillCounts.ContainsKey(key))
        {
            skillCounts[key] += count;
        }
        else
        {
            skillCounts[key] = count;
        }

        OnItemCountChanged?.Invoke(key, skillCounts[key]);
    }


    #region 합성 및 강화

    public void CraftSkill(SkillKey scrollKey)
    {
        if (!scrollCounts.ContainsKey(scrollKey) || scrollCounts[scrollKey] < 3) return;

        // 1. 주문서 소모
        int craftQty = scrollCounts[scrollKey] / 3;
        scrollCounts[scrollKey] %= 3;

        // 주문서 UI 갱신 (0개면 삭제 로직 추가 가능)
        //UpdateUI(scrollKey, scrollCounts[scrollKey], scrollInven, scrollUIs);

        // 2. 스킬 생성 데이터 처리
        SkillKey skillKey = new SkillKey(scrollKey.sid, scrollKey.rarity, false);

        if (!unlockedSkills.ContainsKey(skillKey))
        {
            var sData = DataManager.Instance.GetData<SkillData>(skillKey.sid);
            var stData = DataManager.Instance.GetData<Skill_StatusData>(sData.Skill_Status_Id);
            unlockedSkills[skillKey] = BaseSkill.Create(sData, stData);
        }

        // 3. 스킬 인벤토리에 추가
        AddSkill(skillKey, craftQty);
    }

    public void AllScrollMerge()
    {
        // Dictionary를 순회하며 수정해야 하므로 키 리스트 복사 후 사용
        List<SkillKey> keys = new List<SkillKey>(scrollCounts.Keys);
        foreach (var key in keys)
        {
            if (scrollCounts[key] >= 3) CraftSkill(key);
        }
    }

    public void Reinforce(){
        //장비 하위에 강화 소모 제화 테이블에서 값을 받음
    }
    #endregion

    #region 테스트용
    public void TestGetScrolls()
    {
        foreach (var data in DataManager.Instance.GetDict<SkillData>())
        {
            SkillKey key = new SkillKey(data.Value.Job_Skill_Id, Rarity.Normal, true);
            AddScroll(key, 1);
        }
    }
    #endregion
}
