using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWeaponGenerator : MonoBehaviour
{
    public static TestWeaponGenerator Instance;

    [SerializeField] private EquipmentInventory equipmentInventory;           // 현재 사용할 인벤토리.
    [SerializeField] Sprite weaponSprite;                                     // 획득 무기에 적용할 스프라이트.
    [SerializeField] InventoryPanel inventoryPanel;                           // 갱신해야 할 인벤토리.

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    public void Test(int equipLevel)
    {
        if (equipmentInventory == null)
        {
            Debug.LogError("EquipmentInventory를 연결해주세요!");
            return;
        }

        //랜덤 숫자를 생성합니다. (90% 확률로 미획득, 1.25% 확률로 부위별 획득 - 현재는 전부 무기로.)
        int rng = Random.Range(0, 40);

        int variation = Random.Range(0, 2);
        //60% 확률로 드랍하는 메인 그룹의 장비와 40% 확률로 드랍하는 서브 그룹의 장비입니다.
        int result = rng < 24? (rng) / 6 : (rng)/4;
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
            case 6:
                _equipData = DataManager.Instance.GetData<EquipData>(54000 + variation);
                break;
            case 7:
                _equipData = DataManager.Instance.GetData<EquipData>(55000 + variation);
                break;
            case 8:
                _equipData = DataManager.Instance.GetData<EquipData>(56000 + variation);
                break;
            case 9:
                _equipData = DataManager.Instance.GetData<EquipData>(56002 + variation);
                break;
        }
        int Rarity = ItemCalculator.RarityCalculator();
        Equipment testWeapon = new Equipment(System.Guid.NewGuid().ToString(), _equipData, Rarity, equipLevel);

        if (_equipData != null)
        {
            Debug.Log($"이름 : {testWeapon.equip_name}, 종류 : {testWeapon.equip_type}");
            Debug.Log($"등급 : {testWeapon.equipment_Rarity}, 착용레벨 : {testWeapon.equip_level}");
            Debug.Log($"강화 : {testWeapon.equip_Upgrade}, 세트 : {testWeapon.equip_set_id}, 레벨 : {testWeapon.equip_level}");
        }
        testWeapon.equip_type = _equipData.Equip_Type;
        testWeapon.equip_name = _equipData.Equip_Name;

        //해당 장비를 인벤토리에 넣습니다.
        equipmentInventory.AddEquipment(testWeapon);
        Debug.Log("장비 획득 성공!");

        if (inventoryPanel.currentPart != 0)
        {
            inventoryPanel.Refresh();
        }

        int count = equipmentInventory.GetInventory(Equip_Type.Weapon).Count;
        Debug.Log($"현재 인벤토리의 무기 개수: {count}");
    }
}