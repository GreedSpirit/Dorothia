using System.Collections.Generic;
using UnityEngine;

public static class ItemFilter
{
    public static List<Equipment> FindTargetEquipment(List<Equipment> inventory, bool Upgraded, bool Normal, bool Uncommon, bool Rare, bool Legendary, bool Mythtic)
    {
        List<Equipment> target = new List<Equipment>();
        foreach (Equipment equip in inventory)
        {
            if(equip.isEquipped == true)
            {
                continue;
            }
            if(equip.isLocked == true)
            {
                continue;
            }
            if (Upgraded == false && equip.equip_Upgrade != 0)
            {
                continue;
            }    
            if(Normal == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Normal)
            {
                continue;
            }
            if(Uncommon == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Uncommon)
            {
                continue;
            }
            if(Rare == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Rare)
            {
                continue;
            }
            if(Legendary == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Legendary)
            {
                continue;
            }
            if(Mythtic == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Mythtic)
            {
                continue;
            }
            target.Add(equip);
        }
        return target;
    }
}
