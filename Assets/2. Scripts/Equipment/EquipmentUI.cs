using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUI : MonoBehaviour
{
    [Header("버튼의 리스트")]
    [SerializeField] List<Button> _partButtons;        // 반지 슬롯을 제외한 나머지 버튼을 등록하기 위한 버튼의 리스트입니다.
    [SerializeField] Button _firstRingButton;
    [SerializeField] Button _secondRingButton;
    [SerializeField] InventoryPanel _inventoryPanel;   // 인벤토리를 담당하는 패널

    private EquipmentPart _currentSelectedPart;        // 현재 인벤토리를 열람할 장착 부위
    private EquipSlot _currentSelectedSlot;            // 가장 최근에 누른 장착슬롯

    private int _slotIndex;                   //중요! 부위 별 슬롯 인덱스이므로 2개를 장착 가능한 반지의 2번째 반지 슬롯만 1, 나머지는 전부 0으로 두어야 합니다.

    private void Awake()
    {
        //인스펙터상의 연결 오류 확인용
        if (_partButtons == null)
            Debug.LogError("EquipmentUI - 각 파트별 버튼이 하나도 등록되지 않았습니다!");
        if (_partButtons.Count != 6)
            Debug.LogWarning("EquipmentUI - 파트별 버튼이 모자랍니다!");
        if (_inventoryPanel == null)
            Debug.LogError("EquipmentUI - 장비를 담은 인벤토리 확인용 창이 등록되지 않았습니다!");
    }

    /// <summary>
    /// 인벤토리를 확인합니다.
    /// </summary>
    /// <param name="slot">장착 부위를 담당하는 해당 슬롯</param>
    public void OpenInventory(EquipSlot slot)
    {
        //현재 누른 슬롯에 해당 슬롯을 넣습니다.
        _currentSelectedSlot = slot;
        //그 슬롯의 장착 부위에 맞는 인벤토리를 엽니다.
        _inventoryPanel.Open(slot.part);
    }

    /// <summary>
    /// 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="equip">장착할 장비</param>
    public void EquipPart(Equipment equip)
    {
        //기존에 장착하고 있던 장비가 있다면 해제합니다.
        if (_currentSelectedSlot.equipped != null)
        {
            _currentSelectedSlot.equipped.UnEquip();
        }

        //받아온 장비를 장착합니다.
        _currentSelectedSlot.equipped = equip;

        //해당 장비의 상태를 장착 중으로 변경합니다.
        equip.SetEquipped(_currentSelectedSlot.slotIndex);

        //현재 선택한 슬롯의 상태를 업데이트합니다.
        UpdatePartUI(_currentSelectedSlot);
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
            //장착 표시를 지웁니다.
            slot.equippedMark.SetActive(false);
            return;
        }

        //아이콘을 활성화합니다.
        slot.iconImage.enabled = true;
        //아이콘을 해당 장비 아이콘과 동일하게 만듭니다.
        slot.iconImage.sprite = slot.equipped.icon;

        //장착 표시를 활성화합니다.
        slot.equippedMark.SetActive(true);
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
}
