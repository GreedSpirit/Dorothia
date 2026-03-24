using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GremlinEnchantPanel : BaseUI
{
    [SerializeField] Gremlin _targetGremlin;

    [SerializeField] Image _gremlinImage;
    [SerializeField] TextMeshProUGUI _gremlinName;
    [SerializeField] TextMeshProUGUI _beforeEnchantCount;
    [SerializeField] TextMeshProUGUI _afterEnchantCount;
    [SerializeField] TextMeshProUGUI _enchantLevelBonus;
    [SerializeField] TextMeshProUGUI _currentGold;
    [SerializeField] TextMeshProUGUI _costGold;

    [SerializeField] Button _enchantButton;

    float successRate;
    float costGold;


    private void Awake()
    {
        Close();
    }

    public async void Init(Gremlin gremlin)
    {
        _targetGremlin = gremlin;

        var gSprite = Addressables.LoadAssetAsync<Sprite>($"{gremlin._gremlinData.PrefabName}_Icon");
        await gSprite.Task;

        _gremlinImage.sprite = gSprite.Result;

        GremlinData data = DataManager.Instance.GetData<GremlinData>(gremlin._gremlinData.PetID);
        _gremlinName.text = data.Gremlin_Name;

        _beforeEnchantCount.text = $"+{gremlin._currentLevel}";
        _afterEnchantCount.text = $"+{gremlin._currentLevel + 1}";

        if(_targetGremlin._gremlinData.Type == Gremlin_Type.지원형)
        {
            Gremlin_BufferData bufferData = DataManager.Instance.GetData<Gremlin_BufferData>((int)gremlin._rarity);
            _enchantLevelBonus.text = 
                $"버프 보너스 : {bufferData.Gremlin_Level_Bonus * gremlin._currentLevel * 100}% -> {bufferData.Gremlin_Level_Bonus * (gremlin._currentLevel + 1) * 100}%";
        }

        Gremlin_UpgradeData upgradeData = DataManager.Instance.GetData<Gremlin_UpgradeData>((int)gremlin._rarity);
        costGold = upgradeData.Gremlin_Upgrade_Cost * (1 + upgradeData.Up_Cost_Value * _targetGremlin._currentLevel);
        _costGold.text = $"{costGold}G";
        _currentGold.text = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold) > (BigInteger)costGold?$"{ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)}G":
            $"<color=red>{ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)}G</color>";

        successRate = (upgradeData.Gremlin_Upgrade_Prob / 100) + (upgradeData.Up_Prob_Value * gremlin._currentLevel) < 0.05f? 0.05f:
            (upgradeData.Gremlin_Upgrade_Prob / 100) + (upgradeData.Up_Prob_Value * gremlin._currentLevel);
    }

    public void Enchant()
    {
        if (_targetGremlin == null) return;

        if (_targetGremlin._currentLevel >= 50) return;

        if ((BigInteger)costGold > ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)) return;

        ExchangeManager.Instance.UseMoney(MoneyType.Gold, (BigInteger)costGold);

        Gremlin_UpgradeData upgradeData = DataManager.Instance.GetData<Gremlin_UpgradeData>((int)_targetGremlin._rarity);
        float currentSuccessRate = (successRate + (upgradeData.Up_Prob_Bonus * _targetGremlin._enchantCount)) * 100;

        int rng = Random.Range(1, 101);
        Debug.Log($"{rng} / {currentSuccessRate}");
        if(currentSuccessRate >= (int)rng)
        {
            _targetGremlin._currentLevel++;
            _targetGremlin._enchantCount = 0;
        }
        else
        {
            _targetGremlin._enchantCount++;
        }

        Init(_targetGremlin);
    }

    protected override void OnClose()
    {
        
    }

    protected override void OnOpen()
    {

    }

}
