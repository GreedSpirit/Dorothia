using UnityEngine;

public class EquipSkillSlot : BaseSkillSlot
{
    public override void Click_Slot()
    {
        if (IsEquip)
        {
            if (_skill == null) return;

            if (!_skill.IsReady) return;

            if (_skill.Data.Skill_Type == Skill_Type.Passive) return;

            player.PerformSkill(_skill);
        }
        else
        {
            if (listPanel == null) return;

            listPanel.SetOpenType(slotType, slotIndex);
            UIManager.Instance.OpenPanel(listPanel);
        }
    }
}