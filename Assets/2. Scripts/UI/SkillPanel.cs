using TMPro;
using UnityEngine;

public class SkillPanel : BaseUI
{
    [SerializeField] private SkillInfoPopup skillInfo;

    protected override void OnOpen()
    {
        //창이 처음 열렸을 때 세팅해줘야될것들 구현
    }
    protected override void OnClose()
    {
        //창을 닫았을 떄 저장해야할 것들 구현
    }

    public void Click_SkillInfo(SkillKey key)
    {
        skillInfo.Setup(key);
        skillInfo.gameObject.SetActive(true);
    }
}
