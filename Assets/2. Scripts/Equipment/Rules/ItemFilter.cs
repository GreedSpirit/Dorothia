using System.Collections.Generic;
using UnityEngine;

public static class ItemFilter
{
    public static List<Equipment> FindTargetEquipment(List<Equipment> inventory, bool Upgraded, bool Normal, bool Uncommon, bool Rare, bool Legendary, bool Mythtic)
    {
        Debug.Log($"{Upgraded},{Normal},{Uncommon},{Rare},{Legendary},{Mythtic}");
        List<Equipment> target = new List<Equipment>();
        foreach (Equipment equip in inventory)
        {
            if(equip.isEquipped == true)
            {
                Debug.Log("장착된 장비는 적용 안됨!");
                continue;
            }
            if(equip.isLocked == true)
            {
                Debug.Log("잠금 상태의 장비는 적용 안됨!");
                continue;
            }
            if (Upgraded == false && equip.equip_Upgrade != 0)
            {
                Debug.Log("업그레이드된 장비는 불가능!");
                continue;
            }    
            if(Normal == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Normal)
            {
                Debug.Log("노말 장비는 적용 안됨!");
                continue;
            }
            if(Uncommon == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Uncommon)
            {
                Debug.Log("희귀 장비는 적용 안됨!");
                continue;
            }
            if(Rare == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Rare)
            {
                Debug.Log("레어 장비는 적용 안됨!");
                continue;
            }
            if(Legendary == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Legendary)
            {
                Debug.Log("전설 장비는 적용 안됨!");
                continue;
            }
            if(Mythtic == false && DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank == Rarity.Mythtic)
            {
                Debug.Log("신화 장비는 적용 안됨!");
                continue;
            }
            target.Add(equip);
        }
        return target;
    }
}
