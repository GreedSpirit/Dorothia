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
    [SerializeField] Button _equipButton;                    // 장착을 위해 누를 버튼.
    [SerializeField] EquipmentUI equipmentUI;                // 인벤토리 열기 기능을 사용하기 위한 EquipmentUI
    [SerializeField] CanvasGroup _noticePanel;               // 장비 분해를 시도할 때 나타나도록 할 안내용 창입니다.
    [SerializeField] Button _salvageButton;                  // 장비 분해 시도를 위한 인벤토리 내 버튼입니다.
    [SerializeField] Button _salvageAcceptButton;            // 장비 분해 결정의 경우를 위한 안내창 내 Y 버튼입니다.
    [SerializeField] Button _salvageRejectButton;            // 장비 분해 취소의 경우를 위한 안내창 내 Y 버튼입니다.
    [SerializeField] EquipmentInventory inventory;
    [SerializeField] InventoryPanel invPanel;
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

        _equipButton.onClick.AddListener(() =>
        {
            EquipmentManager.Instance.AddToSlot(EquipmentManager.Instance._selectedEquipment);
        });
        _salvageButton.onClick.AddListener(() =>
        {
            _noticePanel.alpha = 1;
            _noticePanel.interactable = true;
            _noticePanel.blocksRaycasts = true;
        });
        _salvageAcceptButton.onClick.AddListener(() =>
        {
            Salvage(_selectedEquipment);
            _noticePanel.alpha = 0;
            _noticePanel.interactable = false;
            _noticePanel.blocksRaycasts = false;
        });
        _salvageRejectButton.onClick.AddListener(() =>
        {
            _noticePanel.alpha = 0;
            _noticePanel.interactable = false;
            _noticePanel.blocksRaycasts = false;
        });
    }

    /// <summary>
    /// 대상 슬롯을 클릭한 슬롯에 넣습니다.
    /// </summary>
    /// <param name="slot">동작을 통해 장비를 집어넣을 슬롯</param>
    public void SetTargetSlot(EquipSlot slot)
    {
        _targetSlot = slot;
    }

    public Equipment GiveEquipmentData()
    {
        return _selectedEquipment;
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
        _infoName.text = equip.equip_Upgrade == 0? $"이름: {equip.equip_name}": $"이름: {equip.equip_name}<color=orange> +{equip.equip_Upgrade}</color>";
        _infoName.color = RarityColor.GetColor((Rarity)equipRankData.Equip_Rank);
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
            RemoveFromFuseSlot();
        });
    }

    /// <summary>
    /// 합성 슬롯에 해당 장비를 장착합니다.
    /// </summary>
    /// <param name="equip">합성 재료로 쓰기 위한 장비</param>
    private void AddToSlot(Equipment equip)
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

            if(_slotType == SlotType.EquipSlot)
            {
                Debug.Log($"{_targetSlot.equipped.equip_name} 장착 완료!");
            }
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

    /// <summary>
    /// 장비를 분해할 경우의 골드와 스크랩 정산을 위한 코드입니다.
    /// </summary>
    /// <param name="equip">분해를 진행할 장비</param>
    private void Salvage(Equipment equip)
    {
        inventory.RemoveEquipment(equip);
        //(현재 테이블이 업데이트되지 않아 업데이트 이전 테이블에 맞추기 위한 값 40000 제거) 등급에 따른 분해 데이터를 받아옵니다.
        var breakData = DataManager.Instance.GetData<Equip_BreakData>(equip.equipment_Rarity - 40000);

        //강화 수치가 0이 아닐 경우, 테이블로부터 강화 수치 기준 데이터를 받아와 골드에 공식을 적용합니다.
        if(equip.equip_Upgrade > 0)
        {
            var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade);
            TestGoldAndScrapManager.Instance.testGold += (equip.equip_price + (int)(equip.equip_Upgrade * breakData.Equip_Break_Gold / upgradeData.Equip_Success_Prob));
            TestGoldAndScrapManager.Instance.testScrap += ((equip.equip_level + breakData.Equip_Break_Gold_Scrap) / 10);
        }
        //강화 수치가 0인 경우, 골드는 기본값으로 적용하고 스크랩만 계산하여 지급합니다.
        if(equip.equip_Upgrade == 0)
        {
            TestGoldAndScrapManager.Instance.testGold += equip.equip_price;
            TestGoldAndScrapManager.Instance.testScrap += ((equip.equip_level + breakData.Equip_Break_Gold_Scrap) / 10);
        }
        
        invPanel.Refresh();
    }
}
