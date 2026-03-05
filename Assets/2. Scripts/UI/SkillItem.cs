using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillItem : MonoBehaviour
{
    private SkillKey key;

    //이벤트 기반으로 변경하기
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillCountText;

    [SerializeField] private SkillInfoPopup SkillInfo;

    private SkillPanel skillPanel;
    private Button button;

    private void Awake()
    {
        skillPanel = GetComponentInParent<SkillPanel>();

        button = GetComponent<Button>();
        button.onClick.AddListener(Click_SkillInfo);
    }

    public void Setup(SkillKey key)
    {
        this.key = key;

        var data = DataManager.Instance.GetData<SkillData>(key.sid);

        skillNameText.text = data.Skill_Name;
        skillCountText.text = 0.ToString();

        if (SkillManager.Instance != null)
            SkillManager.Instance.OnItemCountChanged += UpdateUI;
    }

    private void UpdateUI(SkillKey key, int count)
    {
        skillCountText.text = $"{count} / 3";
    }

    private void Click_SkillInfo()
    {
        skillPanel.Click_SkillInfo(key);
    }
}
