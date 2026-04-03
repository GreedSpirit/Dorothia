using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//분해와 판매를 담당하는 클래스입니다.
public class EquipmentExchangeFunction : BaseUI
{
    [Header("인벤토리 패널로부터 받아와야 하는 목록")]
    [SerializeField] InventoryPanel _inventoryPanel;
    [SerializeField] EquipmentInventory _equipmentInventory;

    [Header("장비 분해 / 판매 관련")]
    [SerializeField] Button _sellAtOnceButton;               // 일괄 판매 시도를 위한 인벤토리 내 버튼입니다.

    [Header("일괄 판매/분해 전용 패널 관련")]
    [SerializeField] CanvasGroup _multiSelectPanel;          // 일괄 판매를 눌렀을 시 조건을 분류하기 위한 패널입니다.
    [SerializeField] TextMeshProUGUI _multiModifyTitleText;  // 해당 패널의 제목입니다.
    [SerializeField] TextMeshProUGUI _multiModifyDescriptionText;      // 일괄 "분해"인지 "판매"인지 구분해야 하는 텍스트입니다.
    [SerializeField] TextMeshProUGUI _multiModifyAcceptText; // 일괄 판매의 진행 버튼 텍스트입니다.
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

    private bool _isIncludeUpgraded = false;                  // 강화된 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeNormal = false;                    // 일반 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeUncommon = false;                  // 희귀 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeRare = false;                      // 레어 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeLegendary = false;                 // 전설 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.
    private bool _isIncludeMythtic = false;                   // 신화 등급 장비의 대상 포함 여부를 나타내는 bool형 매개변수입니다.

    private void Awake()
    {
        _sellAtOnceButton.onClick.AddListener(() =>
        {
            MultiPanelFunction();
        });
        _includeUpgradedButton.onValueChanged.AddListener(IncludeUpgrade);
        _normalButton.onValueChanged.AddListener(IncludeNormal);
        _uncommonButton.onValueChanged.AddListener(IncludeUncommon);
        _rareButton.onValueChanged.AddListener(IncludeRare);
        _legendaryButton.onValueChanged.AddListener(IncludeLegendary);
        _mythticButton.onValueChanged.AddListener(IncludeMythtic);
        //일괄 기능의 동작 버튼의 기능을 모두 없애고, 다중 분해/판매 기능과 패널의 비활성화 기능을 추가합니다.
        _multiAcceptButton.onClick.RemoveAllListeners();
        _multiAcceptButton.onClick.AddListener(() =>
        {
            MultiSell(_inventoryPanel.currentPart);
            //작업 완료 후 해당 창을 닫거나 하는 것의 이슈는 작성되지 않아, 임시로 완료 후 닫도록 설정합니다.
            //추후, 완료 후에는 안내 창을 띄워야 한다 같은 지시 사항이 존재할 경우 변경될 수 있습니다.
            UIManager.Instance.CloseTopPanel();
        });
        //일괄 기능의 취소 버튼의 기능을 모두 없애고, 패널 비활성화 기능을 추가합니다.
        _multiRejectButton.onClick.RemoveAllListeners();
        _multiRejectButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseTopPanel();
        });
        Close();
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
    /// 장비 판매 시의 기능입니다.
    /// </summary>
    /// <param name="equip">판매할 장비</param>
    public void SellEquip(Equipment equip, InventorySlot slot)
    {
        //계산기를 통해 구한 골드를 추가합니다.
        ExchangeManager.Instance.GetMoney(MoneyType.Gold, (BigInteger)ItemCalculator.SellCalculate(equip));
        
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
        _multiModifyTitleText.text = "일괄 판매 필터 설정";
        _multiModifyAcceptText.text = "일괄 판매 실행";
        _multiModifyDescriptionText.text = "강화된 장비도 판매 대상에 포함";
    }

    public void MultiSell(Equip_Type type)
    {
        List<Equipment> inventory = _equipmentInventory.GetInventory(type);
        List<Equipment> target = new List<Equipment>();
        foreach(Equipment item in inventory)
        {
            target = ItemFilter.FindTargetEquipment(inventory, _isIncludeUpgraded, _isIncludeNormal, _isIncludeUncommon, _isIncludeRare, _isIncludeLegendary, _isIncludeMythtic);
        }
        foreach(Equipment item in target)
        {
            SellEquip(item, _inventoryPanel.GiveTargetSlotData(item));
        }
    }

    protected override void OnOpen()
    {
        
    }

    protected override void OnClose()
    {
        
    }
}
