using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.EventSystems;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SkillMergePanel : BaseUI
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject skillItemPrefab;

    [Header("신비게이지")]
    [SerializeField] private Slider mysteryGauge;
    [SerializeField] private Toggle mysteryToggle;
    [SerializeField] private TextMeshProUGUI gaugeValue;

    [SerializeField] private Button upgradeButton;

    [Header("업그레이드 결과 창 패널")]
    [SerializeField] private SkillMergeResultPanel resultPanel;

    private List<SkillItem> _pool = new List<SkillItem>();
    private SkillKey currentKey;
    private SkillItem currentItem;

    protected override void OnOpen()
    {
        SkillManager.Instance.OnMysteryGaugeChanged += UpdateMysteryGauge;

        RefreshUI();
    }

    protected override void OnClose()
    {
        SkillManager.Instance.OnMysteryGaugeChanged -= UpdateMysteryGauge;

        upgradeButton.interactable = false;
    }

    private void UpdateMysteryGauge(float gauge)
    {
        gaugeValue.text = $"{gauge}/{SkillManager.MYSTERY_LIMIT}";
        mysteryGauge.value = gauge / SkillManager.MYSTERY_LIMIT;
    }

    private void RefreshUI()
    {
        foreach (var item in _pool)
        {
            item.gameObject.SetActive(false);
        }

        var targetSkills = SkillManager.Instance.Inventory
            .Where(x => x.Key.isScroll == false && x.Value >= 3)
            .ToList();

        for (int i = 0; i < targetSkills.Count; i++)
        {
            SkillItem item = GetOrCreateItem(i);
            SkillKey key = targetSkills[i].Key;

            item.SetSlotData(SkillItem.SlotType.Skill, key, -1, SkillItem.DisplayMode.Selection, SelectSKill);

            item.gameObject.SetActive(true);

            // 여기에 아이템의 텍스트나 아이콘을 설정하는 컴포넌트 호출 로직 추가
            // item.GetComponent<SkillItemUI>().SetData(targetSkills[i].Key, targetSkills[i].Value);
        }
    }

    private void SelectSKill(SkillKey key, SkillItem item)
    {
        // 같은 아이템이라면
        if (currentItem == item)
        {
            upgradeButton.interactable = false;
            currentItem.gradeOutlineImage.sprite = SkillManager.Instance.GetSpriteByGrade(key.rarity);
            currentItem = null;
            return;
        }

        // 다른 아이템이라면
        if (currentItem != null)
        {
            // 원래 등급에 맞는 스프라이트로 되돌림
            currentItem.gradeOutlineImage.sprite = SkillManager.Instance.GetSpriteByGrade(currentKey.rarity);
        }

        // 새로운 아이템 선택 적용
        currentKey = key;
        currentItem = item;

        // 새 선택 UI 연출
        currentItem.gradeOutlineImage.sprite = SkillManager.Instance.pickSprite;
        upgradeButton.interactable = true;
    }

    private SkillItem GetOrCreateItem(int index)
    {
        // 풀에 모자라면 새로 생성
        if (index >= _pool.Count)
        {
            GameObject newObj = Instantiate(skillItemPrefab, contentParent);
            SkillItem item = newObj.GetComponent<SkillItem>();

            _pool.Add(item);
            return item;
        }
        return _pool[index];
    }

    public void Click_UpgradeSkill()
    {
        if (currentKey != null)
        {
            // 1. SkillManager에서 합성 로직 실행 및 결과 데이터 수신
            // (SkillManager의 MergeSkill이 (bool 성공여부, SkillKey 결과템)을 반환한다고 가정)
            var result = SkillManager.Instance.MergeSkill(currentKey, mysteryToggle.isOn);

            // 2. 결과창 설정 (result.isSuccess와 result.rewardKey는 예시입니다)
            resultPanel.Setup(result, currentKey);

            // 3. 결과창 열기
            UIManager.Instance.OpenPanel(resultPanel);

            // 4. 합성 후 메인 패널 UI 갱신 (보유 수량이 변했으므로)
            RefreshUI();
            upgradeButton.interactable = false;
        }
    }


}