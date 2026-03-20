using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryStatus
{
    Equip, Fuse
}
[RequireComponent(typeof(InventoryEquipFunction))]
public class InventoryPanel : BaseUI
{
    public InventoryStatus status = InventoryStatus.Equip;
    public Equip_Type currentPart;                          // 현재 열람하고자 하는 인벤토리의 장착 부위 정보
    public Action onInventoryChanged;
    public Action onInventoryClosed;
    public Action onClickEquipment;

    [SerializeField] EquipmentUI _equipmentUI;                   // 장착 중인 장비를 보여주는 UI

    [Header("인벤토리 관련")]
    [SerializeField] Transform content;                                                      // 인벤토리 생성 위치
    [SerializeField] private List<InventorySlot> _slots = new List<InventorySlot>();         // 인벤토리 슬롯의 배열
    [SerializeField] private InventorySlot _slotPrefab;                                      // 생성할 인벤토리 프리팹
    [SerializeField] EquipmentInventory _equipmentInventory;                                 // 인벤토리
    [SerializeField] CanvasGroup _inventoryPanelGroup;                                       // 패널 자신을 넣으면 되는, 캔버스 그룹 제어용.

    [Header("인벤토리 상태별 활성화할 버튼 오브젝트 모음")]
    [SerializeField] GameObject _equipButtons;                   // 장착 슬롯을 눌렀을 때의 버튼입니다.
    [SerializeField] GameObject _fuseButtons;                    // 합성 슬롯을 눌렀을 때의 버튼입니다.
    [SerializeField] GameObject _normalButtons;                  // 다른 장비 버튼들을 눌렀을 때의 버튼입니다.
    [SerializeField] GameObject _ringButtons;                    // 반지 장비 버튼을 눌렀을 때의 버튼입니다.

    [Header("장비 상태별 활성화할 버튼 모음")]
    [SerializeField] Button _enchantButton;                  // 장비 강화 버튼입니다.
    [SerializeField] Button _ringEnchantButton;                  // 장비 강화 버튼입니다.
    [SerializeField] Button _salvageButton;                  // 장비 분해 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _sellButton;                     // 장비 판매 시도를 위한 인벤토리 내 버튼입니다.

    [Header("장비 정보 출력용")]
    [SerializeField] GameObject _infoPanel;                  // 정보를 담을 패널
    [SerializeField] Image _infoIcon;                        // 정보 패널에서의 장비 아이콘 출력용 이미지
    [SerializeField] Sprite _infoIconBaseSprite;             // 정보 패널에서 장비 아이콘의 기본 상태용 스프라이트
    [SerializeField] TextMeshProUGUI _infoName;              // 정보 패널에서 장비의 이름를 나타낼 텍스트
    [SerializeField] TextMeshProUGUI _infoDescription;       // 정보 패널에서 장비의 정보를 나타낼 텍스트

    [SerializeField] Button _confirmButton;                  // 합성 전용 확인버튼
    [SerializeField] Button _cancelButton;                   // 합성 전용 취소버튼
    [SerializeField] Button _equipButton;                    // 장비를 장착합니다.
    [SerializeField] Button _equipRightButton;                    // 장비를 장착합니다.
    [SerializeField] Button _equipLeftButton;                    // 장비를 장착합니다.
    [SerializeField] TextMeshProUGUI _equipButtonText;
    [SerializeField] TextMeshProUGUI _equipRightButtonText;
    [SerializeField] TextMeshProUGUI _equipLeftButtonText;
    [SerializeField] Button _closeButton;

    public EquipSlot targetSlot;                             // 장비를 받기 위한 대상 슬롯입니다.
    private Equipment _selectedEquipment;                    // 인벤토리 칸에서 선택한 장비
    
    private InventoryEquipFunction _inventoryEquipFunction;
    private InventorySlot _currentSelectedSlot;
    private void Awake()
    {
        //인스펙터상의 실수 확인용
        if (_slots.Count != 16)
            Debug.LogWarning("InventoryPanel - 슬롯의 수가 맞지 않습니다.");
        _inventoryEquipFunction = GetComponent<InventoryEquipFunction>();
        _equipButton.onClick.AddListener(() =>
        {
            AddToSlot(_selectedEquipment);
        });
        _equipRightButton.onClick.AddListener(() =>
        {
            SelectSlotAndEquip(_equipmentUI._secondRingSlot, _selectedEquipment);
        });
        _equipLeftButton.onClick.AddListener(() =>
        {
            SelectSlotAndEquip(_equipmentUI._firstRingSlot, _selectedEquipment);
        });
        //분해 버튼 기능 추가 - 분해 상태 O. 안내패널 활성화
        
        //확인버튼 기능 추가 - 슬롯에 해당 장비 추가
        _confirmButton.onClick.AddListener(() =>
        {
            if(_selectedEquipment.isLocked == false)
            {
                AddToSlot(_selectedEquipment);
            }
        });
        //취소버튼 기능 추가 - 해당 슬롯에서 장비 제거
        _cancelButton.onClick.AddListener(() =>
        {
            RemoveFromSlot();
        });

        //인벤토리 변화 시 발생하는 이벤트에 새로고침 메서드 추가
        onInventoryChanged += Refresh;
        onInventoryChanged += ResetInfo;
        onInventoryChanged += DisableInteractable;

        onInventoryClosed += Refresh;
        onInventoryClosed += ResetInfo;
        onInventoryClosed += DisableInteractable;

        onClickEquipment += EnableInteractable;

        _closeButton.onClick.AddListener(() =>
        {
            onInventoryClosed?.Invoke();
            Close();
        });
    }

    private void Start()
    {
        Close();
    }
    private void OnDestroy()
    {
        onInventoryChanged -= Refresh;
        onInventoryChanged -= ResetInfo;
        onInventoryChanged -= DisableInteractable;

        onInventoryClosed += Refresh;
        onInventoryClosed += ResetInfo;
        onInventoryClosed -= DisableInteractable;

        onClickEquipment -= EnableInteractable;
    }

    /// <summary>
    /// 슬롯을 선택했을 때의 동작입니다.
    /// </summary>
    /// <param name="slot">현재 선택중인 슬롯</param>
    public void OnSelectSlot(InventorySlot slot)
    {
        //기존에 선택하고 있던 슬롯이 있었을 경우 아래 코드를 실행합니다.
        if(_currentSelectedSlot != null)
        {
            //해당 슬롯의 선택 표시를 비활성화합니다.
            _currentSelectedSlot.selectMark.SetActive(false);
        }

        //현재 선택한 슬롯을 기억합니다.
        _currentSelectedSlot = slot;
        //그 슬롯의 선택 표시를 활성화합니다.
        _currentSelectedSlot.selectMark.SetActive(true);
    }

    public bool CheckLocked()
    {
        return _selectedEquipment.isLocked;
    }

    public bool CheckEquipped()
    {
        return _selectedEquipment.isEquipped;
    }

    /// <summary>
    /// 장비를 클릭했을 때의 동작입니다.
    /// </summary>
    /// <param name="equip">인벤토리 내에서 선택된 장비</param>
    public void OnClickItem(Equipment equip)
    {
        //인자값으로 받은 장비를 EquipmentManager가 넘겨줄 대상 장비로 설정합니다.
        _selectedEquipment = equip;

        var equipRankData = DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity);

        //정보를 보여줄 패널 활성화, 장비의 아이콘을 가져오고 이름을 변경하며, 그 이름의 색을 레어도와 일치시킵니다.
        _infoPanel.SetActive(true);
        _infoIcon.sprite = equip.icon;
        _infoName.text = equip.equip_Upgrade == 0 ? $"이름: {equip.equip_name}" : $"이름: {equip.equip_name}<color=orange> +{equip.equip_Upgrade}</color>";
        _infoName.color = RarityColor.GetColor((Rarity)equipRankData.Equip_Rank);

        _inventoryEquipFunction.ChangeLockButtonState(equip);
        
        _infoDescription.text = EquipmentSlotManager.Instance.GetEquipStatusString(equip);

        _equipButtonText.text = equip.isEquipped == true ? "장비 해제" : "장비 장착";
        //확인 버튼에 있던 기능을 지우고, 합성 슬롯에 집어넣기 기능을 추가합니다.
        //현재는 장착 기능을 구현하지 않았으므로 예외 처리 없이 바로 넣습니다.
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            AddToSlot(_selectedEquipment);
        });
        //취소 버튼에 있던 기능을 지우고, 합성 슬롯에서 빼기 기능을 추가합니다.
        //확인 버튼과 동일하게 예외 처리 없이 바로 넣습니다.
        _cancelButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.AddListener(() =>
        {
            RemoveFromSlot();
        });
        onClickEquipment?.Invoke();
    }

    /// <summary>
    /// 인벤토리 내에서 선택된 장비가 있는지 여부를 확인합니다.
    /// </summary>
    /// <returns>_selectedEquipment 값 존재 여부</returns>
    public bool CheckEquipmentSelected()
    {
        //선택된 장비가 존재할 경우 참을 반환합니다.
        if(_selectedEquipment != null)
        {
            return true;
        }
        //없을 시 거짓을 반환합니다.
        return false;
    }

    public void EnableInteractable()
    {
        _enchantButton.interactable = true;
        _ringEnchantButton.interactable = true;
        _sellButton.interactable = true;
        _salvageButton.interactable = true;
    }
    public void DisableInteractable()
    {
        _enchantButton.interactable = false;
        _ringEnchantButton.interactable = false;
        _sellButton.interactable = false;
        _salvageButton.interactable = false;
    }

    /// <summary>
    /// 인자값으로 받은 장착 부위에 맞는 인벤토리를 엽니다.
    /// </summary>
    /// <param name="part">인벤토리를 확인하고자 하는 장착 부위</param>
    public void OpenInventory(Equip_Type part, int slotIndex)
    {
        //선택 중인 슬롯을 초기화합니다.
        ClearCurrentSlot();

        //현재 장착 부위를 인자값으로 받아온 값과 일치시킵니다.
        if(part != 0)
        currentPart = part;
        int _currentSlotIndex = slotIndex;

        //인벤토리를 다시 불러옵니다.
        onInventoryChanged.Invoke();
    }

    /// <summary>
    /// 인벤토리 내부를 새로고침하는 메서드입니다.
    /// </summary>
    public void Refresh()
    {
        if(currentPart == Equip_Type.Ring)
        {
            _normalButtons.SetActive(false);
            _ringButtons.SetActive(true);
        }
        else
        {
            _normalButtons.SetActive(true);
            _ringButtons.SetActive(false);
        }
        //장착 부위에 맞는 인벤토리를 가져옵니다.
        List<Equipment> list = _equipmentInventory.GetInventory(currentPart);
        ClearScrollContent();

        if (_slots.Count < list.Count)
            CreateSlots(list.Count);

        //인벤토리 슬롯 길이만큼 다음 동작을 실행합니다.
        for (int i = 0; i<_slots.Count; i++)
        {
            //리스트에 있는 총 수보다 i가 적으면 그 리스트에서 장비 정보를 가져옵니다.
            if(i < list.Count)
            {
                _slots[i].Set(list[i]);
                SetSlot(_slots[i], list[i]);
                _slots[i].gameObject.SetActive(true);
            }
            else
            {
                _slots[i].gameObject.SetActive(false);
            }
        }
    }

    void CreateSlots(int count)
    {
        for (int i = 0; i < count; i++)
        {
            InventorySlot slot = Instantiate(_slotPrefab, content);

            _slots.Add(slot);
        }
    }

    private void ClearScrollContent()
    {
        //생성된 아이템들 기준
        foreach (var item in _slots)
        {
            //내용물이 있으면 파괴
            if (item != null) Destroy(item.gameObject);
        }
        //생성된 아이템들 리스트 초기화
        _slots.Clear();
    }

    /// <summary>
    /// 인벤토리 슬롯에 장비를 지정합니다.
    /// </summary>
    /// <param name="slot">인벤토리 슬롯</param>
    /// <param name="equip">해당 슬롯과 index가 일치하는 인벤토리 내 장비</param>
    private void SetSlot(InventorySlot slot, Equipment equip)
    {
        //해당 슬롯이 지금 내가 선택하고 있는 슬롯이 맞다면, 해당 슬롯을 활성화합니다.
        slot.selectMark.SetActive(_currentSelectedSlot == slot);

        //해당 슬롯 버튼을 눌렀을 때의 동작을 전부 지우고 새로 추가합니다.
        slot.button.onClick.RemoveAllListeners();
        slot.button.onClick.AddListener(() =>
        {
            //해당 슬롯을 눌렀다는 것을 확인할 수 있도록 합니다.
            OnSelectSlot(slot);
            //기존 장비가 있다면 해제하여, 현재 장비를 장착합니다.
            OnClickItem(equip);
        });
    }

    /// <summary>
    /// 해당 인벤토리 슬롯을 비웁니다.
    /// </summary>
    /// <param name="slot">비우고자 하는 슬롯</param>
    private void ClearSlot(InventorySlot slot)
    {
        //스프라이트를 없애고, 스프라이트의 활성 여부를 거짓으로 설정합니다.
        slot.icon.sprite = null;
        slot.icon.enabled = false;

        //장착 중이 아니므로 장착 표기를 비활성화합니다.
        slot.equipMark.SetActive(false);

        //합성할 수 있는 상태가 아니므로 합성 재료 표기를 비활성화합니다.
        slot.fuseMark.SetActive(false);

        //비어있는 장비를 잠글 수는 없으므로 잠금 표기도 비활성화합니다.
        slot.lockedMark.SetActive(false);

        //빈 공간을 선택할 수는 없으므로 선택 표기도 비활성화합니다.
        slot.selectMark.SetActive(false);

        //장비가 존재하지 않으면 강화 또한 불가능하므로 강화 단계 표기 텍스트를 비워줍니다.
        slot.UpgradeValue.text = "";

        //해당 슬롯 버튼을 눌렀을 때 동작을 전부 지웁니다. 이 버튼을 눌렀을 때는 어떤 동작도 일어나면 안 됩니다.
        slot.button.onClick.RemoveAllListeners();
    }

    public void ClearCurrentSlot()
    {
        _currentSelectedSlot = null;
    }

    /// <summary>
    /// 인벤토리 패널에서 선택했을 경우 그 장비 정보를 넘기기 위한 슬롯을 정의합니다.
    /// </summary>
    /// <param name="slot">현재 장비를 받아와야 할 슬롯의 정보</param>
    public void SetTargetSlot(EquipSlot slot)
    {
        targetSlot = slot;
        OpenInventory(slot.part, slot.slotIndex);
    }

    public void SelectSlotAndEquip(EquipSlot slot, Equipment equip)
    {
        targetSlot = slot;
        AddToSlot(equip);
    }
    /// <summary>
    /// 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="equip">장착 혹은 재료로 사용할 장비</param>
    public void AddToSlot(Equipment equip)
    {
        if (targetSlot != null)
        {
            //해당 슬롯에 아무것도 장착되지 않은 상태에서, 이미 다른 슬롯에 장착된 장비를 착용하려 할 경우 반환합니다.
            if(targetSlot.equipped == null && equip.isEquipped == true)
            {
                return;
            }

            //해당 슬롯에 무언가 장착되어있다면, 장착한 슬롯의 장비와 장착을 시도하는 "장착 중인" 장비의 GUID를 비교하고, 다를 경우 반환합니다.
            if (targetSlot.equipped != null && equip.isEquipped == true & targetSlot.equipped.InstanceGUID != equip.InstanceGUID)
            {
                return;
            }
            //해당 슬롯에 무언가 장착되어있다면, 장착한 슬롯의 장비와 장착을 시도하는 "장착 중인" 장비의 GUID를 비교하고, 다를 경우 반환합니다.
            else if (targetSlot.equipped != null && equip.isEquipped == true & targetSlot.equipped == equip)
            {
                targetSlot.equipped.UnEquip();
                targetSlot.ClearSlot();
                EquipmentSlotManager.Instance.ApplyEquipmentSet();
                onInventoryChanged?.Invoke();
                _equipButtonText.text = "장비 장착";
                return;
            }

            //합성 슬롯에 이미 장착 중인 장비를 넣으려고 할 경우, 경고를 출력하고 반환합니다.
            if (targetSlot.slotType == SlotType.FuseSlot && equip.isEquipped == true)
            {
                return;
            }

            if(targetSlot.slotType == SlotType.FuseSlot && equip.isLocked == true)
            {
                return;
            }

            //기존 슬롯에 이미 존재하던 장비는 제거합니다.
            if (targetSlot.equipped != null && targetSlot.slotType == SlotType.EquipSlot)
            {
                targetSlot.equipped.UnEquip();
            }
            else if(targetSlot.equipped != null && targetSlot.slotType == SlotType.FuseSlot)
            {
                targetSlot.equipped.CancelFuseMaterial();
            }

            //대상 슬롯의 장비에 해당 장비를 집어넣습니다.
            targetSlot.equipped = equip;

            //대상 슬롯의 이미지를 동일하게 만들고, 이미지를 활성화하며,
            //시각적으로 볼 수 있도록 레어도에 맞게 이미지 색을 변경합니다.
            targetSlot.iconImage.sprite = equip.icon;
            targetSlot.iconImage.enabled = true;

            //장비 슬롯이라면 장비를 장착합니다.
            if (targetSlot.slotType == SlotType.EquipSlot)
            {
                targetSlot.iconImage.color = Color.white;
                equip.SetEquipped(targetSlot.slotIndex);
                EquipmentSlotManager.Instance.ApplyEquipmentSet();
                onInventoryChanged?.Invoke();
                _equipButtonText.text = "장비 장착";
            }
            //합성 슬롯이라면 합성 재료로 사용중임을 표시합니다.
            else if(targetSlot.slotType == SlotType.FuseSlot)
            {
                targetSlot.iconImage.color = RarityColor.GetColor((Rarity)equip.equipment_Rarity);
                equip.isFusing = true;
                Close();
            }
        }
    }

    /// <summary>
    /// 합성 슬롯으로부터 장비를 제거합니다.
    /// </summary>
    private void RemoveFromSlot()
    {
        //타겟 슬롯이 null이 아니라면
        if (targetSlot != null)
        {
            //슬롯에 장착된 장비를 null로 바꾸고
            targetSlot.equipped = null;
            //스프라이트를 제거합니다.
            targetSlot.iconImage.sprite = null;
        }
    }

    public void ResetInfo()
    {
        _selectedEquipment = null;
        _infoIcon.sprite = _infoIconBaseSprite;
        _infoName.text = "";
        _infoDescription.text = "";
    }

    

    /// <summary>
    /// 장비 데이터를 전송합니다.
    /// </summary>
    /// <returns>현재 선택된 장비</returns>
    public Equipment GiveEquipmentData()
    {
        return _selectedEquipment;
    }

    public InventorySlot GiveCurrentSlotData()
    {
        return _currentSelectedSlot;
    }

    public InventorySlot GiveTargetSlotData(Equipment equip)
    {
        return _slots[_equipmentInventory.GetInventoryIndex(equip)];
    }

    public void OnStatusChange(bool isEquip)
    {
        status = isEquip == true? InventoryStatus.Equip : InventoryStatus.Fuse;
        if(status == InventoryStatus.Equip)
        {
            _equipButtons.SetActive(true);
            _fuseButtons.SetActive(false);
        }
        else
        {
            _equipButtons.SetActive(false);
            _fuseButtons.SetActive(true);
        }
    }

    protected override void OnOpen()
    {
        
    }

    protected override void OnClose()
    {
        
    }
}
