using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SkillListPanel : BaseUI
{
    [SerializeField] private Button activeBtn;
    [SerializeField] private Button passiveBtn;
    [SerializeField] private Button ultimateBtn;

    [Header("UI Layout")]
    [SerializeField] private Transform contentParent; // 아이템이 생성될 위치
    [SerializeField] private GameObject skillItemPrefab; // 스킬 아이템 프리팹

    // 풀링을 위한 리스트
    private List<SkillItem> _pool = new List<SkillItem>();

    private Skill_Type openType = Skill_Type.Active;

    private void Start()
    {
        activeBtn.onClick.AddListener(() => UpdateSkillItem(Skill_Type.Active));
        passiveBtn.onClick.AddListener(() => UpdateSkillItem(Skill_Type.Passive));
        ultimateBtn.onClick.AddListener(() => UpdateSkillItem(Skill_Type.Ultimate));
    }

    public void SetOpenType(Skill_Type type) => openType = type;

    protected override void OnOpen()
    {
        // 처음 열릴 때 기본적으로 액티브 스킬을 보여줌
        UpdateSkillItem(openType);
    }

    private void UpdateSkillItem(Skill_Type type)
    {
        // 1. 현재 풀에 있는 모든 아이템 비활성화
        foreach (var item in _pool)
        {
            item.gameObject.SetActive(false);
        }

        // 2. 조건에 맞는 스킬 데이터 필터링
        // (Inventory의 Value가 개수라고 가정할 때, 3개 이상인 조건 등을 추가할 수 있습니다)
        var filteredSkills = SkillManager.Instance.UnlockedSkills
            .Where(x => x.Value.Data.Skill_Type == type)
            .ToList();

        // 3. 필터링된 데이터만큼 UI 배치 (풀 재사용)
        for (int i = 0; i < filteredSkills.Count; i++)
        {
            SkillItem itemUI = GetOrCreateItem(i);
            itemUI.gameObject.SetActive(true);

            // 데이터 설정 (아이콘, 이름, 개수 등)
            var skillData = filteredSkills[i].Key;
            var skillCount = filteredSkills[i].Value;
            itemUI.Setup(skillData, SkillItem.DisplayMode.Info, OnSkillSelected);
        }
    }

    private SkillItem GetOrCreateItem(int index)
    {
        if (index >= _pool.Count)
        {
            GameObject newObj = Instantiate(skillItemPrefab, contentParent);
            SkillItem uiScript = newObj.GetComponent<SkillItem>();
            _pool.Add(uiScript);
            return uiScript;
        }
        return _pool[index];
    }

    private void OnSkillSelected(SkillKey key, SkillItem item)
    {
        // 스킬 선택 시 처리할 로직 (앞서 구현한 선택 상태 관리 로직과 연결)
        Debug.Log($"선택된 스킬: {key.sid}");
    }

    protected override void OnClose() { }
}