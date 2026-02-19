using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryStatus
{
    Equip, Fuse
}
public class InventoryPanel : MonoBehaviour
{
    public InventoryStatus status = InventoryStatus.Equip;
    public Equip_Type currentPart;                          // 현재 열람하고자 하는 인벤토리의 장착 부위 정보
    public Action onInventoryChanged;

    [SerializeField] EquipmentUI _equipmentUI;                   // 장착 중인 장비를 보여주는 UI

    [Header("인벤토리 관련")]
    [SerializeField] private List<InventorySlot> _slots;         // 인벤토리 슬롯의 배열
    [SerializeField] EquipmentInventory _equipmentInventory;     // 인벤토리
    [SerializeField] CanvasGroup _inventoryPanelGroup;           // 패널 자신을 넣으면 되는, 캔버스 그룹 제어용.

    [Header("인벤토리 상태별 활성화할 버튼 오브젝트 모음")]
    [SerializeField] GameObject _equipButtons;                   // 장착 슬롯을 눌렀을 때의 버튼입니다.
    [SerializeField] GameObject _fuseButtons;                    // 합성 슬롯을 눌렀을 때의 버튼입니다.

    [Header("장비 분해 관련")]
    [SerializeField] CanvasGroup _noticePanel;               // 장비 분해를 시도할 때 나타나도록 할 안내용 창입니다.
    [SerializeField] Button _salvageButton;                  // 장비 분해 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _salvageAcceptButton;            // 장비 분해 결정의 경우를 위한 안내창 내 Y 버튼입니다.
    [SerializeField] Button _salvageRejectButton;            // 장비 분해 취소의 경우를 위한 안내창 내 N 버튼입니다.

    [Header("장비 정보 출력용")]
    [SerializeField] GameObject _infoPanel;                  // 정보를 담을 패널
    [SerializeField] Image _infoIcon;                        // 정보 패널에서의 장비 아이콘 출력용 이미지
    [SerializeField] TextMeshProUGUI _infoName;              // 정보 패널에서 장비의 이름를 나타낼 텍스트
    [SerializeField] TextMeshProUGUI _infoDescription;       // 정보 패널에서 장비의 정보를 나타낼 텍스트

    [SerializeField] Button _confirmButton;                  // 합성 전용 확인버튼
    [SerializeField] Button _cancelButton;                   // 합성 전용 취소버튼
    [SerializeField] Button _equipButton;                    // 장비를 장착합니다.

    public EquipSlot _targetSlot;                               // 장비를 받기 위한 대상 슬롯입니다.
    private Equipment _selectedEquipment;                        // 인벤토리 칸에서 선택한 장비

    private void Awake()
    {
        //인스펙터상의 실수 확인용
        if (_slots.Count != 16)
            Debug.Log("InventoryPanel - 슬롯의 수가 맞지 않습니다.");
        _equipButton.onClick.AddListener(() =>
        {
            AddToSlot(_selectedEquipment);
        });
        _salvageButton.onClick.AddListener(() =>
        {
            _noticePanel.alpha = 1;
            _noticePanel.interactable = true;
            _noticePanel.blocksRaycasts = true;
        });
        _salvageAcceptButton.onClick.AddListener(() =>
        {
            Salvage(_selectedEquipment);
            _noticePanel.alpha = 0;
            _noticePanel.interactable = false;
            _noticePanel.blocksRaycasts = false;
        });
        _salvageRejectButton.onClick.AddListener(() =>
        {
            _noticePanel.alpha = 0;
            _noticePanel.interactable = false;
            _noticePanel.blocksRaycasts = false;
        });
        _confirmButton.onClick.AddListener(() =>
        {
            AddToSlot(_selectedEquipment);
        });
        _cancelButton.onClick.AddListener(() =>
        {
            RemoveFromSlot();
        });
        onInventoryChanged += Refresh;
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
        _infoDescription.text = equip.GetEquipStatusString();

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
    }

    /// <summary>
    /// 인자값으로 받은 장착 부위에 맞는 인벤토리를 엽니다.
    /// </summary>
    /// <param name="part">인벤토리를 확인하고자 하는 장착 부위</param>
    public void Open(Equip_Type part, int slotIndex)
    {
        //현재 장착 부위를 인자값으로 받아온 값과 일치시킵니다.
        if(part != 0)
        currentPart = part;
        int _currentSlotIndex = slotIndex;

        //인벤토리를 다시 불러옵니다.
        Refresh();

        SetPanelActiveValue(true);
    }

    /// <summary>
    /// 인벤토리 내부를 새로고침하는 메서드입니다.
    /// </summary>
    public void Refresh()
    {
        //장착 부위에 맞는 인벤토리를 가져옵니다.
        List<Equipment> list = _equipmentInventory.GetInventory(currentPart);

        //인벤토리 슬롯 길이만큼 다음 동작을 실행합니다.
        for(int i = 0; i<_slots.Count; i++)
        {
            //리스트에 있는 총 수보다 i가 적으면 그 리스트에서 장비 정보를 가져옵니다.
            if(i < list.Count)
            {
                SetSlot(_slots[i], list[i]);
            }

            //리스트에 있는 총 개수보다 i가 같거나 크면 비웁니다.
            else
            {
                ClearSlot(_slots[i]);
            }
        }
    }

    /// <summary>
    /// 인벤토리 슬롯에 장비를 지정합니다.
    /// </summary>
    /// <param name="slot">인벤토리 슬롯</param>
    /// <param name="equip">해당 슬롯과 index가 일치하는 인벤토리 내 장비</param>
    private void SetSlot(InventorySlot slot, Equipment equip)
    {
        //스프라이트를 동일하게 맞추고, 그 스프라이트를 활성화합니다.
        slot.icon.sprite = equip.icon;
        slot.icon.enabled = true;

        //장착 중이라는 것을 볼 수 있도록 장착 표기를 활성화합니다.
        slot.equipMark.SetActive(equip.isEquipped);

        //합성 슬롯에 등록했다는 것을 볼 수 있도록 합성 재료 표기를 활성화합니다.
        slot.FuseMark.SetActive(equip.isFusing);

        //해당 슬롯 버튼을 눌렀을 때의 동작을 전부 지우고 새로 추가합니다.
        slot.button.onClick.RemoveAllListeners();
        slot.button.onClick.AddListener(() =>
        {
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
        slot.FuseMark.SetActive(false);

        //해당 슬롯 버튼을 눌렀을 때 동작을 전부 지웁니다. 이 버튼을 눌렀을 때는 어떤 동작도 일어나면 안 됩니다.
        slot.button.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// 패널의 활성화 여부를 정합니다.
    /// </summary>
    /// <param name="value">활성화 여부</param>
    public void SetPanelActiveValue(bool value)
    {
        //참이면 1, 거짓이면 0으로 하여 참일 경우에만 보이게 합니다.
        _inventoryPanelGroup.alpha = value == true? 1 : 0;

        //상호작용 여부와 뒤 오브젝트와의 상호작용 제한은 참일 경우에만 활성화되도록 합니다.
        _inventoryPanelGroup.interactable = value;
        _inventoryPanelGroup.blocksRaycasts = value;
    }

    /// <summary>
    /// 장비를 분해할 경우의 골드와 스크랩 정산을 위한 코드입니다.
    /// </summary>
    /// <param name="equip">분해를 진행할 장비</param>
    public void Salvage(Equipment equip)
    {
        //(현재 테이블이 업데이트되지 않아 업데이트 이전 테이블에 맞추기 위한 값 40000 제거) 등급에 따른 분해 데이터를 받아옵니다.
        var breakData = DataManager.Instance.GetData<Equip_BreakData>(equip.equipment_Rarity - 40000);

        //강화 수치가 0이 아닐 경우, 테이블로부터 강화 수치 기준 데이터를 받아와 골드에 공식을 적용합니다.
        if (equip.equip_Upgrade > 0)
        {
            var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade);
            TestGoldAndScrapManager.Instance.testGold += (equip.equip_price + (int)(equip.equip_Upgrade * breakData.Equip_Break_Gold / upgradeData.Equip_Success_Prob));
            TestGoldAndScrapManager.Instance.testScrap += ((equip.equip_level + breakData.Equip_Break_Gold_Scrap) / 10);
        }
        //강화 수치가 0인 경우, 골드는 기본값으로 적용하고 스크랩만 계산하여 지급합니다.
        if (equip.equip_Upgrade == 0)
        {
            TestGoldAndScrapManager.Instance.testGold += equip.equip_price;
            TestGoldAndScrapManager.Instance.testScrap += ((equip.equip_level + breakData.Equip_Break_Gold_Scrap) / 10);
        }

        //현재 장비를 인벤토리에서 제거합니다.
        _equipmentInventory.RemoveEquipment(equip);

        //인벤토리를 갱신합니다.
        Refresh();
    }

    /// <summary>
    /// 인벤토리 패널에서 선택했을 경우 그 장비 정보를 넘기기 위한 슬롯을 정의합니다.
    /// </summary>
    /// <param name="slot">현재 장비를 받아와야 할 슬롯의 정보</param>
    public void SetTargetSlot(EquipSlot slot)
    {
        _targetSlot = slot;
        Open(slot.part, slot.slotIndex);
    }

    /// <summary>
    /// 합성 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="equip">합성 재료로 쓰기 위한 장비</param>
    private void AddToSlot(Equipment equip)
    {
        if (_targetSlot != null)
        {
            //대상 슬롯의 장비에 해당 장비를 집어넣습니다.
            _targetSlot.equipped = equip;

            //대상 슬롯의 이미지를 동일하게 만들고, 이미지를 활성화하며,
            //시각적으로 볼 수 있도록 레어도에 맞게 이미지 색을 변경합니다.
            _targetSlot.iconImage.sprite = equip.icon;
            _targetSlot.iconImage.enabled = true;
            _targetSlot.iconImage.color = RarityColor.GetColor((Rarity)equip.equipment_Rarity);

            if (_targetSlot.slotType == SlotType.EquipSlot)
            {
                Debug.Log($"{_targetSlot.equipped.equip_name} 장착 완료!");
                equip.isEquipped = true;
            }
            else if(_targetSlot.slotType == SlotType.FuseSlot)
            {
                equip.isFusing = true;
            }
        }
    }

    /// <summary>
    /// 합성 슬롯으로부터 장비를 제거합니다.
    /// </summary>
    private void RemoveFromSlot()
    {
        //타겟 슬롯이 null이 아니라면
        if (_targetSlot != null)
        {
            //슬롯에 장착된 장비를 null로 바꾸고
            _targetSlot.equipped = null;
            //스프라이트를 제거합니다.
            _targetSlot.iconImage.sprite = null;
        }
    }

    public Equipment GiveEquipmentData()
    {
        return _selectedEquipment;
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
}
