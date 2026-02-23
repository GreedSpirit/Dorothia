using TMPro;
using UnityEngine;

public class SkillItem : MonoBehaviour
{
    int scrollId;

    //이벤트 기반으로 변경하기
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillCountText;

    public void Setup(SkillData data)
    {
        scrollId = data.Job_Skill_Id;
        skillNameText.text = data.Skill_Name;
        skillCountText.text = 0.ToString();

        if (SkillManager.Instance != null)
            SkillManager.Instance.OnAddScroll += UpdateUI;
    }

    private void UpdateUI(int sid)
    {
        if (SkillManager.Instance.scrolls.ContainsKey(sid))
        {
            skillCountText.text = SkillManager.Instance.scrolls[sid].ToString();
        }
    }

}
