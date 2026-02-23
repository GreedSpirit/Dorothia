using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    static private SkillManager instance;
    static public SkillManager Instance { get => instance; set => instance = value; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    // 패시브 스킬 슬롯
    private const int PASSIVESLOT_COUNT = 5;
    public List<PassiveSkill> passiveSkillSlots = new List<PassiveSkill>();

    // 액티브 스킬 슬롯
    //private const int ACTIVESLOT_COUNT = 5;
    public List<BaseSkill> activeSkillSlots = new List<BaseSkill>();

    // 스킬 관리
    private Dictionary<int, BaseSkill> unlockedSkill = new Dictionary<int, BaseSkill>();
    private Dictionary<int, int> skills = new Dictionary<int, int>();

    // 스킬주문서 관리 (스킬id,개수)
    [SerializeField] private Transform inven;
    [SerializeField] private GameObject scrollItem;
    public Dictionary<int, int> scrolls = new Dictionary<int, int>();
    public Dictionary<int, SkillItem> items = new Dictionary<int, SkillItem>();

    // 등급별 신비 게이지
    private Dictionary<Rarity, float> rarityGauge = new Dictionary<Rarity, float>();

    public event Action<int> OnAddScroll;

    // 1. 특수 던전에서 클리어 시 스킬주문서 획득
    // 2. 상점에서 골드로 스킬주문서 획득
    public void TestGetScrolls()
    {

        foreach (var skill in DataManager.Instance.GetDict<SkillData>())
        {
            Debug.Log($"{skill.Value.Job_Skill_Id}\n" +
            $"스킬명 : {skill.Value.Skill_Name}\n" +
            $"타입 : {skill.Value.Skill_Type}\n" +
            $"쿨타임 : {skill.Value.Skill_Cooltime}\n" +
            $"statusId : {skill.Value.Skill_Status_Id}\n");

            Skill_StatusData status = DataManager.Instance.GetData<Skill_StatusData>(skill.Value.Skill_Status_Id);

            Debug.Log($"type : {status.Affection_Skill}\n" +
           $"increaseValue : {status.Affection_Skill_Value}");

            AddScroll(skill.Value.Job_Skill_Id);
        }
    }


    #region 스킬 주문서 관련
    public void AddScroll(int sId, int count = 1)
    {

        if (scrolls.ContainsKey(sId))
        {
            scrolls[sId] += count;
        }
        else
        {
            scrolls[sId] = count;

            GameObject scroll = Instantiate(scrollItem, inven);

            SkillData data = DataManager.Instance.GetData<SkillData>(sId);

            SkillItem item = scroll.GetComponent<SkillItem>();
            item.Setup(data);
            items[sId] = item;
        }
        OnAddScroll?.Invoke(sId);
    }


    // 스킬주문서 3개 -> 스킬 1개
    public void CraftSkill(int sId)
    {
        // 혹시 모를 예외 방지
        if (!scrolls.ContainsKey(sId) || scrolls[sId] < 3) return;

        int result = scrolls[sId] / 3;

        scrolls[sId] %= 3;

        // 데이터 가져오기
        SkillData skillData = DataManager.Instance.GetData<SkillData>(sId);
        Skill_StatusData statusData = DataManager.Instance.GetData<Skill_StatusData>(skillData.Skill_Status_Id);

        // 스킬 데이터 생성
        if (!unlockedSkill.TryGetValue(sId, out BaseSkill skill))
        {

            skill = BaseSkill.Create(skillData, statusData);
            unlockedSkill[sId] = skill;
        }

        Debug.Log($"{sId}스킬 {result}개 생성 완료. \n 해당 주문서 남은 개수 {scrolls[sId]}");

        AddSkill(skill, result);
    }

    public void SelectScrollMerge(List<int> sids)
    {

    }

    //일괄적으로합성 가능한스킬들을합성함.
    public void AllScrollMerge()
    {
        foreach (var scroll in scrolls)
        {
            int count = scroll.Value;
            if (count < 3) continue;

            CraftSkill(scroll.Key);
        }
    }
    #endregion

    #region 스킬 관련

    public void AddSkill(BaseSkill skill, int count = 1)
    {
        int sId = skill.Data.Job_Skill_Id;

        if (skills.ContainsKey(sId))
        {
            skills[sId] += count;
        }
        else
        {
            skills[sId] = 1;
        }
    }
    public void SkillMerge(int sId)
    {
        //같은 스킬, 같은 등급 3개로 레어리티 1향상
        BaseSkill skill = unlockedSkill[sId];

        Rarity rarity = skill.Rarity;

        Skill_RankData rankData = DataManager.Instance.GetData<Skill_RankData>((int)rarity);

        //상승값
        float increaseVal = rankData.Skill_Value;
        //실패 보정치 = 신비게이지 상승 값
        float failureVal = rankData.Skill_Rank_Failure;
        //성공확률
        float perSuccess = rankData.Skill_Success_Prob;

        //신비게이지 확인
        rarityGauge[rarity] += failureVal;

        skills[sId] -= 3;
    }

    public void SkillUpgrade(int sId)
    {
        BaseSkill skill = unlockedSkill[sId];

        Skill_UpgradeData upgradeData = DataManager.Instance.GetData<Skill_UpgradeData>(skill.Level);


        //스킬레벨 1업

        //100레벨이상 불가
    }
    #endregion


    /*
     합성 실패 시 게이지가차오름.

     합성 10000번에 한 번씩 게이지가완전히충전됨.
     게이지를최대로채울 시 100% 강화 성공
     게이지양에 따라 1~100% 확률이존재함
    3개의 같은 등급, 같은 스킬 합성 시 상위 등급 스킬 획득 가능.
 스킬 합성 시에 높은 등급으로갈수록합성 성공 확률.
➢ 일반:100% 희귀: 50% 레어: 20% 전설: 10% ->신화: 1%
➢ 만약 스킬이 강화된 스킬을 재료로 합성 시도하면 최대 10% 보정확률 추가.
✓ + 50강화 = 10%

 합성 실패 시 ‘신비 게이지(가칭)’ 상승하여게이지가한계치에도달하면100% 업그레이드.
 등급에따라 게이지의천장이달라짐. (등급이낮을수록신비 게이지천장이낮음.)
    .*/

    // 등급, 스킬(%) 순으로 자동 편성
    public void AutomaticSkillFormation_Passive()
    {

    }

    public void AutomaticSkillFormation_Active()
    {

    }


    //// 스킬 장착
    //public void AttachSkill(BaseSkill skill)
    //{
    //    // 액티브는 플레이어에서 실행
    //    // 패시브는 바로 실행
    //    if (skill.Data.Skill_Type == Skill_Type.Passive)
    //    {
    //        skill.Execute();

    //        if (!passiveSkillSlots.Contains(skill) && passiveSkillSlots.Count < PASSIVESLOT_COUNT)
    //        {
    //            passiveSkillSlots.Add(skill);
    //        }
    //    }
    //    else
    //    {
    //        if (!activeSkillSlots.Contains(skill))
    //        {
    //            activeSkillSlots.Add(skill);
    //        }
    //    }
    //}

    //public void DettachSkill(BaseSkill skill)
    //{
    //    if (skill.Data.Skill_Type == Skill_Type.Passive)
    //    {
    //        if (passiveSkillSlots.Contains(skill))
    //        {
    //            skill.Undo();
    //            passiveSkillSlots.Remove(skill);
    //        }
    //    }
    //    else
    //    {
    //        if (activeSkillSlots.Contains(skill))
    //        {
    //            activeSkillSlots.Remove(skill);
    //        }
    //    }
    //}

}
