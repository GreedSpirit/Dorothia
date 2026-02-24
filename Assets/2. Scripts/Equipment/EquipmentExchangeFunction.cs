using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//분해와 판매를 담당하는 클래스입니다.
public class EquipmentExchangeFunction : MonoBehaviour
{
    [Header("인벤토리 패널로부터 받아와야 하는 목록")]
    [SerializeField] InventoryPanel _inventoryPanel;
    [SerializeField] EquipmentInventory _equipmentInventory;

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
    [SerializeField] TextMeshProUGUI _multiModifyTitleText;  // 해당 패널의 제목입니다.
    [SerializeField] TextMeshProUGUI _multiModifyAcceptText; // 일괄 분해 및 일괄 판매의 진행 버튼 텍스트입니다.
    [SerializeField] Toggle _includeUpgradedButton;          // 강화 장비를 포함할지 여부를 결정지을 버튼입니다.
    [SerializeField] Toggle _normalButton;                    // 일반 등급 버튼입니다. 일반 등급의 장비를 일괄 선택합니다.
    [SerializeField] Toggle _uncommonButton;                  // 희귀 등급 버튼입니다. 희귀 등급의 장비를 일괄 선택합니다.
    [SerializeField] Toggle _rareButton;                      // 레어 등급 버튼입니다. 레어 등급의 장비를 일괄 선택합니다.
    [SerializeField] Toggle _legendaryButton;                 // 전설 등급 버튼입니다. 전설 등급의 장비를 일괄 선택합니다.
    [SerializeField] Toggle _mythticButton;                   // 신화 등급 버튼입니다. 신화 등급의 장비를 일괄 선택합니다.
    [SerializeField] Button _multiAcceptButton;
    [SerializeField] Button _multiRejectButton;

    [SerializeField] Sprite _buttonAccepted;                  // 필터버튼 적용 시의 버튼이미지입니다.
    [SerializeField] Sprite _buttonNotAccepted;               // 필터버튼 해제 시의 버튼이미지입니다.

    private bool _isSalvage = false;                          // 패널 출현 시 분해 버튼을 통해 열린 경우에만 참이 되는 변수
    private bool _isIncludeUpgraded = false;                  // 강화된 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeNormal = false;                    // 일반 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeUncommon = false;                  // 희귀 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeRare = false;                      // 레어 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeLegendary = false;                 // 전설 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeMythtic = false;                   // 신화 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.

    private void Awake()
    {
        _salvageButton.onClick.AddListener(() =>
        {
            _isSalvage = true;
            _noticeMessage.text = "정말 분해하시겠습니까?";
            _buttonText.text = "분해";
            _noticePanel.alpha = 1;
            _noticePanel.interactable = true;
            _noticePanel.blocksRaycasts = true;
        });
        //판매 버튼 기능 추가 - 분해 상태 X. 안내패널 활성화
        _sellButton.onClick.AddListener(() =>
        {
            _isSalvage = false;
            _noticeMessage.text = "정말 판매하시겠습니까?";
            _buttonText.text = "판매";
            _noticePanel.alpha = 1;
            _noticePanel.interactable = true;
            _noticePanel.blocksRaycasts = true;
        });
        //안내패널 내 Y버튼 기능 추가 - 분해, 안내패널 비활성화
        _AcceptButton.onClick.AddListener(() =>
        {
            SalvageOrSellEquip(_inventoryPanel.GiveEquipmentData(), _inventoryPanel.GiveCurrentSlotData());
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
        _salvageAtOnceButton.onClick.AddListener(() =>
        {
            _isSalvage = true;
            SetPanelActiveValue(true);
            MultiPanelFunction();
        });
        _sellAtOnceButton.onClick.AddListener(() =>
        {
            _isSalvage = false;
            SetPanelActiveValue(true);
            MultiPanelFunction();
        });
        _includeUpgradedButton.onValueChanged.AddListener(IncludeUpgrade);
        _normalButton.onValueChanged.AddListener(IncludeNormal);
        _uncommonButton.onValueChanged.AddListener(IncludeUncommon);
        _rareButton.onValueChanged.AddListener(IncludeRare);
        _legendaryButton.onValueChanged.AddListener(IncludeLegendary);
        _mythticButton.onValueChanged.AddListener(IncludeMythtic);
    }

    /// <summary>
    /// 판매 버튼 또는 분해 버튼으로 패널을 열었을 때, 해당 패널에서 동의(수락)버튼을 눌렀을 시의 동작입니다.
    /// </summary>
    /// <param name="equip">팔거나 분해할 장비</param>
    public void SalvageOrSellEquip(Equipment equip, InventorySlot slot)
    {
        //분해 버튼을 통해 해당 창을 열었으면 분해를 진행합니다.
        if (_isSalvage == true)
        {
            Salvage(equip, slot);
        }
        //그것이 아니라면 판매를 진행합니다.
        else
        {
            SellEquip(equip, slot);
        }
    }

    public void IncludeUpgrade(bool value)
    {
        _isIncludeUpgraded = value;
    }
    public void IncludeNormal(bool value)
    {
        _isIncludeNormal = value;
    }
    public void IncludeUncommon(bool value)
    {
        _isIncludeUncommon = value;
    }
    public void IncludeRare(bool value)
    {
        _isIncludeRare = value;
    }
    public void IncludeLegendary(bool value)
    {
        _isIncludeLegendary = value;
    }
    public void IncludeMythtic(bool value)
    {
        _isIncludeMythtic = value;
    }

    /// <summary>
    /// 장비를 분해할 경우의 골드와 스크랩 정산을 위한 코드입니다.
    /// </summary>
    /// <param name="equip">분해를 진행할 장비</param>
    public void Salvage(Equipment equip, InventorySlot slot)
    {
        //계산기를 통해 구한 스크랩과 골드를 각각 추가합니다.
        TestGoldAndScrapManager.Instance.testScrap += ItemCalculator.SalvageScrapCalculate(equip);
        TestGoldAndScrapManager.Instance.testGold += ItemCalculator.SalvageGoldCalculate(equip);

        //현재 장비를 인벤토리에서 제거합니다.
        _equipmentInventory.RemoveEquipment(equip);

        //인벤토리를 갱신합니다.
        _inventoryPanel.onInventoryChanged.Invoke();

        //선택하고 있던 슬롯에서 아이템이 사라졌으니, 해당 슬롯의 선택을 중지합니다.
        slot.selectMark.SetActive(false);
        _inventoryPanel.ClearCurrentSlot();
    }

    /// <summary>
    /// 장비 판매 시의 기능입니다.
    /// </summary>
    /// <param name="equip">판매할 장비</param>
    public void SellEquip(Equipment equip, InventorySlot slot)
    {
        //계산기를 통해 구한 골드를 추가합니다.
        TestGoldAndScrapManager.Instance.testGold += ItemCalculator.SellCalculate(equip);
        
        //현재 장비를 인벤토리에서 제거합니다.
        _equipmentInventory.RemoveEquipment(equip);
        //인벤토리를 갱신합니다.
        _inventoryPanel.onInventoryChanged.Invoke();

        //선택하고 있던 슬롯에서 아이템이 사라졌으니, 해당 슬롯의 선택을 중지합니다.
        slot.selectMark.SetActive(false);
        _inventoryPanel.ClearCurrentSlot();
    }
    

    public void MultiPanelFunction()
    {
        if(_isSalvage == true)
        {
            _multiModifyTitleText.text = "일괄 분해 필터 설정";
            _multiModifyAcceptText.text = "일괄 분해";
            _multiAcceptButton.onClick.AddListener(() =>
            {
                MultiSalvageOrSell(_inventoryPanel.currentPart);
            });
        }
        else
        {
            _multiModifyTitleText.text = "일괄 판매 필터 설정";
            _multiModifyAcceptText.text = "일괄 판매";
            _multiAcceptButton.onClick.AddListener(() =>
            {
                MultiSalvageOrSell(_inventoryPanel.currentPart);
            });
        }
    }

    public void MultiSalvageOrSell(Equip_Type type)
    {
        List<Equipment> inventory = _equipmentInventory.GetInventory(type);
        List<Equipment> target = new List<Equipment>();
        foreach(Equipment item in inventory)
        {
            target = ItemFilter.FindTargetEquipment(inventory, _isIncludeUpgraded, _isIncludeNormal, _isIncludeUncommon, _isIncludeRare, _isIncludeLegendary, _isIncludeMythtic);
        }
        foreach(Equipment item in target)
        {
            if(_isSalvage == true)
            {
                Salvage(item, _inventoryPanel.GiveTargetSlotData(item));
            }
            else
            {
                SellEquip(item, _inventoryPanel.GiveTargetSlotData(item));
            }
        }
    }

    /// <summary>
    /// 패널의 활성화 여부를 정합니다.
    /// </summary>
    /// <param name="value">활성화 여부</param>
    public void SetPanelActiveValue(bool value)
    {
        //참이면 1, 거짓이면 0으로 하여 참일 경우에만 보이게 합니다.
        _multiSelectPanel.alpha = value == true ? 1 : 0;

        //상호작용 여부와 뒤 오브젝트와의 상호작용 제한은 참일 경우에만 활성화되도록 합니다.
        _multiSelectPanel.interactable = value;
        _multiSelectPanel.blocksRaycasts = value;
    }
}
