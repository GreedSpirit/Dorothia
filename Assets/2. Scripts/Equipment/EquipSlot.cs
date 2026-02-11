using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{
    public EquipmentPart part;                     // 해당 슬롯이 담당할 장착 부위입니다.
    public int slotIndex;                          // 해당 슬롯의 번호입니다. (반지 2슬롯을 대비하여 생성)
    
    public Button button;                          // 해당 슬롯의 버튼입니다.
    public Image iconImage;                        // 해당 슬롯에 사용될 아이콘 이미지입니다.
    public GameObject equippedMark;                // 해당 슬롯에 장착 여부를 적용할 게임오브젝트입니다.

    public Equipment equipped;                     // 현재 장착되어있는 장비입니다.

    [SerializeField] EquipmentUI equipmentUI;      // 장착 장비 UI입니다. 해당 슬롯의 장착 부위에 맞는 인벤토리 칸을 열기 위해 필요합니다.

    /// <summary>
    /// 해당 슬롯을 눌렀을 때 사용할 메서드입니다.
    /// </summary>
    public void OnClickSlot()
    {
        //이 슬롯을 기반으로, 해당 슬롯에 맞는 인벤토리를 열도록 EquipmentUI의 메서드를 실행합니다.
        equipmentUI.OpenInventory(this);
    }
}
