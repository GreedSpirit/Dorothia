using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestWeaponGenerator : MonoBehaviour
{
    public static TestWeaponGenerator Instance;

    [SerializeField] private EquipmentInventory equipmentInventory;           // 현재 사용할 인벤토리.
    [SerializeField] InventoryPanel inventoryPanel;                           // 갱신해야 할 인벤토리.

    private Dictionary<Equip_Type, List<EquipData>> _equipList;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }

        Instance = this;
    }

    private void Start()
    {
        _equipList = new Dictionary<Equip_Type, List<EquipData>>();
        Dictionary<int, EquipData> data = DataManager.Instance.GetDict<EquipData>();

        foreach(var table in data.Values)
        {
            if (!_equipList.ContainsKey(table.Equip_Type))
            {
                _equipList[table.Equip_Type] = new List<EquipData>();
            }

            _equipList[table.Equip_Type].Add(table);
        }
        
    }

    /// <summary>
    /// 장비 생성에 필요한 숫자를 생성합니다.
    /// </summary>
    /// <returns></returns>
    public int GetNumber()
    {
        //랜덤 숫자를 생성합니다. (90% 확률로 미획득, 1.25% 확률로 부위별 획득 - 현재는 전부 무기로.)
        int rng = Random.Range(0, 40);

        //60% 확률로 드랍하는 메인 그룹의 장비와 40% 확률로 드랍하는 서브 그룹의 장비입니다.
        int result = rng < 24 ? (rng) / 6 : (rng) / 4;

        return result;
    }

    /// <summary>
    /// GetNumber을 통해 생성된 숫자를 기반으로 장비를 생성합니다.
    /// </summary>
    /// <param name="rng">GetNumber()을 통해 얻은 숫자</param>
    /// <returns></returns>
    public EquipData GetEquipmentData(int rng)
    {
        EquipData _equipData = new EquipData();

        switch (rng)
        {
            //1을 더했을 때 각각의 장착 부위가 되도록 생성합니다. 8번은 열거형 값에 존재하지 않으므로 7번을 중복 사용했습니다.
            case 0:
                _equipData = _equipList[Equip_Type.Weapon][Random.Range(0, _equipList.Count)];
                break;
            case 1:
                _equipData = _equipList[Equip_Type.Clothes][Random.Range(0, _equipList.Count)];
                break;
            case 2:
                _equipData = _equipList[Equip_Type.Pants][Random.Range(0, _equipList.Count)];
                break;
            case 3:
                _equipData = _equipList[Equip_Type.Gloves][Random.Range(0, _equipList.Count)];
                break;
            case 6:
                _equipData = _equipList[Equip_Type.Shoes][Random.Range(0, _equipList.Count)];
                break;
            case 7:
                _equipData = _equipList[Equip_Type.Necklace][Random.Range(0, _equipList.Count)];
                break;
            case 8:
                _equipData = _equipList[Equip_Type.Ring][Random.Range(0, _equipList.Count)];
                break;
            case 9:
                _equipData = _equipList[Equip_Type.Ring][Random.Range(0, _equipList.Count)];
                break;
        }
        return _equipData;
    }
    public void Test(int equipLevel)
    {
        if (equipmentInventory == null)
        {
            return;
        }

        EquipData _equipData = GetEquipmentData(GetNumber());
        int Rarity = ItemCalculator.RarityCalculator();
        Equipment testWeapon = new Equipment(System.Guid.NewGuid().ToString(), _equipData, (Rarity)Rarity, equipLevel);


        //해당 장비를 인벤토리에 넣습니다.
        equipmentInventory.AddEquipment(testWeapon);
        //Debug.LogError(testWeapon.equip_name);

        if (inventoryPanel.currentPart != 0)
        {
            inventoryPanel.Refresh();
        }
    }


    public Equipment Test2(int equipLevel, int Rarity)
    {
        if (equipmentInventory == null)
        {
            return null;
        }

        EquipData _equipData = GetEquipmentData(GetNumber());
        if(Rarity < 1 || Rarity > 5)
        {
            Rarity = ItemCalculator.RarityCalculator();
        }
        Equipment testWeapon = new Equipment(System.Guid.NewGuid().ToString(), _equipData, (Rarity)Rarity, equipLevel);


        //해당 장비를 인벤토리에 넣습니다.
        equipmentInventory.AddEquipment(testWeapon);

        if (inventoryPanel.currentPart != 0)
        {
            inventoryPanel.Refresh();
        }

        return testWeapon;
    }
}