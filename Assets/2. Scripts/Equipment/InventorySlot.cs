using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Button button;                         // 인벤토리 내 해당 슬롯의 버튼입니다.
    public Image icon;                            // 인벤토리 내 해당 슬롯에 맞는 아이콘입니다.
    public GameObject equipMark;                  // 인벤토리 내 해당 아이템 장착 여부입니다.
    public GameObject fuseMark;                   // 인벤토리 내 해당 아이템 합성 슬롯 등록 여부입니다.
    public GameObject lockedMark;                 // 인벤토리 내 아이템 잠금 여부입니다.
    public GameObject selectMark;                 // 인벤토리 내 해당 아이템 선택 여부입니다.
    public TextMeshProUGUI UpgradeValue;          // 인벤토리 내 해당 아이템의 강화 단계 표기용 텍스트입니다.
}
