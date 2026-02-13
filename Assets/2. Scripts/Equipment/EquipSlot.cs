using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    EquipSlot, FuseSlot
}
public class EquipSlot : MonoBehaviour
{
    public SlotType slotType;                      // 해당 슬롯이 합성용인지, 장비 장착용인지 표기하기 위한 슬롯입니다.
    public Equip_Type part;                     // 해당 슬롯이 담당할 장착 부위입니다.
    public int slotIndex;                          // 해당 슬롯의 번호입니다. (반지 2슬롯을 대비하여 생성)
    
    public Button button;                          // 해당 슬롯의 버튼입니다.
    public Image iconImage;                        // 해당 슬롯에 사용될 아이콘 이미지입니다.
    public Sprite iconSprite;                      // 아무것도 장착되지 않은 상태의 기본 스프라이트입니다.

    public Equipment equipped;                     // 현재 장착되어있는 장비입니다.

    [SerializeField] EquipmentUI equipmentUI;      // 장착 장비 UI입니다. 해당 슬롯의 장착 부위에 맞는 인벤토리 칸을 열기 위해 필요합니다.

    /// <summary>
    /// 해당 슬롯을 눌렀을 때 사용할 메서드입니다.
    /// </summary>
    public void OnClickSlot()
    {
        equipmentUI.SetSlot(this);
        //이 슬롯을 기반으로, 해당 슬롯에 맞는 인벤토리를 열도록 EquipmentUI의 메서드를 실행합니다.
        equipmentUI.OpenInventory(this);
    }

    /// <summary>
    /// 슬롯 UI를 장비로 업데이트
    /// </summary>
    public void UpdatePartUI()
    {
        if (equipped == null)
        {
            // 장비가 없으면 아이콘 비활성화
            iconImage.enabled = false;
            return;
        }

        // 장비가 있으면 아이콘 표시
        iconImage.sprite = equipped.icon;
        iconImage.enabled = true;

        iconImage.color = RarityColor.GetColor((Rarity)equipped.equipment_Rarity); // 성공 시 한눈에 확인하는 용도.
    }

    /// <summary>
    /// 슬롯 초기화(빈 슬롯으로 만들기)
    /// </summary>
    public void ClearSlot()
    {
        equipped = null;
        iconImage.sprite = iconSprite;
        iconImage.color = Color.white;
    }
}
