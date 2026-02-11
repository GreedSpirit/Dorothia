using System.Collections.Generic;
using UnityEngine;

public class TestWeaponGenerator : MonoBehaviour
{
    [SerializeField] private EquipmentInventory equipmentInventory;
    [SerializeField] Sprite weaponSprite;

    public void Test()
    {
        if (equipmentInventory == null)
        {
            Debug.LogError("EquipmentInventory를 연결해주세요!");
            return;
        }

        List<Equipment> inv = equipmentInventory.GetInventory(EquipmentPart.Weapon);
        // 테스트용 무기 9개 생성
        for (int i = 0; i < 9; i++)
        {
            Equipment testWeapon = new Equipment
            {
                equipPart = EquipmentPart.Weapon,
                equipmentRarity = EquipmentRarity.Normal,
                icon = weaponSprite,
                name = "테스트용 무기"
            };

            // 인벤토리에 추가
            equipmentInventory.AddEquipment(testWeapon);
        }
        int count = equipmentInventory.GetInventory(EquipmentPart.Weapon).Count;
        Debug.Log($"현재 인벤토리의 무기 개수: {count}");
        Debug.Log("테스트용 무기 9개 생성 완료!");
    }
}