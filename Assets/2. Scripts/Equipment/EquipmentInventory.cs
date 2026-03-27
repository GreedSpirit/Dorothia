using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory : MonoBehaviour, ISaveable<InventorySaveData>
{
    public static EquipmentInventory Instance;
    Dictionary<Equip_Type, List<Equipment>> invDic = new Dictionary<Equip_Type, List<Equipment>>(); // 장착 부위에 맞는 인벤토리를 담을 Dictionary

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //각 장착 부위마다 새롭게 인벤토리를 지정해줍니다.
        foreach(Equip_Type part in System.Enum.GetValues(typeof(Equip_Type)))
        {
            invDic.Add(part, new List<Equipment>());
        }
    }

    /// <summary>
    /// 인벤토리에 장비를 추가합니다.
    /// </summary>
    /// <param name="equip">추가할 장비</param>
    /// <returns>성공 여부</returns>
    public bool AddEquipment(Equipment equip)
    {
        //획득한 장비의 장착 부위와 일치하는 리스트를 받아옵니다.
        var list = invDic[equip.equip_type];

        //그 리스트가 이미 해당 장비를 포함했다면 장비 중인 것이므로 실패를 반환합니다.
        if (list.Contains(equip))
        {
            return false;
        }

        //해당 리스트에 해당 장비를 추가합니다.
        list.Add(equip);
        //성공을 반환합니다.
        return true;
    }

    /// <summary>
    /// 인벤토리에서 해당 장비를 없앱니다.
    /// </summary>
    /// <param name="equip">없애고자 하는 장비</param>
    /// <returns>성공 여부</returns>
    public bool RemoveEquipment(Equipment equip)
    {
        //해당 사전에서 해당 장비의 장착 부위의 리스트를 받아, 해당 값을 제거하는 것을 시도합니다.
        return invDic[equip.equip_type].Remove(equip);
    }

    /// <summary>
    /// 장착 부위에 맞는 인벤토리를 받아옵니다.
    /// </summary>
    /// <param name="part">인벤토리를 받아올 장착 부위</param>
    /// <returns>해당 장착 부위와 일치하는 인벤토리</returns>
    public List<Equipment> GetInventory(Equip_Type part)
    {
        return invDic[part];
    }

    public int GetInventoryIndex(Equipment equip)
    {
        return invDic[equip.equip_type].IndexOf(equip);
    }

    public InventorySaveData GetSaveData()
    {
        var data = new InventorySaveData();
        data.EquipmentInventory = new List<EquipmentSaveData>();

        //각 장비 부위별 인벤토리마다 아래 코드를 실행합니다.
        foreach (var pair in invDic)
        {
            //장비 부위는 Dictionary의 Key값으로 사용되는 값을 그대로 넣을 예정입니다.
            Equip_Type type = pair.Key;

            //해당 Key 기준 인벤토리 리스트를 받아옵니다.
            List<Equipment> list = pair.Value;

            //리스트 내에 있는 모든 유효한 장비 수만큼 아래 코드를 실행합니다.
            for (int i = 0; i < list.Count; i++)
            {
                //해당 장비 칸이 비어있지 않다면
                if (list[i] != null)
                {
                    //저장할 데이터에 아래 항목을 추가합니다.
                    data.EquipmentInventory.Add(new EquipmentSaveData
                    {
                        instanceGUID = list[i].InstanceGUID,
                        equipID = list[i].equip_id,
                        equipLevel = list[i].equip_level,
                        equipEnchant = list[i].equip_Upgrade,
                        equipRarity = list[i].equipment_Rarity,
                        enchantWeight = list[i].equip_Upgrade_Weight,
                        fuseWeight = list[i].equip_Fuse_Weight,
                        isEquipped = list[i].isEquipped,
                        slotIndex = list[i].EquippedSlotIndex
                    });
                }
            }
        }
        return data;
    }

    public void LoadFromSaveData(InventorySaveData data)
    {
        //이제 받아온 세이브데이터의 모든 데이터를 대상으로 아래 코드를 실행합니다.
        foreach (var slot in data.EquipmentInventory)
        {
            //저장 데이터의 인벤토리 내에 담아둔 장비 저장 데이터를 기반으로 새롭게 장비를 생성합니다.
            Equipment equipment = new Equipment(slot.instanceGUID, DataManager.Instance.GetData<EquipData>(slot.equipID), (Rarity)slot.equipRarity, slot.equipLevel);

            //강화 단계와 강화 가중치, 합성 가중치는 0으로 생성되므로, 해당 값을 대입해줍니다.
            equipment.equip_Upgrade = slot.equipEnchant;
            equipment.equip_Upgrade_Weight = slot.enchantWeight;
            equipment.equip_Fuse_Weight = slot.fuseWeight;
            equipment.isEquipped = slot.isEquipped;
            equipment.EquippedSlotIndex = slot.slotIndex;

            AddEquipment(equipment);
        }
        EquipmentUI ui = FindAnyObjectByType<EquipmentUI>(FindObjectsInactive.Include);
        Debug.Log(ui == null);
        ui.UpdateEquipState();
        Debug.Log("장비 장착 갱신완료");
    }
}
