using UnityEngine;

public class EquipSkillSlot : BaseSkillSlot
{
    public override void Click_Slot()
    {
        if (IsEquip)
        {
            if (_skill == null) return;
        
            if (_skill?.Data.Skill_Type == Skill_Type.Passive) return;

            if (_skill.IsReady)
            {
                _skill?.Execute(player);
                _skill?.StartCooldown();
            }
        }
        else
        {
            if (listPanel == null) return;

            listPanel.SetOpenType(slotType, slotIndex);
            UIManager.Instance.OpenPanel(listPanel);
        }
    }
}