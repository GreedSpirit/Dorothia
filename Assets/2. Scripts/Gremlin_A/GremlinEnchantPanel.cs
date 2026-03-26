using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GremlinEnchantPanel : BaseUI
{
    [SerializeField] Gremlin _targetGremlin;                   // 강화하고자 하는 대상 그렘린

    [SerializeField] Image _gremlinImage;                      // 해당 그렘린 아이콘 표기용 이미지
    [SerializeField] TextMeshProUGUI _gremlinName;             // 해당 그렘린 이름 표기용 TMP
    [SerializeField] TextMeshProUGUI _beforeEnchantCount;      // 강화하기 전 강화수치 ( 현재 강화수치 )
    [SerializeField] TextMeshProUGUI _afterEnchantCount;       // 강화 성공 후 강화 수치
    [SerializeField] TextMeshProUGUI _enchantLevelBonus;       // 강화 시의 변경점 표기용 TMP
    [SerializeField] TextMeshProUGUI _currentGold;             // 소지 골드 표기용 TMP
    [SerializeField] TextMeshProUGUI _costGold;                // 골드 소모량 표기용 TMP

    [SerializeField] Button _enchantButton;                    // 강화 시도용 버튼

    float successRate;      //성공 확률
    float costGold;         // 소모 골드량


    private void Awake()
    {
        //패널이 열리고 나면 즉시 닫기
        Close();
    }

    public async void Init(Gremlin gremlin)
    {
        //받아온 그렘린을 대상으로 지정
        _targetGremlin = gremlin;

        //해당 그렘린의 스프라이트를 어드레서블 통해 받아오기
        var gSprite = Addressables.LoadAssetAsync<Sprite>($"{gremlin._gremlinData.PrefabName}_Icon");
        await gSprite.Task;

        //받아온 이미지 적용
        _gremlinImage.sprite = gSprite.Result;

        //그렘린 데이터를 받아와 필요한 정보 작성
        GremlinData data = DataManager.Instance.GetData<GremlinData>(gremlin._gremlinData.PetID);
        _gremlinName.text = data.Gremlin_Name;

        _beforeEnchantCount.text = $"+{gremlin._currentLevel}";
        _afterEnchantCount.text = $"+{gremlin._currentLevel + 1}";

        //받아온 그렘린이 지원형인 경우
        if(_targetGremlin._gremlinData.Type == Gremlin_Type.지원형)
        {
            Gremlin_BufferData bufferData = DataManager.Instance.GetData<Gremlin_BufferData>((int)gremlin._rarity);
            _enchantLevelBonus.text = 
                $"버프 보너스 : {(bufferData.Gremlin_Level_Bonus * gremlin._currentLevel * 100).ToString("F1")}% -> {(bufferData.Gremlin_Level_Bonus * (gremlin._currentLevel + 1) * 100).ToString("F1")}%";
        }
        //받아온 그렘린이 공격형인 경우
        if(_targetGremlin._gremlinData.Type == Gremlin_Type.공격형)
        {
            Gremlin_AtkerData atkerData = DataManager.Instance.GetData<Gremlin_AtkerData>((int)gremlin._rarity);
            _enchantLevelBonus.text = 
                $"공격력 보너스 : {(atkerData.Gremlin_Level_Bonus * gremlin._currentLevel).ToString("F1")} -> {(atkerData.Gremlin_Level_Bonus * (gremlin._currentLevel + 1)).ToString("F1")}";
        }

        //강화 데이터 받아와 필요한 정보 작성
        Gremlin_UpgradeData upgradeData = DataManager.Instance.GetData<Gremlin_UpgradeData>((int)gremlin._rarity);

        //소모 골드 먼저 확인
        costGold = upgradeData.Gremlin_Upgrade_Cost * (1 + upgradeData.Up_Cost_Value * _targetGremlin._currentLevel);
        _costGold.text = $"{costGold}G";
        _currentGold.text = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold) > (BigInteger)costGold?$"{ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)}G":
            $"<color=red>{ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)}G</color>";

        //성공률 최소 수치 5% 보정
        successRate = (upgradeData.Gremlin_Upgrade_Prob / 100) + (upgradeData.Up_Prob_Value * gremlin._currentLevel) < 0.05f? 0.05f:
            (upgradeData.Gremlin_Upgrade_Prob / 100) + (upgradeData.Up_Prob_Value * gremlin._currentLevel);
    }

    /// <summary>
    /// 강화 진행
    /// </summary>
    public void Enchant()
    {
        //대상 그렘린이 존재하지 않을 경우 반환
        if (_targetGremlin == null) return;

        //대상 그렘린의 강화 단계가 50이거나 그 이상일 경우 반환
        if (_targetGremlin._currentLevel >= 50) return;

        //소모 골드가 현재 보유 골드량보다 클 경우 반환
        if ((BigInteger)costGold > ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)) return;

        //재화를 우선 사용
        ExchangeManager.Instance.UseMoney(MoneyType.Gold, (BigInteger)costGold);

        //강화 데이터로부터 필요한 정보 작성
        Gremlin_UpgradeData upgradeData = DataManager.Instance.GetData<Gremlin_UpgradeData>((int)_targetGremlin._rarity);
        //강화 수치는 성공률 + 실패횟수에 따른 보정. %로 확인할 것이므로 100을 곱해 정수로 만들기.
        float currentSuccessRate = (successRate + (upgradeData.Up_Prob_Bonus * _targetGremlin._enchantCount)) * 100;

        //1부터 100까지 랜덤 숫자 확인
        int rng = Random.Range(1, 101);

        //랜덤으로 뽑은 숫자가 성공률보다 낮게 나왔을 경우 성공
        if(currentSuccessRate >= (int)rng)
        {
            //단계 상승, 보정값 초기화
            _targetGremlin._currentLevel++;
            _targetGremlin._enchantCount = 0;
        }
        //높게 나왔을 경우
        else
        {
            //강화 시도 횟수 증가. 다음 강화에 보정값 적용.
            _targetGremlin._enchantCount++;
        }

        //강화창 초기화
        Init(_targetGremlin);
    }

    protected override void OnClose()
    {
        
    }

    protected override void OnOpen()
    {

    }

}
