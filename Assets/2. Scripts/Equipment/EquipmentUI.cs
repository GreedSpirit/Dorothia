using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : BaseUI
{
    [Header("버튼의 리스트")]
    [SerializeField] List<EquipSlot> _partSlots;        // 반지 슬롯을 제외한 나머지 버튼을 등록하기 위한 버튼의 리스트입니다.
    public EquipSlot _firstRingSlot;
    public EquipSlot _secondRingSlot;
    [SerializeField] InventoryPanel _inventoryPanel;   // 인벤토리를 담당하는 패널

    [SerializeField] EquipmentSlotManager _equipmentSlotManager;

    [SerializeField]private Equip_Type _currentSelectedPart;           // 현재 인벤토리를 열람할 장착 부위
    [SerializeField]private EquipSlot _currentSelectedSlot;            // 가장 최근에 누른 장착슬롯

    private int _slotIndex;                   //중요! 부위 별 슬롯 인덱스이므로 2개를 장착 가능한 반지의 2번째 반지 슬롯만 1, 나머지는 전부 0으로 두어야 합니다.

    private void Start()
    {
        Equip_Type[] partMapping = new Equip_Type[]
        {
            Equip_Type.Necklace,
            Equip_Type.Clothes,
            Equip_Type.Pants,
            Equip_Type.Shoes,
            Equip_Type.Weapon,
            Equip_Type.Gloves
        };

        // 리스트 순회하면서 AddListener
        for (int i = 0; i < _partSlots.Count; i++)
        {
            int index = i; // 클로저 문제 때문에 로컬 변수에 저장
            _partSlots[i].gameObject.GetComponent<Button>().onClick.AddListener(() =>
            {
                _inventoryPanel.SetTargetSlot(_partSlots[index]);
                _inventoryPanel.OpenInventory(partMapping[index], 0);
            });
        }
        _firstRingSlot.gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            _inventoryPanel.SetTargetSlot(_firstRingSlot);
            _inventoryPanel.OpenInventory(Equip_Type.Ring, 0);
        });
        _secondRingSlot.gameObject.GetComponent<Button>().onClick.AddListener(() => 
        {
            _inventoryPanel.SetTargetSlot(_secondRingSlot);
            _inventoryPanel.OpenInventory(Equip_Type.Ring, 1);
        });

        Close();
    }

    /// <summary>
    /// 인벤토리를 확인합니다.
    /// </summary>
    /// <param name="slot">장착 부위를 담당하는 해당 슬롯</param>
    public void OpenInventory(EquipSlot slot)
    {
        //그 슬롯의 장착 부위에 맞는 인벤토리를 엽니다.
        _inventoryPanel.OpenInventory(slot.part, slot.slotIndex);
    }

    public void SetSlot(EquipSlot slot)
    {
        _currentSelectedSlot = slot;
    }

    /// <summary>
    /// 장착 슬롯을 업데이트합니다.
    /// </summary>
    /// <param name="slot">업데이트할 슬롯</param>
    public void UpdatePartUI(EquipSlot slot)
    {
        //슬롯에 아무것도 장착되어있지 않다면
        if(slot.equipped == null)
        {
            //아이콘을 비활성화합니다.
            slot.iconImage.enabled = false;
            return;
        }

        //아이콘을 활성화합니다.
        slot.iconImage.enabled = true;
        //아이콘을 해당 장비 아이콘과 동일하게 만듭니다.
        slot.iconImage.sprite = slot.equipped.icon;
    }

    //반지 슬롯을 대비해, 평상시에 Index를 0으로 하기 위해 만든 메서드입니다.
    public void EquipSlotFunction()
    {
        _slotIndex = 0;
    }

    //반지 슬롯 2를 눌렀을 때를 대비해, 해당 슬롯의 Index값을 하나 더 설정하였으며, 그 값을 적용하기 위해 만든 메서드입니다.
    public void SecondRingSlotFunction()
    {
        _slotIndex = 1;
    }

    public void UpdateEquipState()
    {
        List<Equipment> inv = EquipmentInventory.Instance.GetInventory(_firstRingSlot.part);
        Debug.Log(inv.Count);
        foreach(var equip in inv)
        {
            Debug.Log($"equip: {equip.EquippedSlotIndex}, slot: {_firstRingSlot.slotIndex}");
            if (equip.isEquipped == true && equip.EquippedSlotIndex == _firstRingSlot.slotIndex)
            {
                Debug.Log(equip.icon == null);
                _firstRingSlot.equipped = equip;
                _firstRingSlot.iconImage.color = Color.white;
                equip.SetEquipped(_firstRingSlot.slotIndex);
                _firstRingSlot.iconImage.sprite = equip.icon;
                _firstRingSlot.iconImage.enabled = true;
            }
            else if (equip.isEquipped == true && equip.EquippedSlotIndex == _secondRingSlot.slotIndex)
            {
                Debug.Log(equip.icon == null);
                _secondRingSlot.equipped = equip;
                _secondRingSlot.iconImage.color = Color.white;
                equip.SetEquipped(_secondRingSlot.slotIndex);
                _secondRingSlot.iconImage.sprite = equip.icon;
                _secondRingSlot.iconImage.enabled = true;

            }
        }
        foreach (var slots in _partSlots)
        {
            inv = EquipmentInventory.Instance.GetInventory(slots.part);

            foreach(var equip in inv)
            {
                Debug.Log($"equip: {equip.EquippedSlotIndex}, slot: {_firstRingSlot.slotIndex}");
                if (equip.isEquipped == true && equip.EquippedSlotIndex == slots.slotIndex)
                {
                    Debug.Log(equip.icon == null);
                    slots.equipped = equip;
                    slots.iconImage.color = Color.white;
                    equip.SetEquipped(slots.slotIndex);
                    slots.iconImage.sprite = equip.icon;
                    slots.iconImage.enabled = true;
                }
            }
        }
        //EquipmentSlotManager.Instance.ApplyEquipmentSet();
        _inventoryPanel.onInventoryChanged?.Invoke();
    }

    protected override void OnOpen()
    {
        
    }

    protected override void OnClose()
    {
        
    }
}
