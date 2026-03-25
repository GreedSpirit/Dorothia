using System.Numerics;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfoPopup : BaseUI
{
    private SkillKey key;
    private int _targetIdx = -1;

    [SerializeField] private TextMeshProUGUI title;

    [Header("아이콘 관련")]
    [SerializeField] private SkillItem icon;
    [SerializeField] private TextMeshProUGUI iconSkillLevel;

    [Header("스킬 기본 정보")]
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI levelText; // level 변수명 중복 방지

    [Header("토글 관련")]
    [SerializeField] private ToggleGroup toggles;
    [SerializeField] private Toggle basicToggle;
    [SerializeField] private Toggle gradeToggle;
    [SerializeField] private GameObject basicPanel;
    [SerializeField] private GameObject gradePanel;

    [Header("기본정보 관련")]
    [SerializeField] private TextMeshProUGUI cooldown;
    [SerializeField] private TextMeshProUGUI description;

    [Header("승급정보 관련")]
    [SerializeField] private TextMeshProUGUI[] grades;

    [Header("버튼 관련")]
    [SerializeField] private Button reinforceBtn;
    [SerializeField] private TextMeshProUGUI needsGold;
    [SerializeField] private TextMeshProUGUI currentGold;
    [SerializeField] private Button merge_equip_Btn;
    [SerializeField] private TextMeshProUGUI btnText;
    [SerializeField] private MergeNotificationPopup notification;

    private bool isEquip = false;

    private void Start()
    {
        // 토글 리스너 등록
        basicToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdatePanel(true); });
        gradeToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdatePanel(false); });

        if (ExchangeManager.Instance != null)
        {
            ExchangeManager.Instance.OnGoldChanged += RefreshGoldUI;
        }
    }

    protected override void OnOpen()
    {

    }

    protected override void OnClose()
    {
        if (ExchangeManager.Instance != null)
        {
            ExchangeManager.Instance.OnGoldChanged -= RefreshGoldUI;
        }

        SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);
        if (data != null)
        {
            AddressableManager.Instance.ReleaseAsset(data.Skill_Icon);
        }
    }
    public void Setup(SkillKey key, int targetIdx = -1)
    {
        this.key = key;
        _targetIdx = targetIdx;

        var sm = SkillManager.Instance;
        SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);

        title.text = $"{data.Skill_Name} 정보";

        icon.SetSlotData(SkillItem.SlotType.InfoDetail, key);

        // 주문서 여부에 따른 레벨 및 개수 표시 수정
        //iconSkillLevel.gameObject.SetActive(!key.isScroll);
        levelText.gameObject.SetActive(!key.isScroll);

        if (!key.isScroll)
        {
            // 해금된 스킬 정보 가져오기 (딕셔너리 안전 접근)
            if (sm.UnlockedSkills.TryGetValue(key, out BaseSkill skill))
            {
                //iconSkillLevel.text = skill.Level.ToString();
                levelText.text = skill.Level.ToString();
            }
        }

        // 기본 정보 셋팅
        Color color = RarityColor.GetColor(key.rarity);
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        skillName.text = $"{data.Skill_Name} <color=#{hexColor}><{key.rarity}></color>";
        cooldown.text = $"{data.Skill_Cooltime}s";
        description.text = $"스킬 설명 컬럼이 없음";

        // 승급/랭크 정보 (Skill_RankData)
        var ranks = DataManager.Instance.GetDict<Skill_RankData>();
        for (int i = 0; i < grades.Length; i++)
        {
            if (ranks.TryGetValue(i + 1, out var rankData))
            {
                grades[i].text = $"{rankData.Skill_Rank} : {rankData.Skill_Rank_Multiplier * 100}% 상승";
            }
            else
            {
                grades[i].text = "-";
            }
        }

        // 버튼 상태 제어
        btnText.text = "합성";
        if (!key.isScroll)
        {
            RefreshEquipStatus();
            RefreshReinforceUI();
        }

        // 초기 패널 상태
        basicToggle.SetIsOnWithoutNotify(true);
        UpdatePanel(true);
    }
    private void RefreshReinforceUI()
    {
        var sm = SkillManager.Instance;
        if (!sm.UnlockedSkills.TryGetValue(key, out BaseSkill skill)) return;

        levelText.text = $"{skill.Level}/100";

        BigInteger haveGold = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold);
        currentGold.text = $"{haveGold:N0}G";

        int nextCost = sm.GetReinforceCost(key);
        needsGold.text = $"{nextCost:N0}G";

        bool isMaxLevel = skill.Level >= SkillManager.MAX_LEVEL;
        bool canAfford = haveGold >= nextCost;

        needsGold.color = canAfford ? Color.white : Color.red;

        if (isMaxLevel)
        {
            reinforceBtn.interactable = false;
            needsGold.text = "MAX";
        }
        else
        {
            reinforceBtn.interactable = canAfford;
        }
    }

    private void RefreshGoldUI(BigInteger newGold)
    {
        // 현재 팝업이 열려있고 유효한 키가 있을 때만 갱신
        if (gameObject.activeSelf)
        {
            RefreshReinforceUI();
        }
    }

    private void UpdatePanel(bool isBasic)
    {
        basicPanel.SetActive(isBasic);
        gradePanel.SetActive(!isBasic);
    }

    public void Click_Reinforce()
    {
        var sm = SkillManager.Instance;

        // 강화 시도
        var result = sm.ReinforceSkill(key);

        switch (result)
        {
            case SkillManager.ReinforceResult.Success:
                RefreshReinforceUI(); // 성공 시 UI 갱신
                break;

            case SkillManager.ReinforceResult.NotEnoughGold:
                Debug.Log("골드가 부족합니다.");
                // UIManager.Instance.ShowToast("골드가 부족합니다.");
                break;

            case SkillManager.ReinforceResult.LevelMax:
                Debug.Log("최대 레벨입니다.");
                // UIManager.Instance.ShowToast("최대 레벨입니다.");
                reinforceBtn.interactable = false;
                break;

            case SkillManager.ReinforceResult.NotFound:
                Debug.LogWarning($"스킬 정보를 찾을 수 없습니다. key: {key.sid}");
                break;
        }
    }



    // 버튼 클릭 이벤트 (인스펙터에서 연결하거나 Setup에서 등록)
    public void OnClickMainButton()
    {
        if (key.isScroll)
        {
            notification.Key = key;
            UIManager.Instance.OpenPanel(notification);
        }
        else
        {
            if (isEquip)
            {
                int idx = SkillManager.Instance.GetEquippedIndex(key);

                Skill_Type type = SkillManager.Instance.GetSkill(key).Data.Skill_Type;
                if (type == Skill_Type.Active)
                {

                    SkillManager.Instance.UnequipActive(idx);
                }
                else if (type == Skill_Type.Ultimate)
                {

                    SkillManager.Instance.UnequipUltimate();
                }
                else
                {
                    SkillManager.Instance.UnequipPassive(idx);
                }
            }
            else
            {
                SkillManager.Instance.EquipSkill(key, _targetIdx);
            }
            UIManager.Instance.CloseTopPanel();
        }

        // 데이터 변했으니 UI 갱신
        Setup(key, _targetIdx);
    }
    private void RefreshEquipStatus()
    {
        isEquip = SkillManager.Instance.IsEquipped(key);
        btnText.text = isEquip ? "장착 중" : "장착";
    }

}