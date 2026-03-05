using System.Linq;
using TMPro;
using UnityEngine;
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
    [SerializeField] private TextMeshProUGUI level;

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

    private void Awake()
    {
        basicToggle.onValueChanged.AddListener(ToggleActiveCheck);
        gradeToggle.onValueChanged.AddListener(ToggleActiveCheck);
    }

    public void Setup(SkillKey key)
    {
        this.key = key;
        SkillData data = DataManager.Instance.GetData<SkillData>(key.sid);

        title.text = $"{data.Skill_Name} 정보";

        //icon = Resources.Load();
        iconSkillLevel.enabled = (!key.isScroll);
        if (!key.isScroll)
        {
            iconSkillLevel.text = $"{SkillManager.Instance.UnlockedSkills[key].Level}";
            skillCount.text = $"{SkillManager.Instance.skillCounts[key]}/3";
        }

        skillName.text = $"{data.Skill_Name}<{key.rarity}>";

        level.enabled = (!key.isScroll);
        if (!key.isScroll)
        {
            level.text = $"{SkillManager.Instance.UnlockedSkills[key].Level}";
        }

        cooldown.text = $"{data.Skill_Cooltime}";
        description.text = $"스킬설명";

        var ranks = DataManager.Instance.GetDict<Skill_RankData>();
        for (int i = 0; i < ranks.Count; i++)
        {
            grades[i].text = $"{ranks[i + 1].Skill_Rank} : {ranks[i + 1].Skill_Value}% 상승";
        }

        reinforceBtn.enabled = (!key.isScroll);
        needsGold.text = $"데이터 없음";

        merge_equip_Btn.enabled = (!key.isScroll);
        btnText.text = key.isScroll ? "합성" : "강화";

        basicToggle.isOn = true;
        gradeToggle.isOn = false;

        basicPanel.SetActive(true);
        gradePanel.SetActive(false);
    }

    public void ToggleActiveCheck(bool isOn)
    {
        if (isOn) return;

        Toggle activeToggle = toggles.ActiveToggles().FirstOrDefault();

        if (activeToggle != null)
        {
            bool isBasic = (activeToggle == basicToggle);

            basicPanel.SetActive(isBasic);
            gradePanel.SetActive(!isBasic);
        }
    }
}
