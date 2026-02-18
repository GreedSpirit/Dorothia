using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipEnchant : MonoBehaviour
{
    [Header("인벤토리 창의 장비강화 버튼")]
    [SerializeField] Button _enchantButton;                           // 장비 강화로 진입하기 위한 버튼. 장비창에서 인벤토리 오픈 시에 있는 버튼을 연결해 주십시오.

    [Header("강화 창 캔버스그룹(패널)")]
    [SerializeField] CanvasGroup _enchantPanel;                       // 장비 강화와 관련된 패널 담당용 캔버스 그룹입니다. 해당 패널에 캔버스그룹을 추가해 넣어주십시오.

    [Header("강화 창 내의 강화 버튼")]
    [SerializeField] Button _proceedEnchantButton;                    // 실제 장비 강화를 진행하기 위한 버튼. 강화 창에서의 장비 강화 진행용 버튼을 연결해 주십시오.

    [Header("강화 대상 장비 이미지 표현용")]
    [SerializeField] Image _beforeEnchantEquipment;                   // 장비 강화가 성공하기 전, 인벤토리 내의 해당 장비 이미지입니다.
    [SerializeField] Image _afterEnchantEquipment;                    // 장비 강화가 성공하고 난 후, 해당 장비를 보여주기 위한 이미지입니다.

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
        _enchantButton.onClick.AddListener(() =>
        {
            GetEquipment(EquipmentManager.Instance.GiveEquipmentData());
            SetPanelActiveValue(true);
            RefreshEnchantPanel(_equipment);
        });
        _proceedEnchantButton.onClick.AddListener(() =>
        {
            Enchant(_equipment);
        });
    }

    public void GetEquipment(Equipment equip)
    {
        _equipment = equip;
    }

    /// <summary>
    /// 패널의 활성화 여부를 정합니다.
    /// </summary>
    /// <param name="value">활성화 여부</param>
    public void SetPanelActiveValue(bool value)
    {
        //참이면 1, 거짓이면 0으로 하여 참일 경우에만 보이게 합니다.
        _enchantPanel.alpha = value == true ? 1 : 0;

        //상호작용 여부와 뒤 오브젝트와의 상호작용 제한은 참일 경우에만 활성화되도록 합니다.
        _enchantPanel.interactable = value;
        _enchantPanel.blocksRaycasts = value;
    }

    public void RefreshEnchantPanel(Equipment equip)
    {
        //강화를 진행하기 전과 성공 후의 장비를 나타내기 위해, 우선 해당 이미지를 현재 장비와 동일하게 맞춥니다.
        //테스트를 위해 아이콘을 넣어 생성하도록 하였으니, 해당 조건을 기반으로 스프라이트 참조 조건을 지정하겠습니다.
        //(icon이 존재하지 않을 경우에는, equip_icon의 경로를 기반으로 스프라이트를 생성, 존재하는 경우에는 해당 icon을 그대로 사용)
        _beforeEnchantEquipment.sprite = equip.icon == null ? Resources.Load<Sprite>(equip.equip_icon) : equip.icon;
        _afterEnchantEquipment.sprite = equip.icon == null ? Resources.Load<Sprite>(equip.equip_icon) : equip.icon;

        //강화가 되기 전과 성공했을 때의 강화 수치를 기록하는 텍스트를 변경해줍니다.
        _beforeEnchantUpgradeValueText.text = $"{equip.equip_Upgrade}";
        _afterEnchantUpgradeValueText.text = $"{equip.equip_Upgrade + 1}";

        //장비의 골드 소모량은 전용 식이 존재합니다. 해당 식을 계산하기 위해 조건문을 작성하겠습니다.
        //아직 테이블 내에서 특정 조건을 맞춰 가져오는 방식을 알 수 없으므로, 해당 계산은 값을 따로 빼오는 방식으로 진행하겠습니다. (하드코딩)
        if(equip.equip_Upgrade <= 10)
        {
            _costGold = equip.equip_price * equip.equip_Upgrade * GetIntByRarity(equip.equipment_Rarity);
        }
        else if(equip.equip_Upgrade > 10 && equip.equip_Upgrade <= 30)
        {
            _costGold = (int)(equip.equip_price * Mathf.Pow(equip.equip_Upgrade, 1.2f) * GetIntByRarity(equip.equipment_Rarity));
        }
        else if(equip.equip_Upgrade > 30 && equip.equip_Upgrade <= 50)
        {
            _costGold = (int)(equip.equip_price * Mathf.Pow(equip.equip_Upgrade, 1.5f) * GetIntByRarity(equip.equipment_Rarity));
        }

        _costGoldText.text = $"{_costGold}<color=orange>G</color>";
        _currentGoldText.text = $"{TestGoldAndScrapManager.Instance.testGold}<color=orange>G</color>";
    }

    public void Enchant(Equipment equip)
    {
        if(equip.equip_Upgrade >= 50)
        {
            Debug.Log("이미 최대 강화 수치를 달성한 장비입니다!");
            return;
        }

        if (TestGoldAndScrapManager.Instance.testGold < (int)_costGold)
        {
            Debug.Log("소지 골드가 부족합니다.");
            return;
        }

        TestGoldAndScrapManager.Instance.testGold -= (int)_costGold;
        Debug.Log($"골드를 {_costGold}만큼 소모하여 강화를 시도합니다. 현재 남은 골드는 {TestGoldAndScrapManager.Instance.testGold}입니다.");
        //우선 해당 장비에 대한 정보를 먼저 받아옵니다.
        //이때, 테이블 내에서 정의한 것이 "해당 강화도로 만들기 위한 장비 강화 과정"에 사용될 정보라는 것을 기반으로 작성합니다.
        //ex) 강화도 10은 강화도 9에서 강화도 10으로 올라가기 위해 사용하는 정보.
        var upgradeData = DataManager.Instance.GetData<Equip_UpgradeData>(equip.equip_Upgrade+1);

        //성공률을 테이블로부터 받아옵니다.
        int successChance = (int)(upgradeData.Equip_Success_Prob * 100);

        //보정값을 사용하기로 했다면, 해당 수치만큼 더해줍니다.
        if(_isUsingFailureCount == true)
        {
            //보정값을 전부 사용하였을 때 100을 초과하지 않는다면 그냥 그 값을 그대로 더합니다.
            if(successChance + equip.equip_Upgrade_Count <= 100)
            {
                successChance += equip.equip_Upgrade_Count;
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
        if(value <= successChance)
        {
            Debug.Log("강화에 성공하였습니다!");
            
            //강화 수치를 높입니다.
            equip.equip_Upgrade++;

            //강화 성공 시 강화 구간이 변경되는 경우에만, 강화 보정치를 초기화합니다.
            if(equip.equip_Upgrade % 10 == 1)
            {
                equip.equip_Upgrade_Count = 0;
            }
        }

        //그 이상의 수가 나왔다면 실패입니다.
        else
        {
            //보정값을 사용한 것이라면, 그만큼 빼 줍니다.
            Debug.Log("강화에 실패하였습니다. 보정값을 1 획득합니다.");

            //강화 횟수를 올립니다. 강화 실패 시 얻는 보정값으로 취급합니다.
            equip.equip_Upgrade_Count++;
        }

        RefreshEnchantPanel(equip);
    }

    /// <summary>
    /// Rarity ID값읊 기반으로 배율을 받아옵니다. 
    /// 데이터 테이블을 통해 이미 장비 정보를 받아왔다면 Rarity 열거형 버전을 사용해주십시오.
    /// </summary>
    /// <param name="Rarity">해당 장비의 레어도 ID값을 갖도록 하는, 장비의 equipment_Rarity 부분.</param>
    /// <returns>해당 등급 ID값을 기반으로 확인한 등급배율</returns>
    public float GetIntByRarity(int Rarity)
    {
        switch(Rarity)
        {
            case 40001:
                return 1;

            case 40002:
                return 1.5f;

            case 40003:
                return 3;

            case 40004:
                return 6;

            case 40005:
                return 10;

            default:
                return 1;
        }
    }

    /// <summary>
    /// Rarity 열겨형 기반으로 배율을 받아옵니다.
    /// </summary>
    /// <param name="Rarity">해당 장비의 레어도를 나타내는 Rarity 열거형 값</param>
    /// <returns>해당 배율과 일치하는 강화 배율값</returns>
    public float GetIntByRarity(Rarity Rarity)
    {
        switch(Rarity)
        {
            case Rarity.Normal:
                return 1;

            case Rarity.Uncommon:
                return 1.5f;

            case Rarity.Rare:
                return 3;

            case Rarity.Legendary:
                return 6;

            case Rarity.Mythtic:
                return 10;

            default:
                return 1;
        }
    }
}
