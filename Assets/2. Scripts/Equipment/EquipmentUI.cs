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
    [SerializeField] CanvasGroup _equipmentUIGroup;    // 자기 자신을 넣어주면 되는, 캔버스 그룹 제어용.

    [SerializeField]private EquipmentPart _currentSelectedPart;        // 현재 인벤토리를 열람할 장착 부위
    [SerializeField]private EquipSlot _currentSelectedSlot;            // 가장 최근에 누른 장착슬롯

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

    private void Start()
    {
        EquipmentPart[] partMapping = new EquipmentPart[]
        {
            EquipmentPart.Necklace,
            EquipmentPart.Clothes,
            EquipmentPart.Pants,
            EquipmentPart.Shoes,
            EquipmentPart.Weapon,
            EquipmentPart.Gloves
        };

        // 리스트 순회하면서 AddListener
        for (int i = 0; i < _partButtons.Count; i++)
        {
            int index = i; // 클로저 문제 때문에 로컬 변수에 저장
            _partButtons[i].onClick.AddListener(() =>
            {
                _inventoryPanel.Open(partMapping[index], 0);
            });
        }
        _firstRingButton.onClick.AddListener(() => _inventoryPanel.Open(EquipmentPart.Ring, 0));
        _secondRingButton.onClick.AddListener(() => _inventoryPanel.Open(EquipmentPart.Ring, 1));
    }

    /// <summary>
    /// 인벤토리를 확인합니다.
    /// </summary>
    /// <param name="slot">장착 부위를 담당하는 해당 슬롯</param>
    public void OpenInventory(EquipSlot slot)
    {
        //그 슬롯의 장착 부위에 맞는 인벤토리를 엽니다.
        _inventoryPanel.Open(slot.part, slot.slotIndex);
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

    /// <summary>
    /// 패널의 활성화 여부를 정합니다.
    /// </summary>
    /// <param name="value">활성화 여부</param>
    public void SetPanelActiveValue(bool value)
    {
        //참이면 1, 거짓이면 0으로 하여 참일 경우에만 보이게 합니다.
        _equipmentUIGroup.alpha = value == true ? 1 : 0;

        //상호작용 여부와 뒤 오브젝트와의 상호작용 제한은 참일 경우에만 활성화되도록 합니다.
        _equipmentUIGroup.interactable = value;
        _equipmentUIGroup.blocksRaycasts = value;
    }
}
