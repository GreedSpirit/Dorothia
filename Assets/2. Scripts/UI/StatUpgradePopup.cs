using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class StatUpgradePopup : BaseUI
{
    public enum Type { Upgrade, Promotion };
    private Type type = Type.Upgrade;

    [SerializeField] private TextMeshProUGUI title;

    [SerializeField] private ToggleGroup toggles;
    [SerializeField] private Toggle upgradeToggle;
    [SerializeField] private Toggle promotionToggle;

    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject promotionPanel;

    private void Start()
    {
        upgradeToggle.onValueChanged.AddListener(ToggleActiveCheck);
        promotionToggle.onValueChanged.AddListener(ToggleActiveCheck);
    }

    public void SetType(Type type)
    {
        this.type = type;
    }

    public void ToggleActiveCheck(bool isOn)
    {
        if (!isOn) return;

        Toggle activeToggle = toggles.ActiveToggles().FirstOrDefault();

        if (activeToggle != null)
        {
            bool isUpgrade = (activeToggle == upgradeToggle);

            upgradePanel.SetActive(isUpgrade);
            promotionPanel.SetActive(!isUpgrade);

            type = isUpgrade ? Type.Upgrade : Type.Promotion;

            title.text = isUpgrade ? "캐릭터 업그레이드" : "캐릭터 승급";
        }
    }

    protected override void OnOpen()
    {
        bool isUpgrade = (type == Type.Upgrade);

        Debug.Log($"{isUpgrade}");
        upgradeToggle.isOn = isUpgrade;
        promotionToggle.isOn = !isUpgrade;

        Debug.Log($"{upgradeToggle.isOn},{promotionToggle.isOn}");

        ToggleActiveCheck(true);
    }

    protected override void OnClose()
    {
    }
}