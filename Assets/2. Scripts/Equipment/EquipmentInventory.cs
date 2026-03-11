using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory : MonoBehaviour
{
    Dictionary<Equip_Type, List<Equipment>> invDic = new Dictionary<Equip_Type, List<Equipment>>(); // 장착 부위에 맞는 인벤토리를 담을 Dictionary

    private void Awake()
    {
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

        //그 리스트가 16개의 값을 이미 담고 있거나 그 이상을 담았을 경우 (테스트용) 범위를 넘어갔으므로 실패를 반환합니다.
        if (list.Count >= 16)
            return false;

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

    /// <summary>
    /// 현재 인벤토리 상태를 저장합니다.
    /// </summary>
    public void Save()
    {
        //저장할 데이터를 생성합니다.
        InventorySaveData saveData = new InventorySaveData();
        //데이터의 인벤토리를 새로 지정합니다.
        saveData.EquipmentInventory = new List<EquipmentSaveData>();

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
                    saveData.EquipmentInventory.Add(new EquipmentSaveData
                    {
                        instanceGUID = list[i].InstanceGUID,
                        equipID = list[i].equip_id,
                        equipLevel = list[i].equip_level,
                        equipEnchant = list[i].equip_Upgrade,
                        equipRarity = list[i].equipment_Rarity,
                        enchantWeight = 0,                     //list[i].강화가중치,
                        fuseWeight = 0                         //list[i].합성가중치
                    });
                }
            }
        }

        //이 데이터를 JsonUtility를 통해 Json 형식으로 변경합니다.
        string json = JsonUtility.ToJson(saveData);

        //테스트용 PlayerPrefs를 사용하여, Json형식으로 변경한 문자열을 InventorySave로 저장합니다.
        PlayerPrefs.SetString("InventorySave", json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 저장된 인벤토리를 불러옵니다.
    /// </summary>
    public void Load()
    {
        //PlayerPrefs에 InventorySave Key값이 없다면 저장한 적 없는 것이므로 반환합니다.
        if (!PlayerPrefs.HasKey("InventorySave"))
            return;

        //PlayerPrefs에 저장한 InventorySave 문자열을 받아옵니다.
        string json = PlayerPrefs.GetString("InventorySave");
        //해당 문자열을 다시 InventorySaveData로 치환합니다.
        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

        //인벤토리 Dictionary 내에 있는 모든 데이터를 대상으로 아래 코드를 실행합니다.
        foreach (var pair in invDic)
        {
            pair.Value.Clear();
        }

        //이제 받아온 세이브데이터의 모든 데이터를 대상으로 아래 코드를 실행합니다.
        foreach (var data in saveData.EquipmentInventory)
        {
            //저장 데이터의 인벤토리 내에 담아둔 장비 저장 데이터를 기반으로 새롭게 장비를 생성합니다.
            Equipment equipment = new Equipment(data.instanceGUID, DataManager.Instance.GetData<EquipData>(data.equipID), (Rarity)data.equipRarity, data.equipLevel);

            //강화 단계와 강화 가중치, 합성 가중치는 0으로 생성되므로, 해당 값을 대입해줍니다.
            equipment.equip_Upgrade = data.equipEnchant;
            equipment.equip_Upgrade_Weight = data.enchantWeight;
            equipment.equip_Fuse_Weight = data.fuseWeight;

            AddEquipment(equipment);
            //장비 Dictionary의 Key값을 저장 데이터의 Equip_Type으로, 그 Key값의 Value로 나오는 리스트의 칸 위치는 저장 데이터의 slotIndex로 하여
            //해당 위치에 방금 생성한 장비를 끼워넣습니다.
            //invDic = equipment;
        }
    }

    public int GetInventoryIndex(Equipment equip)
    {
        return invDic[equip.equip_type].IndexOf(equip);
    }
}
