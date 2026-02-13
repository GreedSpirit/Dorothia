using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;                 // 싱글톤 패턴

    [SerializeField] GameObject _infoPanel;                  // 정보를 담을 패널
    [SerializeField] Image _infoIcon;                        // 정보 패널에서의 장비 아이콘 출력용 이미지
    [SerializeField] TextMeshProUGUI _infoName;              // 정보 패널에서 장비의 이름를 나타낼 텍스트
    [SerializeField] TextMeshProUGUI _infoDescription;       // 정보 패널에서 장비의 정보를 나타낼 텍스트
    [SerializeField] Button _confirmButton;                  // 합성 전용 확인버튼
    [SerializeField] Button _cancelButton;                   // 합성 전용 취소버튼
    [SerializeField] EquipmentUI equipmentUI;                // 인벤토리 열기 기능을 사용하기 위한 EquipmentUI

    private SlotType _slotType;                              // 클릭한 슬롯의 타입을 저장하기 위한 슬롯타입. ( 장착 칸 / 합성 칸 )
    private EquipSlot _targetSlot;                           // 클릭한 슬롯
    private Equipment _selectedEquipment;                    // 인벤토리 칸에서 선택한 장비
    
    private void Awake()
    {
        //인스턴스가 이미 존재하며 자신이 아닌 경우 삭제합니다.
        if(Instance != null &&  Instance != this)
            Destroy(gameObject);

        //통과했다면 자기 자신을 인스턴스에 두고 파괴를 방지합니다.
        Instance = this;
        DontDestroyOnLoad(Instance);
    }

    /// <summary>
    /// 대상 슬롯을 클릭한 슬롯에 넣습니다.
    /// </summary>
    /// <param name="slot">동작을 통해 장비를 집어넣을 슬롯</param>
    public void SetTargetSlot(EquipSlot slot)
    {
        _targetSlot = slot;
    }

    /// <summary>
    /// 장비를 클릭했을 때의 동작입니다.
    /// </summary>
    /// <param name="equip">인벤토리 내에서 선택된 장비</param>
    public void OnClickItem(Equipment equip)
    {
        //인자값으로 받은 장비를 EquipmentManager가 넘겨줄 대상 장비로 설정합니다.
        _selectedEquipment = equip;
        
        //정보를 보여줄 패널 활성화, 장비의 아이콘을 가져오고 이름을 변경하며, 그 이름의 색을 레어도와 일치시킵니다.
        _infoPanel.SetActive(true);
        _infoIcon.sprite = equip.icon;
        _infoName.text = $"이름: {equip.equip_name}";
        _infoName.color = RarityColor.GetColor((Rarity)equip.equipment_Rarity);
        _infoDescription.text = equip.GetEquipStatusString();

        //확인 버튼에 있던 기능을 지우고, 합성 슬롯에 집어넣기 기능을 추가합니다.
        //현재는 장착 기능을 구현하지 않았으므로 예외 처리 없이 바로 넣습니다.
        _confirmButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.AddListener(() =>
        {
            AddToFuseSlot(_selectedEquipment);
        });
        //취소 버튼에 있던 기능을 지우고, 합성 슬롯에서 빼기 기능을 추가합니다.
        //확인 버튼과 동일하게 예외 처리 없이 바로 넣습니다.
        _cancelButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.AddListener(() =>
        {
            RemoveFromFuseSlot();
        });
    }

    /// <summary>
    /// 합성 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="equip">합성 재료로 쓰기 위한 장비</param>
    private void AddToFuseSlot(Equipment equip)
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
        }
    }

    /// <summary>
    /// 합성 슬롯으로부터 장비를 제거합니다.
    /// </summary>
    private void RemoveFromFuseSlot()
    {
        //타겟 슬롯이 null이 아니라면
        if(_targetSlot != null)
        {
            //슬롯에 장착된 장비를 null로 바꾸고
            _targetSlot.equipped = null;
            //스프라이트를 제거합니다.
            _targetSlot.iconImage.sprite = null;
        }
    }
}
