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

    [Header("기타 버튼")]
    [SerializeField] Button _autoEquipButtons;                   // 자동장착 버튼입니다.
    [SerializeField] Button _lockButton;                         // 잠금 버튼입니다.
    [SerializeField] TextMeshProUGUI _lockButtonText;            // 잠금 버튼의 텍스트입니다.

    [Header("장비 분해 / 판매 관련")]
    [SerializeField] Button _salvageButton;                  // 장비 분해 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _sellButton;                     // 장비 판매 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _salvageAtOnceButton;            // 일괄 분해 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _sellAtOnceButton;               // 일괄 판매 시도를 위한 인벤토리 내 버튼입니다.

    [Header("일반 판매/분해 전용 패널 관련")]
    [SerializeField] CanvasGroup _noticePanel;               // 장비 분해를 시도할 때 나타나도록 할 안내용 창입니다.
    [SerializeField] TextMeshProUGUI _noticeMessage;         // 안내용 창의 안내 메세지입니다.
    [SerializeField] Button _AcceptButton;                   // 장비 분해/판매 결정의 경우를 위한 안내창 내 Y 버튼입니다.
    [SerializeField] TextMeshProUGUI _buttonText;            // 분해/판매 선택에 따라 변경하기 위한 동의 버튼의 텍스트입니다.
    [SerializeField] Button _RejectButton;                   // 장비 분해/판매 취소의 경우를 위한 안내창 내 N 버튼입니다.

    [Header("일괄 판매/분해 전용 패널 관련")]
    [SerializeField] CanvasGroup _multiSelectPanel;          // 일괄 판매나 분해를 눌렀을 시 조건을 분류하기 위한 패널입니다.
    [SerializeField] Button _includeUpgradedButton;          // 강화 장비를 포함할지 여부를 결정지을 버튼입니다.
    [SerializeField] Button NormalButton;                    // 일반 등급 버튼입니다. 일반 등급의 장비를 일괄 선택합니다.
    [SerializeField] Button UncommonButton;                  // 희귀 등급 버튼입니다. 희귀 등급의 장비를 일괄 선택합니다.
    [SerializeField] Button RareButton;                      // 레어 등급 버튼입니다. 레어 등급의 장비를 일괄 선택합니다.
    [SerializeField] Button LegendaryButton;                 // 전설 등급 버튼입니다. 전설 등급의 장비를 일괄 선택합니다.
    [SerializeField] Button MythticButton;                   // 신화 등급 버튼입니다. 신화 등급의 장비를 일괄 선택합니다.

    [Header("장비 정보 출력용")]
    [SerializeField] GameObject _infoPanel;                  // 정보를 담을 패널
    [SerializeField] Image _infoIcon;                        // 정보 패널에서의 장비 아이콘 출력용 이미지
    [SerializeField] Sprite _infoIconBaseSprite;             // 정보 패널에서 장비 아이콘의 기본 상태용 스프라이트
    [SerializeField] TextMeshProUGUI _infoName;              // 정보 패널에서 장비의 이름를 나타낼 텍스트
    [SerializeField] TextMeshProUGUI _infoDescription;       // 정보 패널에서 장비의 정보를 나타낼 텍스트

    [SerializeField] Button _confirmButton;                  // 합성 전용 확인버튼
    [SerializeField] Button _cancelButton;                   // 합성 전용 취소버튼
    [SerializeField] Button _equipButton;                    // 장비를 장착합니다.

    public EquipSlot targetSlot;                             // 장비를 받기 위한 대상 슬롯입니다.
    private Equipment _selectedEquipment;                    // 인벤토리 칸에서 선택한 장비
    private bool isSalvage = false;                          // 패널 출현 시 분해 버튼을 통해 열린 경우에만 참이 되는 변수

    private InventorySlot _currentSelectedSlot;

    private void Awake()
    {
        //인스펙터상의 실수 확인용
        if (_slots.Count != 16)
            Debug.LogWarning("InventoryPanel - 슬롯의 수가 맞지 않습니다.");
        _equipButton.onClick.AddListener(() =>
        {
            AddToSlot(_selectedEquipment);
        });
        //분해 버튼 기능 추가 - 분해 상태 O. 안내패널 활성화
        _salvageButton.onClick.AddListener(() =>
        {
            isSalvage = true;
            _noticeMessage.text = "정말 분해하시겠습니까?";
            _buttonText.text = "분해";
            _noticePanel.alpha = 1;
            _noticePanel.interactable = true;
            _noticePanel.blocksRaycasts = true;
        });
        //판매 버튼 기능 추가 - 분해 상태 X. 안내패널 활성화
        _sellButton.onClick.AddListener(() =>
        {
            isSalvage = false;
            _noticeMessage.text = "정말 판매하시겠습니까?";
            _buttonText.text = "판매";
            _noticePanel.alpha = 1;
            _noticePanel.interactable = true;
            _noticePanel.blocksRaycasts = true;
        });
        //안내패널 내 Y버튼 기능 추가 - 분해, 안내패널 비활성화
        _AcceptButton.onClick.AddListener(() =>
        {
            SalvageOrSellEquip(_selectedEquipment);
            _noticePanel.alpha = 0;
            _noticePanel.interactable = false;
            _noticePanel.blocksRaycasts = false;
        });
        //안내패널 내 N버튼 기능 추가 - 안내패널 비활성화
        _RejectButton.onClick.AddListener(() =>
        {
            _noticePanel.alpha = 0;
            _noticePanel.interactable = false;
            _noticePanel.blocksRaycasts = false;
        });
        //확인버튼 기능 추가 - 슬롯에 해당 장비 추가
        _confirmButton.onClick.AddListener(() =>
        {
            AddToSlot(_selectedEquipment);
        });
        //취소버튼 기능 추가 - 해당 슬롯에서 장비 제거
        _cancelButton.onClick.AddListener(() =>
        {
            RemoveFromSlot();
        });
        //자동장착 기능 추가 - 현재 인벤토리 기준 장비 장착
        _autoEquipButtons.onClick.AddListener(() =>
        {
            AutoEquip(currentPart);
            SetPanelActiveValue(false);
            Refresh();
        });
        //인벤토리 변화 시 발생하는 이벤트에 새로고침 메서드 추가
        onInventoryChanged += Refresh;
        onInventoryChanged += ResetInfo;
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
        if(equip.isLocked == true)
        {
            _lockButtonText.text = "Unlock";
            _lockButton.onClick.RemoveAllListeners();
            _lockButton.onClick.AddListener(() =>
            {
                UnlockEquipment(_selectedEquipment);
            });
        }
        else
        {
            _lockButtonText.text = "Lock";
            _lockButton.onClick.RemoveAllListeners();
            _lockButton.onClick.AddListener(() =>
            {
                LockEquipment(_selectedEquipment);
            });
        }
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
    /// 장비를 잠금 상태로 변경합니다.
    /// </summary>
    /// <param name="equip">잠글 장비</param>
    private void LockEquipment(Equipment equip)
    {
        //고른 장비가 없으면 반환합니다.
        if (_selectedEquipment == null)
        {
            return;
        }
        //반환되지 않았다면 고른 장비가 존재한다는 것.
        //잠겨있는 경우에는 잠금을 해제합니다.
        if(_selectedEquipment.isLocked == false)
        {
            equip.isLocked = true;
        }
        Refresh();
        ResetInfo();
        OnClickItem(equip);
    }

    /// <summary>
    /// 장비를 잠금 해제 상태로 변경합니다.
    /// </summary>
    /// <param name="equip">잠금 해제할 장비</param>
    private void UnlockEquipment(Equipment equip)
    {
        //고른 장비가 없으면 반환합니다.
        if (_selectedEquipment == null)
        {
            return;
        }
        //반환되지 않았다면 고른 장비가 존재한다는 것.
        //잠겨있는 경우에는 잠금을 해제합니다.
        if(_selectedEquipment.isLocked == true)
        {
            equip.isLocked = false;
        }
        Refresh();
        ResetInfo();
        OnClickItem(equip);
    }

    /// <summary>
    /// 자동 장착 시의 동작입니다.
    /// </summary>
    /// <param name="part">장착 부위</param>
    public void AutoEquip(Equip_Type part)
    {
        //인벤토리로부터 해당 장착 부위의 장비 리스트를 가져옵니다.
        List<Equipment> list = _equipmentInventory.GetInventory(part);

        //리스트 안에 들어있는 게 없다면?
        if (list.Count == 0)
        {
            //아무것도 없다는 것이므로 그냥 반환시킵니다.
            return;
        }

        //장착 중인 장비를 확인합니다.
        int currentEquipWeight = targetSlot.equipped != null? targetSlot.equipped.GetEquipScore() : 0;
        //장착할 장비를 선언하고, 장착 중인 장비가 있다면 해당 장비를 넣습니다. (없어도 null이 들어갈 것입니다.)
        Equipment equipmentToEquip = targetSlot.equipped;

        for(int i = 0; i < list.Count; i++)
        {
            //이미 장착 중인 장비라면, 해당 칸에 이미 장착되었거나, 반지의 경우 다른 칸에 이미 장착된 경우입니다.
            //그러니 다음 단계로 넘어갑니다.
            if (list[i].isEquipped == true)
            {
                continue;
            }
            //현재 칸의 장비 점수를 체크합니다.
            int score = list[i].GetEquipScore();

            //해당 장비 점수가 현재의 가중치보다 높을 경우
            if(score > currentEquipWeight)
            {
                //가중치를 해당 점수로 두고
                currentEquipWeight = score;
                //해당 장비를 장착할 장비로 선언한 후
                equipmentToEquip = list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //해당 장비 점수가 현재의 가중치보다 낮을 경우
            else if(score < currentEquipWeight)
            {
                //바로 다음 단계로 넘어갑니다.
                continue;
            }

            //여기 도착했다는 건 장비 점수가 같다는 이야기입니다.
            //점수가 같은데 장비가 없다는 건 가중치 0, 점수 0의 장비라는 것.
            if(equipmentToEquip == null)
            {
                //장비가 없다는 뜻이니 우선 장착할 장비로 둡니다.
                equipmentToEquip = list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //장착할 장비가 존재한다면, 점수가 같을 때 처음 봐야 하는 것은 등급입니다.
            //등급이 서로 다를 경우, 장착할 장비를 결정합니다.
            if (equipmentToEquip.equipment_Rarity != list[i].equipment_Rarity)
            {
                //등급이 높은 쪽이 장착할 대상이 됩니다.
                equipmentToEquip = equipmentToEquip.equipment_Rarity > list[i].equipment_Rarity ?
                    equipmentToEquip : list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //장비가 존재하고, 등급도 같다면 다음에 봐야 하는 것은 강화도입니다.
            if (equipmentToEquip.equip_Upgrade != list[i].equip_Upgrade)
            {
                //강화도가 높은 쪽이 장착할 대상이 됩니다.
                equipmentToEquip = equipmentToEquip.equip_Upgrade > list[i].equip_Upgrade?
                    equipmentToEquip : list[i];
                //다음 단계로 넘어갑니다.
                continue;
            }
            //장비가 존재하고, 등급도 같으며, 강화도마저 같으면 획득한 순서를 살펴봅니다.
            //다만, GUID가 같으면 같은 장비인 것이고, GUID가 다르면 list[i]쪽이 더 나중에 획득한 장비입니다. ( 현재 배치순서 변경 불가 )
            //따라서, 여기까지 왔으면 다음 단계로 넘어갑니다.
        }
        //전부 진행했다면, 해당 장비를 장착합니다.
        AddToSlot(equipmentToEquip);
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
        onInventoryChanged.Invoke();

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
        slot.fuseMark.SetActive(equip.isFusing);

        //잠겨있는 장비인 경우, 잠금 상태인 것을 확인할 수 있도록 자물쇠 모양 표기를 활성화합니다.
        slot.lockedMark.SetActive(equip.isLocked);

        //해당 슬롯이 지금 내가 선택하고 있는 슬롯이 맞다면, 해당 슬롯을 활성화합니다.
        slot.selectMark.SetActive(_currentSelectedSlot == slot);

        //해당 슬롯에 존재하는 장비의 강화도를 표기해줍니다.
        slot.UpgradeValue.text = $"+{equip.equip_Upgrade}";

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
    /// 판매 버튼 또는 분해 버튼으로 패널을 열었을 때, 해당 패널에서 동의(수락)버튼을 눌렀을 시의 동작입니다.
    /// </summary>
    /// <param name="equip">팔거나 분해할 장비</param>
    public void SalvageOrSellEquip(Equipment equip)
    {
        //분해 버튼을 통해 해당 창을 열었으면 분해를 진행합니다.
        if(isSalvage == true)
        {
            Salvage(equip);
        }
        //그것이 아니라면 판매를 진행합니다.
        else
        {
            SellEquip(equip);
        }
    }

    /// <summary>
    /// 장비를 분해할 경우의 골드와 스크랩 정산을 위한 코드입니다.
    /// </summary>
    /// <param name="equip">분해를 진행할 장비</param>
    public void Salvage(Equipment equip)
    {
        //등급에 따른 분해 데이터를 받아옵니다.
        var breakData = DataManager.Instance.GetData<Equip_BreakData>(equip.equipment_Rarity);

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
        onInventoryChanged.Invoke();
    }

    /// <summary>
    /// 장비 판매 시의 기능입니다.
    /// </summary>
    /// <param name="equip">판매할 장비</param>
    public void SellEquip(Equipment equip)
    {
        //해당 장비의 강화 단계를 기준으로 데이터를 먼저 불러옵니다.
        var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade);

        //판매가격인 골드 지역변수를 선언합니다.
        int equipGold = 0;

        //강화 단계가 50(현재 최대치)이거나, 모종의 툴을 사용하여 그 이상이 나왔을 경우
        //오류를 방지하기 위해 49단계 기준으로 진행합니다.
        if(equip.equip_Upgrade >= 50)
        {
            //공식 : (기본 판매가격 * 장비 장착 레벨 * 장비 등급에 따른 가중치) + (강화 평균 소모 골드 * 0.2)
            //강화 평균 소모 골드 : 해당 단계 기준 1회 강화 비용(장비 기본 판매가격 * (현재 강화단계 + 1)값의 (골드데이터 상의 배율)제곱 * 등급에 따른 가중치) / 성공 확률
            int firstGold = Mathf.RoundToInt
                (
                    equip.equip_price * equip.equip_level * 
                    GetRarityWeight((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                );
            int secondGold = Mathf.RoundToInt
                (
                    Mathf.RoundToInt(equip.equip_price * Mathf.Pow(50, DataManager.Instance.GetData<Equip_Upgrade_GoldData>(50).Equip_Upgrade_Value)) *
                    equip.GetEnchantWeightByRarity((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                    / DataManager.Instance.GetData<Equip_UpgradeData>(50).Equip_Success_Prob * 0.2f
                );

            equipGold = firstGold + secondGold;
        }
        else
        {
            //공식 : (기본 판매가격 * 장비 장착 레벨 * 장비 등급에 따른 가중치) + (강화 평균 소모 골드 * 0.2)
            //강화 평균 소모 골드 : 해당 단계 기준 1회 강화 비용(장비 기본 판매가격 * (현재 강화단계 + 1)값의 (골드데이터 상의 배율)제곱 * 등급에 따른 가중치) / 성공 확률
            int firstGold = Mathf.RoundToInt
                (
                    equip.equip_price * equip.equip_level *
                    GetRarityWeight((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                );
            int secondGold = Mathf.RoundToInt
                (
                    Mathf.RoundToInt(equip.equip_price * Mathf.Pow(equip.equip_Upgrade + 1, DataManager.Instance.GetData<Equip_Upgrade_GoldData>(equip.equip_Upgrade + 1).Equip_Upgrade_Value)) *
                    equip.GetEnchantWeightByRarity((Rarity)DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank)
                    / DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade + 1).Equip_Success_Prob * 0.2f
                );

            equipGold = firstGold + secondGold;
        }

        //계산된 만큼 골드를 획득합니다.
        TestGoldAndScrapManager.Instance.testGold += equipGold;
        //현재 장비를 인벤토리에서 제거합니다.
        _equipmentInventory.RemoveEquipment(equip);
        //인벤토리를 갱신합니다.
        onInventoryChanged.Invoke();
    }

    /// <summary>
    /// 인벤토리 패널에서 선택했을 경우 그 장비 정보를 넘기기 위한 슬롯을 정의합니다.
    /// </summary>
    /// <param name="slot">현재 장비를 받아와야 할 슬롯의 정보</param>
    public void SetTargetSlot(EquipSlot slot)
    {
        targetSlot = slot;
        Open(slot.part, slot.slotIndex);
    }

    /// <summary>
    /// 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="equip">장착 혹은 재료로 사용할 장비</param>
    private void AddToSlot(Equipment equip)
    {
        if (targetSlot != null)
        {
            //합성 슬롯에 이미 장착 중인 장비를 넣으려고 할 경우, 경고를 출력하고 반환합니다.
            if (targetSlot.slotType == SlotType.FuseSlot && equip.isEquipped == true)
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
            targetSlot.iconImage.color = RarityColor.GetColor((Rarity)equip.equipment_Rarity);

            //장비 슬롯이라면 장비를 장착합니다.
            if (targetSlot.slotType == SlotType.EquipSlot)
            {
                equip.SetEquipped(targetSlot.slotIndex);
            }
            //합성 슬롯이라면 합성 재료로 사용중임을 표시합니다.
            else if(targetSlot.slotType == SlotType.FuseSlot)
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
    /// 등급에 따른 판매 시의 가중치를 구합니다.
    /// </summary>
    /// <param name="rarity">판매하려는 장비의 등급</param>
    /// <returns></returns>
    private float GetRarityWeight(Rarity rarity)
    {
        switch(rarity)
        {
            case Rarity.Normal:
                return 1;
            case Rarity.Uncommon:
                return 1.5f;
            case Rarity.Rare:
                return 2.5f;
            case Rarity.Legendary:
                return 5;
            case Rarity.Mythtic:
                return 10;
            default:
                return 0.5f;
        }
    }

    /// <summary>
    /// 장비 데이터를 전송합니다.
    /// </summary>
    /// <returns>현재 선택된 장비</returns>
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
