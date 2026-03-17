using UnityEngine;
using UnityEngine.UI;

public class InventoryPartButton : MonoBehaviour
{
    [SerializeField] EquipSlot _slot;                    // 통상 슬롯
    [SerializeField] EquipSlot _ringSlot;                // 반지 전용 2번째 슬롯
    [SerializeField] InventoryPanel _inventoryPanel;      // 인벤토리 관련 기능을 갖춘 인벤토리 패널
    [SerializeField] Image _thisImage;
    public Sprite _currentPartSprite;          // 현재 열린 인벤토리와 같은 EquipPart일 때의 이미지
    public Sprite _PartSprite;                 // 통상 상태의 이미지

    private void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            SendSlot();
        });
    }

    private void OnEnable()
    {
        _inventoryPanel.onInventoryChanged += ChangeImage;
    }

    private void OnDisable()
    {
        _inventoryPanel.onInventoryChanged -= ChangeImage;
    }

    /// <summary>
    /// 인벤토리 슬롯에 자기 자신을 보냅니다.
    /// </summary>
    public void SendSlot()
    {
        if(_inventoryPanel.status == InventoryStatus.Fuse)
        {
            _inventoryPanel.OpenInventory(_slot.part, _slot.slotIndex);
        }
        //반지 슬롯이 아닐 경우 통상 진행
        else if(_inventoryPanel.status == InventoryStatus.Equip)
        {
            _inventoryPanel.targetSlot = _slot;
            _inventoryPanel.OpenInventory(_slot.part, _slot.slotIndex);
        }
    }

    public void ChangeImage()
    {
        _thisImage.sprite = _slot.part == _inventoryPanel.currentPart?
            _currentPartSprite:_PartSprite;
    }
}
