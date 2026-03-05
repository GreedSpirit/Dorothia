using UnityEngine;
using UnityEngine.UI;

public class SkillListPanel : BaseUI
{
    [SerializeField] private Button activeBtn;
    [SerializeField] private Button passiveBtn;
    [SerializeField] private Button ultimateBtn;

    private void Awake()
    {
        activeBtn.onClick.AddListener(()=>UpdateSkillItem(Skill_Type.Active));
        passiveBtn.onClick.AddListener(() => UpdateSkillItem(Skill_Type.Passive));
        ultimateBtn.onClick.AddListener(() => UpdateSkillItem(Skill_Type.Active));
    }

    protected override void OnClose()
    {
    }

    protected override void OnOpen()
    {
    }

    private void UpdateSkillItem(Skill_Type type)
    {
        
    }
}
