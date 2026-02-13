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

   public int GetInventoryIndex(Equipment equip)
    {
        return invDic[equip.equip_type].IndexOf(equip);
    }
}
