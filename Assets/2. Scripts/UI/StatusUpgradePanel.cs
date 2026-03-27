using UnityEngine;
using TMPro;
using System.Numerics;
using UnityEngine.UI;

public class StatusUpgradePanel : MonoBehaviour
{
    #region Inner Class

    [System.Serializable]
    private class StatUpgradeUI
    {
        public Status type;
        public TextMeshProUGUI statText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;

        [HideInInspector] public Button upgradeButton;
    }

    #endregion

    #region Fields

    [Header("스탯 강화 UI 목록")]
    [SerializeField]
    private StatUpgradeUI[] _statUIs = new StatUpgradeUI[]
    {
        new StatUpgradeUI { type = Status.AttackSpeed    },
        new StatUpgradeUI { type = Status.CriticalChance },
        new StatUpgradeUI { type = Status.CriticalDamage },
        new StatUpgradeUI { type = Status.HPRegen        },
        new StatUpgradeUI { type = Status.MoveSpeed      },
    };

    [SerializeField] private TextMeshProUGUI haveGold;

    private const float UPGRADE_WEIGHT = 1.06f;

    private PlayerStats _playerStats;

    #endregion

    // ══════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Start()
    {
        // 버튼 캐싱 (costText 부모에서 Button 탐색)
        foreach (var ui in _statUIs)
        {
            if (ui.costText != null)
                ui.upgradeButton = ui.costText.GetComponentInParent<Button>();
        }

        _playerStats = FindAnyObjectByType<PlayerStats>();

        if (_playerStats != null)
            _playerStats.OnStatsChanged += RefreshAllUI;

        ExchangeManager.Instance.OnGoldChanged += OnGoldChanged;

        RefreshAllUI();
    }

    private void OnDestroy()
    {
        if (_playerStats != null)
            _playerStats.OnStatsChanged -= RefreshAllUI;

        if (ExchangeManager.Instance != null)
            ExchangeManager.Instance.OnGoldChanged -= OnGoldChanged;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Upgrade

    public void Click_Upgrade(int statusType)
    {
        Status type = (Status)statusType;
        FinalStat stat = StatManager.Instance.stats[type];

        if (!stat.CanUpgrade)
        {
            Debug.Log($"[StatusUpgradePanel] {type} 최대 강화 도달");
            return;
        }

        int cost = GetUpgradeCost(stat.upgradeLevel);
        BigInteger currentGold = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold);

        if (currentGold < cost)
        {
            Debug.Log("[StatusUpgradePanel] 골드 부족");
            return;
        }

        if (!stat.TryUpgrade()) return;

        ExchangeManager.Instance.UseMoney(MoneyType.Gold, cost);
        StatManager.Instance.RefreshStats();

        Debug.Log($"[StatusUpgradePanel] {type} 강화 완료 → Lv.{stat.upgradeLevel}");
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region UI Refresh

    void OnGoldChanged(BigInteger currentGold)
    {
        if (haveGold != null)
            haveGold.text = $"{currentGold} G";

        foreach (var ui in _statUIs)
            RefreshStatUI(ui, currentGold);
    }

    void RefreshAllUI()
    {
        BigInteger currentGold = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold);

        if (haveGold != null)
            haveGold.text = $"{currentGold} G";

        foreach (var ui in _statUIs)
            RefreshStatUI(ui, currentGold);
    }

    void RefreshStatUI(StatUpgradeUI ui, BigInteger currentGold)
    {
        FinalStat stat = StatManager.Instance.stats[ui.type];

        // 스탯 텍스트
        if (ui.statText != null)
            ui.statText.text = TrimStatValue(stat.FinalValue);

        // 레벨 텍스트
        if (ui.levelText != null)
            ui.levelText.text = $"Lv.{stat.upgradeLevel} / {stat.maxUpgradeLevel}";

        // 코스트 텍스트 + 버튼
        if (ui.costText == null) return;

        if (!stat.CanUpgrade)
        {
            ui.costText.text = "Max";
            ui.costText.color = Color.white;

            // 최대 강화 시 버튼 비활성화
            if (ui.upgradeButton != null)
                ui.upgradeButton.interactable = false;

            return;
        }

        // 강화 가능 상태 — 버튼 활성화
        if (ui.upgradeButton != null)
            ui.upgradeButton.interactable = true;

        int cost = GetUpgradeCost(stat.upgradeLevel);
        ui.costText.text = cost.ToString("N0");
        ui.costText.color = currentGold >= cost ? Color.white : Color.red;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Utility

    string TrimStatValue(double value)
    {
        return value.ToString("F4");
    }

    int GetUpgradeCost(int currentLevel)
    {
        var data = DataManager.Instance.GetData<Character_UpgradeData>(70001);
        return Mathf.RoundToInt(data.Character_Upgrade_Gold * Mathf.Pow(UPGRADE_WEIGHT, currentLevel));
    }

    #endregion
}