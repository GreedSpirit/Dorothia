using System.Collections.Specialized;
using System.Linq;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class SkillInfoPopup : MonoBehaviour
{
    private SkillKey key;

    [SerializeField] private TextMeshProUGUI title;

    [Header("아이콘 관련")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI iconSkillLevel;
    [SerializeField] private TextMeshProUGUI skillCount;

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
    [SerializeField] private GameObject notification;

    private void Awake()
    {
        // 토글 리스너 등록
        basicToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdatePanel(true); });
        gradeToggle.onValueChanged.AddListener((isOn) => { if (isOn) UpdatePanel(false); });
    }

    private void OnDisable()
    {
        SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);
        AddressableManager.Instance.ReleaseAsset(data.Skill_Icon);
    }

    public void Setup(SkillKey key)
    {
        this.key = key;
        var sm = SkillManager.Instance;
        SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);

        title.text = $"{data.Skill_Name} 정보";

        Debug.Log(key.sid);
        Debug.Log(key.isScroll);

        AddressableManager.Instance.LoadAsset<Sprite>(data.Skill_Icon, (sprite) =>
        {
            icon.sprite = sprite;
        });

        // 주문서 여부에 따른 레벨 및 개수 표시 수정
        //iconSkillLevel.gameObject.SetActive(!key.isScroll);
        levelText.gameObject.SetActive(!key.isScroll);

        // 현재 아이템 개수 가져오기 (리팩토링된 GetItemCount 사용)
        int currentCount = sm.GetItemCount(key);
        skillCount.text = $"{currentCount}/3";

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
        skillName.text = $"{data.Skill_Name} <color={RarityColor.GetColor(key.rarity)}><{key.rarity}></color>";
        cooldown.text = $"{data.Skill_Cooltime}s";
        description.text = $"스킬 설명 컬럼이 없음"; 

        // 승급/랭크 정보 (Skill_RankData)
        var ranks = DataManager.Instance.GetDict<Skill_RankData>();
        for (int i = 0; i < grades.Length; i++)
        {
            if (ranks.TryGetValue(i + 1, out var rankData))
            {
                grades[i].text = $"{rankData.Skill_Rank} : {rankData.Skill_Value}% 상승";
            }
            else
            {
                grades[i].text = "-";
            }
        }

        // 버튼 상태 제어
        // 주문서면 '합성' 버튼 활성, 스킬이면 '강화' 버튼 활성 로프
        btnText.text = key.isScroll ? "합성" : "강화";

        // 골드 관련 데이터 연결 (임시)
        needsGold.text = "1,000";
        currentGold.text = "보유 골드";

        // 초기 패널 상태
        basicToggle.SetIsOnWithoutNotify(true);
        UpdatePanel(true);
    }

    private void UpdatePanel(bool isBasic)
    {
        basicPanel.SetActive(isBasic);
        gradePanel.SetActive(!isBasic);
    }

    // 버튼 클릭 이벤트 (인스펙터에서 연결하거나 Setup에서 등록)
    public void OnClickMainButton()
    {
        if (key.isScroll)
        {
            notification.SetActive(true);
            SkillManager.Instance.CraftSkill(key);
        }
        else
        {
            // SkillManager에 구현할 Reinforce 호출
            // SkillManager.Instance.Reinforce(key);
        }

        // 데이터 변했으니 UI 갱신
        Setup(key);
    }
}