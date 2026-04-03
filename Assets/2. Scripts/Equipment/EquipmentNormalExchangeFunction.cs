using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentNormalExchangeFunction : BaseUI
{
    [Header("인벤토리 패널로부터 받아와야 하는 목록")]
    [SerializeField] InventoryPanel _inventoryPanel;
    [SerializeField] EquipmentInventory _equipmentInventory;

    [Header("장비 분해 / 판매 관련")]
    [SerializeField] Button _sellButton;                     // 장비 판매 시도를 위한 인벤토리 내 버튼입니다.

    [Header("일반 판매/분해 전용 패널 관련")]
    [SerializeField] TextMeshProUGUI _noticeTitle;           // 안내용 창의 안내 제목입니다.
    [SerializeField] TextMeshProUGUI _noticeMessage;         // 안내용 창의 안내 메세지입니다.
    [SerializeField] Button _AcceptButton;                   // 장비 판매 결정의 경우를 위한 안내창 내 Y 버튼입니다.
    [SerializeField] TextMeshProUGUI _buttonText;            // 판매 선택에 따라 변경하기 위한 동의 버튼의 텍스트입니다.
    [SerializeField] Button _RejectButton;                   // 장비 판매 취소의 경우를 위한 안내창 내 N 버튼입니다.

    private void Awake()
    {
        //판매 버튼 기능 추가 - 분해 상태 X. 안내패널 활성화
        _sellButton.onClick.AddListener(() =>
        {
            OnSell();
        });
        //안내패널 내 Y버튼 기능 추가 - 분해, 안내패널 비활성화
        _AcceptButton.onClick.AddListener(() =>
        {
            SellEquip(_inventoryPanel.GiveEquipmentData(), _inventoryPanel.GiveCurrentSlotData());
        });

        Close();
    }

    public void OnSell()
    {
        if (_inventoryPanel.CheckEquipmentSelected() == true)
        {
            if (_inventoryPanel.CheckLocked() == true || _inventoryPanel.CheckEquipped() == true)
            {
                return;
            }
            _noticeTitle.text = "판매 경고";
            _noticeMessage.text = "정말 판매하시겠습니까?";
            _buttonText.text = "판매";
            Open();
        }
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
    protected override void OnClose()
    {

    }

    protected override void OnOpen()
    {

    }
}
