using UnityEngine;
using UnityEngine.UI;

//장비의 장착 부위.
public enum EquipmentPart
{
    Weapon, Clothes, Pants, Gloves, Shoes, Necklace, Ring
}

//장비의 레어도.
public enum EquipmentRarity
{
    Normal, Uncommon, Rare, Legendary, Mythic
}

//파츠 별 가지게 될 옵션을 정의하기 위한, 추후 사용할 열거형.
public enum EquipmentStatus
{
    HealthPoint, AttackPoint, AttackSpeed, CriticalRate, CriticalDamage, Defense, MagicDefense, HealthRegeneration, MoveSpeed
}
public class Equipment : MonoBehaviour
{
    public EquipmentPart equipPart;          // 해당 장비의 장착 부위입니다.
    public EquipmentRarity equipmentRarity;  // 해당 장비의 레어도입니다.

    public Sprite icon;

    public int ID;                     // 동일 장비여도 ID값을 통해 구분하기 위함 (임시값)
    public int EquippedSlotIndex = -1; // 기본값은 -1, 장착 시 0, "반지 2 슬롯 한정" 1
    public bool isEquipped = false;    // 장착 시에만 true가 되는 장착 여부 확인용 bool형 매개변수

    /// <summary>
    /// 장착 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="slotIndex">장착할 슬롯(반지 슬롯 대비)</param>
    public void SetEquipped(int slotIndex)
    {
        //현재 장비를 장착하는 것이므로 장착 여부를 참으로 설정합니다.
        isEquipped = true;
        
        //현재 장착한 슬롯의 위치를 인자값으로 받아온 인덱스 값으로 설정합니다.
        EquippedSlotIndex = slotIndex;
    }

    /// <summary>
    /// 장비를 장착 해제합니다.
    /// </summary>
    public void UnEquip()
    {
        //현재 장비를 장착 해제한 것이므로 장착 여부를 거짓으로 설정합니다.
        isEquipped = false;

        //현재 장착한 슬롯 위치는 장착하지 않았을 때의 기본값으로 변경합니다.
        EquippedSlotIndex = -1;
    }
}
