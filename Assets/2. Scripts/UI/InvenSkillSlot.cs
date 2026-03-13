using UnityEngine;

public class InvenSkillSlot : BaseSkillSlot
{
    [SerializeField] private SkillInfoPopup infoPopup; // 스킬 상세 정보 팝업
    public override void Click_Slot()
    {
        if (IsEquip)
        {
            // 스킬이 장착되어 있으면 상세 정보 팝업 띄우기
            if (infoPopup == null) return;

            if (_skill == null) return;

            SkillKey key = new SkillKey(_skill.Data.Job_Skill_Id,_skill.Data.Skill_Type, _skill.Rarity);
            infoPopup.Setup(key,slotIndex);

            UIManager.Instance.OpenPanel(infoPopup);
        }
        else
        {
            // 비어있으면 스킬 리스트 패널 오픈 (장착을 위해)
            if (listPanel == null) return;
            listPanel.SetOpenType(slotType, slotIndex);
            UIManager.Instance.OpenPanel(listPanel);
        }
    }
}