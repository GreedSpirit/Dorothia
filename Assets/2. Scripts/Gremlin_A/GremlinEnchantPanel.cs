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
    [SerializeField] TextMeshProUGUI _currentGold;
    [SerializeField] TextMeshProUGUI _costGold;

    [SerializeField] Button _enchantButton;

    float successRate;


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

        Gremlin_UpgradeData upgradeData = DataManager.Instance.GetData<Gremlin_UpgradeData>((int)gremlin._rarity);
        float costGold = upgradeData.Gremlin_Upgrade_Cost * (1 + upgradeData.Up_Cost_Value);
        _costGold.text = $"{costGold}G";
        _currentGold.text = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold) > (BigInteger)costGold?$"{ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)}G":
            $"<color=red>{ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold)}G</color>";

        successRate = upgradeData.Gremlin_Upgrade_Prob / 100;
    }

    protected override void OnClose()
    {
        
    }

    protected override void OnOpen()
    {

    }

}
