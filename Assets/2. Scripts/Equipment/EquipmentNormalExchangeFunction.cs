using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentNormalExchangeFunction : BaseUI
{
    [Header("인벤토리 패널로부터 받아와야 하는 목록")]
    [SerializeField] InventoryPanel _inventoryPanel;
    [SerializeField] EquipmentInventory _equipmentInventory;

    [Header("장비 분해 / 판매 관련")]
    [SerializeField] Button _salvageButton;                  // 장비 분해 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _sellButton;                     // 장비 판매 시도를 위한 인벤토리 내 버튼입니다.

    [Header("일반 판매/분해 전용 패널 관련")]
    [SerializeField] TextMeshProUGUI _noticeTitle;           // 안내용 창의 안내 제목입니다.
    [SerializeField] TextMeshProUGUI _noticeMessage;         // 안내용 창의 안내 메세지입니다.
    [SerializeField] Button _AcceptButton;                   // 장비 분해/판매 결정의 경우를 위한 안내창 내 Y 버튼입니다.
    [SerializeField] TextMeshProUGUI _buttonText;            // 분해/판매 선택에 따라 변경하기 위한 동의 버튼의 텍스트입니다.
    [SerializeField] Button _RejectButton;                   // 장비 분해/판매 취소의 경우를 위한 안내창 내 N 버튼입니다.

    private bool _isSalvage = false;                          // 패널 출현 시 분해 버튼을 통해 열린 경우에만 참이 되는 변수

    private void Awake()
    {
        _salvageButton.onClick.AddListener(() =>
        {
            if (_inventoryPanel.CheckEquipmentSelected() == true)
            {
                if (_inventoryPanel.CheckLocked() == true)
                {
                    return;
                }
                _isSalvage = true;
                _noticeMessage.text = "정말 분해하시겠습니까?";
                _buttonText.text = "분해";
            }
        });
        //판매 버튼 기능 추가 - 분해 상태 X. 안내패널 활성화
        _sellButton.onClick.AddListener(() =>
        {
            if (_inventoryPanel.CheckEquipmentSelected() == true)
            {
                if (_inventoryPanel.CheckLocked() == true)
                {
                    return;
                }
                _isSalvage = false;
                _noticeMessage.text = "정말 판매하시겠습니까?";
                _buttonText.text = "판매";
            }
        });
        //안내패널 내 Y버튼 기능 추가 - 분해, 안내패널 비활성화
        _AcceptButton.onClick.AddListener(() =>
        {
            SalvageOrSellEquip(_inventoryPanel.GiveEquipmentData(), _inventoryPanel.GiveCurrentSlotData());
            UIManager.Instance.CloseTopPanel();
        });
        //안내패널 내 N버튼 기능 추가 - 안내패널 비활성화
        _RejectButton.onClick.AddListener(() =>
        {
            UIManager.Instance.CloseTopPanel();
        });

        Close();
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
    protected override void OnClose()
    {

    }

    protected override void OnOpen()
    {

    }
}
