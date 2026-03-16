using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipEnchant : BaseUI
{
    [Header("인벤토리 창 관련")]
    [SerializeField] InventoryPanel _inventoryPanel;
    [SerializeField] Button _enchantButton;                           // 장비 강화로 진입하기 위한 버튼. 장비창에서 인벤토리 오픈 시에 있는 버튼을 연결해 주십시오.

    [Header("강화 창 내의 강화 버튼")]
    [SerializeField] Button _proceedEnchantButton;                    // 실제 장비 강화를 진행하기 위한 버튼. 강화 창에서의 장비 강화 진행용 버튼을 연결해 주십시오.

    [SerializeField] Toggle _useWeightToggle;                         // 가중치 사용 여부를 결정할 토글입니다.
    [SerializeField] TextMeshProUGUI _toggleText;                     // 토글 클릭 시 유저가 확인할 수 있도록 하는, 토글의 체크 표시를 대체할텍스트입니다.

    [Header("강화 대상 장비 이미지 표현용")]
    [SerializeField] Image _beforeEnchantEquipment;                   // 장비 강화가 성공하기 전, 인벤토리 내의 해당 장비 이미지입니다.
    [SerializeField] Image _afterEnchantEquipment;                    // 장비 강화가 성공하고 난 후, 해당 장비를 보여주기 위한 이미지입니다.
    [SerializeField] TextMeshProUGUI EquipNameTitle;                      // 장비의 이름을 출력할 텍스트입니다.

    [Header("강화 관련 툴팁 표현용 TMP")]
    [SerializeField] TextMeshProUGUI _beforeEnchantUpgradeValueText;  // 장비의 현재 강화 수치를 보여주기 위한 텍스트입니다.
    [SerializeField] TextMeshProUGUI _afterEnchantUpgradeValueText;   // 장비의 강화 성공 후 강화 수치를 보여주기 위한 텍스트입니다.
    [SerializeField] TextMeshProUGUI _currentGoldText;                // 현재 소지중인 골드량을 보여주기 위한 텍스트입니다. 숫자 표기 부분을 연결해 주세요.
    [SerializeField] TextMeshProUGUI _costGoldText;                   // 강화 시에 소모될 골드량을 보여주기 위한 텍스트입니다. 숫자 표기 부분을 연결해 주세요.

    private Equipment _equipment;                                     // 강화를 진행할 장비입니다. 장비를 선택한 채로 장비 강화 버튼을 누르면 해당 장비 정보를 담아두기 위함입니다.
    private bool _isUsingFailureCount = false;                        // 강화 실패로 쌓이게 된 보정값을 사용할지 여부를 결정합니다.
    private float _costGold;                                          // 강화 시에 사용하게 될 골드량입니다.


    private void Awake()
    {
        //강화 버튼에 다음 기능을 추가합니다.
        // - 강화할 장비 받아오기
        // - 강화 창 활성화
        // - 강화 창 갱신하기
        _enchantButton.onClick.AddListener(() =>
        {
            if (_inventoryPanel.CheckEquipmentSelected() == true)
            {
                GetEquipment(_inventoryPanel.GiveEquipmentData());
                RefreshEnchantPanel(_equipment);
            }
        });

        //강화 진행 버튼에 다음 기능을 추가합니다.
        // - 장비 강화 진행
        _proceedEnchantButton.onClick.AddListener(() =>
        {
            Enchant(_equipment);
        });
        _useWeightToggle.onValueChanged.AddListener(UseWeight);

        _inventoryPanel.onInventoryChanged += DisableInteractable;
        _inventoryPanel.onInventoryClosed += DisableInteractable;
        _inventoryPanel.onClickEquipment += EnableInteractable;
    }

    private void Start()
    {
        Close();
    }

    private void OnDisable()
    {
        _inventoryPanel.onInventoryChanged -= DisableInteractable;
        _inventoryPanel.onInventoryClosed -= DisableInteractable;
        _inventoryPanel.onClickEquipment -= EnableInteractable;
    }


    public void EnableInteractable()
    {
        _enchantButton.interactable = true;
    }

    public void DisableInteractable()
    {
        _enchantButton.interactable = false;
    }

    private void UseWeight(bool value)
    {
        _isUsingFailureCount = value;
        _toggleText.text = value == true ? "On" : "Off";
    }

    /// <summary>
    /// 강화 창에 강화할 장비를 등록합니다.
    /// </summary>
    /// <param name="equip">강화하고자 하는 장비</param>
    public void GetEquipment(Equipment equip)
    {
        _equipment = equip;
    }

    /// <summary>
    /// 장비의 정보가 갱신됨에 따라, 장비 강화 패널의 정보들 또한 갱신시킵니다.
    /// </summary>
    /// <param name="equip">강화하고자 하는 장비</param>
    public void RefreshEnchantPanel(Equipment equip)
    {
        if(equip.isEquipped == true)
        {
            EquipNameTitle.text = $"{equip.equip_name}[착용중] +{equip.equip_Upgrade}";
        }
        else
        {
            EquipNameTitle.text = $"{equip.equip_name} +{equip.equip_Upgrade}";
        }
        //강화를 진행하기 전과 성공 후의 장비를 나타내기 위해, 우선 해당 이미지를 현재 장비와 동일하게 맞춥니다.
        //테스트를 위해 아이콘을 넣어 생성하도록 하였으니, 해당 조건을 기반으로 스프라이트 참조 조건을 지정하겠습니다.
        //(icon이 존재하지 않을 경우에는, equip_icon의 경로를 기반으로 스프라이트를 생성, 존재하는 경우에는 해당 icon을 그대로 사용)
        _beforeEnchantEquipment.sprite = equip.icon == null ? Resources.Load<Sprite>(equip.equip_icon) : equip.icon;
        _afterEnchantEquipment.sprite = equip.icon == null ? Resources.Load<Sprite>(equip.equip_icon) : equip.icon;

        //강화가 되기 전과 성공했을 때의 강화 수치를 기록하는 텍스트를 변경해줍니다.
        _beforeEnchantUpgradeValueText.text = $"{equip.equip_Upgrade}";
        _afterEnchantUpgradeValueText.text = $"{equip.equip_Upgrade + 1}";

        //장비의 골드 소모량은 전용 식이 존재합니다. 해당 식을 계산하기 위해 조건문을 작성하겠습니다.
        _costGold = Mathf.RoundToInt(equip.equip_price * Mathf.Pow(equip.equip_Upgrade+1, DataManager.Instance.GetData<Equip_Upgrade_GoldData>(equip.equip_Upgrade+1).Equip_Upgrade_Value)
                * ItemCalculator.GetEnchantWeightByRarity(DataManager.Instance.GetData<Equip_RankData>(equip.equipment_Rarity).Equip_Rank));

        //소모될 골드의 텍스트는, 소모 골드량 값 뒤에 주황색 G를 붙여 표현합니다.
        _costGoldText.text = $"{_costGold}<color=orange>G</color>";

        //현재의 골드량은, (테스트를 위해 임시로 작성한 테스트용)소지 중인 골드 값 뒤에 주황색 G를 붙여 표현합니다.
        _currentGoldText.text = $"{TestGoldAndScrapManager.Instance.testGold}<color=orange>G</color>";
    }

    /// <summary>
    /// 장비의 강화를 진행합니다.
    /// </summary>
    /// <param name="equip">강화하고자 하는 장비</param>
    public void Enchant(Equipment equip)
    {
        //현재 장비의 강화 최대치는 50입니다. 50을 달성했으면 반환합니다.
        if(equip.equip_Upgrade >= 50)
        {
            Debug.Log("이미 최대 강화 수치를 달성한 장비입니다!");
            return;
        }

        //현재 소지 중인 골드가 요구하는 골드량보다 부족한 경우 안내하고 반환합니다.
        if (TestGoldAndScrapManager.Instance.testGold < (int)_costGold)
        {
            Debug.Log("소지 골드가 부족합니다.");
            return;
        }

        //우선 골드를 사용합니다.
        TestGoldAndScrapManager.Instance.testGold -= (int)_costGold;
        Debug.Log($"골드를 {_costGold}만큼 소모하여 강화를 시도합니다. 현재 남은 골드는 {TestGoldAndScrapManager.Instance.testGold}입니다.");

        //우선 해당 장비에 대한 정보를 먼저 받아옵니다.
        //이때, 테이블 내에서 정의한 것이 "해당 강화도로 만들기 위한 장비 강화 과정"에 사용될 정보라는 것을 기반으로 작성합니다.
        //ex) 강화도 10은 강화도 9에서 강화도 10으로 올라가기 위해 사용하는 정보.
        var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade+1);

        //성공률을 테이블로부터 받아옵니다.
        float successChance = upgradeData.Equip_Success_Prob * 100;

        //보정값을 사용하기로 했다면, 해당 수치만큼 더해줍니다.
        if(_isUsingFailureCount == true)
        {
            //보정값을 전부 사용하였을 때 100을 초과하지 않는다면 그냥 그 값을 그대로 더합니다.
            if(successChance + equip.equip_Upgrade_Weight <= 100)
            {
                successChance += equip.equip_Upgrade_Weight * 100;
            }
            //초과하는 경우라면, 100을 달성할 값까지만 사용합니다.
            else
            {
                successChance = 100;
            }
        }

        //성공 여부를 정하기 위해 100까지의 수에서 랜덤으로 확인하도록 합니다.
        int value = Random.Range(1, 101);

        //성공률 이하의 수가 나왔다면 성공입니다.
        if(value <= (int)successChance)
        {
            Debug.Log("강화에 성공하였습니다!");
            
            //강화 수치를 높입니다.
            equip.equip_Upgrade++;

            //강화 보정치를 사용해서 강화했다면 0으로 초기화시킵니다.
            if(_isUsingFailureCount == true)
            {
                //가중치 + 성공률이 1 이상일 경우, 가중치에서 소모량만큼 감소, 그 외에는 0으로 초기화합니다.
                equip.equip_Upgrade_Weight = equip.equip_Upgrade_Weight + upgradeData.Equip_Success_Prob >= 1?
                    equip.equip_Upgrade_Weight - (1 - upgradeData.Equip_Success_Prob): 0;
            }

            //강화 성공 시 강화 구간이 변경되는 경우에만, 강화 보정치를 초기화합니다.
            if(equip.equip_Upgrade > 1 && upgradeData.Equip_Upgrade_Section != DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade -1).Equip_Upgrade_Section)
            {
                Debug.Log("강화 보정치 초기화");
                equip.equip_Upgrade_Weight = 0;
            }
        }

        //그 이상의 수가 나왔다면 실패입니다.
        else
        {
            //보정값을 사용한 것이라면, 그만큼 빼 줍니다.
            if(_isUsingFailureCount == true)
            {
                equip.equip_Upgrade_Weight = 0;
            }

            //강화 보정값을 상승시킵니다. 현재 보정값이 비어있는 데이터가 존재하므로, 0인 경우 0.1f라는 임시 값을 넣어주겠습니다.
            equip.equip_Upgrade_Weight += DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade + 1).Equip_Upgrade_Failure != 0?
                DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade + 1).Equip_Upgrade_Failure:
                0.1f;

            Debug.Log($"강화에 실패하였습니다. 보정값을 획득합니다. 현재 보정값 : {equip.equip_Upgrade_Weight}");
        }

        RefreshEnchantPanel(equip);
    }

    protected override void OnOpen()
    {
        
    }

    protected override void OnClose()
    {
        
    }
}
