using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWeaponGenerator : MonoBehaviour
{
    [SerializeField] private EquipmentInventory equipmentInventory;           // 현재 사용할 인벤토리.
    [SerializeField] Sprite weaponSprite;                                     // 획득 무기에 적용할 스프라이트.
    [SerializeField] InventoryPanel inventoryPanel;                           // 갱신해야 할 인벤토리.

    public void Test()
    {
        if (equipmentInventory == null)
        {
            Debug.LogError("EquipmentInventory를 연결해주세요!");
            return;
        }

        //랜덤 숫자를 생성합니다. (90% 확률로 미획득, 1.25% 확률로 부위별 획득 - 현재는 전부 무기로.)
        int rng = Random.Range(1, 401);

        //90% 확률에 맞게 10%만큼의 범위 내에 들어왔을 때 생성하도록 합니다.
        if(rng < 41)
        {
            //10% 범위 내에서 8개로 나눠, 1.25%의 드랍율을 맞추겠습니다.
            int result = (rng - 1) / 5;
            EquipData _equipData = new EquipData();

            switch (result)
            {
                //1을 더했을 때 각각의 장착 부위가 되도록 생성합니다. 8번은 열거형 값에 존재하지 않으므로 7번을 중복 사용했습니다.
                case 0:
                    _equipData = DataManager.Instance.GetData<EquipData>(50001);
                    break;
                case 1:
                    _equipData = DataManager.Instance.GetData<EquipData>(51000);
                    break;
                case 2:
                    _equipData = DataManager.Instance.GetData<EquipData>(52000);
                    break;
                case 3:
                    _equipData = DataManager.Instance.GetData<EquipData>(53000);
                    break;
                case 4:
                    _equipData = DataManager.Instance.GetData<EquipData>(54000);
                    break;
                case 5:
                    _equipData = DataManager.Instance.GetData<EquipData>(55000);
                    break;
                case 6:
                    _equipData = DataManager.Instance.GetData<EquipData>(56000);
                    break;
                case 7:
                    _equipData = DataManager.Instance.GetData<EquipData>(56000);
                    break;

            }
            Equipment testWeapon = new Equipment(System.Guid.NewGuid().ToString(),_equipData, 40001, 1);

            if (_equipData != null)
            {
                Debug.Log($"이름 : {testWeapon.equip_name}, 종류 : {testWeapon.equip_type}");
                Debug.Log($"등급 : {testWeapon.equipment_Rarity}, 착용레벨 : {testWeapon.equip_level}");
                Debug.Log($"강화 : {testWeapon.equip_Upgrade}, 세트 : {testWeapon.equip_set_id}");
            }
            testWeapon.equip_type = _equipData.Equip_Type;
            testWeapon.equip_name = _equipData.Equip_Name;

            testWeapon.icon = weaponSprite;

            //해당 장비를 인벤토리에 넣습니다.
            equipmentInventory.AddEquipment(testWeapon);
            Debug.Log("장비 획득 성공!");

            if(inventoryPanel.currentPart != 0)
            {
                inventoryPanel.onInventoryChanged.Invoke();
            }
        }

        int count = equipmentInventory.GetInventory(Equip_Type.Weapon).Count;
        Debug.Log($"현재 인벤토리의 무기 개수: {count}");
    }

    /// <summary>
    /// 테이블 변경사항(종류 추가) 반영 및 확정 지급을 통한 빠른 테스트 지원용 메서드입니다.
    /// </summary>
    public void Test2()
    {
        if (equipmentInventory == null)
        {
            Debug.LogError("EquipmentInventory를 연결해주세요!");
            return;
        }

        //랜덤 숫자를 생성합니다. (90% 확률로 미획득, 1.25% 확률로 부위별 획득 - 현재는 전부 무기로.)
        int rng = Random.Range(1, 41);

        //90% 확률에 맞게 10%만큼의 범위 내에 들어왔을 때 생성하도록 합니다.
        if (rng < 41)
        {
            //10% 범위 내에서 8개로 나눠, 1.25%의 드랍율을 맞추겠습니다.
            int result = (rng - 1) / 5;
            int variation = Random.Range(0, 2);
            EquipData _equipData = new EquipData();

            switch (result)
            {
                //1을 더했을 때 각각의 장착 부위가 되도록 생성합니다. 8번은 열거형 값에 존재하지 않으므로 7번을 중복 사용했습니다.
                case 0:
                    _equipData = DataManager.Instance.GetData<EquipData>(50001 + variation);
                    break;
                case 1:
                    _equipData = DataManager.Instance.GetData<EquipData>(51000 + variation);
                    break;
                case 2:
                    _equipData = DataManager.Instance.GetData<EquipData>(52000 + variation);
                    break;
                case 3:
                    _equipData = DataManager.Instance.GetData<EquipData>(53000 + variation);
                    break;
                case 4:
                    _equipData = DataManager.Instance.GetData<EquipData>(54000 + variation);
                    break;
                case 5:
                    _equipData = DataManager.Instance.GetData<EquipData>(55000 + variation);
                    break;
                case 6:
                    _equipData = DataManager.Instance.GetData<EquipData>(56000 + variation);
                    break;
                case 7:
                    _equipData = DataManager.Instance.GetData<EquipData>(56002 + variation);
                    break;

            }
            int Rarity = Random.Range(40001, 40006);
            Equipment testWeapon = new Equipment(System.Guid.NewGuid().ToString(),_equipData, Rarity, 1);

            if (_equipData != null)
            {
                Debug.Log($"이름 : {testWeapon.equip_name}, 종류 : {testWeapon.equip_type}");
                Debug.Log($"등급 : {testWeapon.equipment_Rarity}, 착용레벨 : {testWeapon.equip_level}");
                Debug.Log($"강화 : {testWeapon.equip_Upgrade}, 세트 : {testWeapon.equip_set_id}");
            }
            testWeapon.equip_type = _equipData.Equip_Type;
            testWeapon.equip_name = _equipData.Equip_Name;

            testWeapon.icon = weaponSprite;

            //해당 장비를 인벤토리에 넣습니다.
            equipmentInventory.AddEquipment(testWeapon);
            Debug.Log("장비 획득 성공!");

            if (inventoryPanel.currentPart != 0)
            {
                inventoryPanel.onInventoryChanged.Invoke();
            }
        }

        int count = equipmentInventory.GetInventory(Equip_Type.Weapon).Count;
        Debug.Log($"현재 인벤토리의 무기 개수: {count}");
    }
}