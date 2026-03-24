using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : BaseUI
{
    [SerializeField] private SkillInfoPopup skillInfo;

    protected override void OnOpen()
    {
    }

    protected override void OnClose()
    {
    }
    
    public void Click_SkillInfo(SkillKey key, int targetIdx=-1)
    {
        skillInfo.Setup(key,targetIdx);
        UIManager.Instance.OpenPanel(skillInfo);
    }

    public void AutoEquip(){
        SkillManager.Instance.AutoEquip();
    }
}