using UnityEngine;

public class EquipmentUpgrade
{
    public int equip_Upgrade;
    public string equip_Upgrade_Section;
    public float equip_Success_prob;
    public float equip_Value;
    public float equip_Upgrade_Failure;

    public EquipmentUpgrade(Equip_UpgradeData equip_UpgradeData)
    {
        equip_Upgrade = equip_UpgradeData.Equip_Upgrade;
        equip_Upgrade_Section = equip_UpgradeData.Equip_Upgrade_Section;
        equip_Success_prob = equip_UpgradeData.Equip_Success_Prob;
        equip_Value = equip_UpgradeData .Equip_Value;
        equip_Upgrade_Failure = equip_UpgradeData.Equip_Upgrade_Failure == 0? 0.1f : equip_UpgradeData.Equip_Upgrade_Failure;
    }
}
