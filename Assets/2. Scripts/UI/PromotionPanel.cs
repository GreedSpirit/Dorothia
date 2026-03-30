using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PromotionPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI haveGold;
    [SerializeField] private TextMeshProUGUI needsGold;
    [Header("승급 전")]
    [SerializeField] private TextMeshProUGUI beforeRank;
    [SerializeField] private Image beforeStand;
    [Header("승급 후")]
    [SerializeField] private TextMeshProUGUI afterRank;
    [SerializeField] private Image afterStand;
    [SerializeField] private TextMeshProUGUI afterStat;
    [SerializeField] private Button promotionButton;

    // 현재 로드된 어드레서블 키 추적
    private string _beforeStandKey;
    private string _afterStandKey;

    // ──────────────────────────────────────
    #region Unity Lifecycle

    private void OnEnable()
    {
        PlayerStats.Instance.OnPromotionChanged += RefreshUI;
        ExchangeManager.Instance.OnGoldChanged += RefreshGold;

        RefreshUI(PlayerStats.Instance.CurrentPromotion);
    }

    private void OnDisable()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnPromotionChanged -= RefreshUI;

        if (ExchangeManager.Instance != null)
            ExchangeManager.Instance.OnGoldChanged -= RefreshGold;

        ReleaseAllStands();
    }

    #endregion

    // ──────────────────────────────────────
    #region UI Refresh

    private void RefreshUI(int currentPromotion)
    {
        Character_RankData beforeData = DataManager.Instance.GetData<Character_RankData>(currentPromotion);
        UpdateStandUI(beforeRank, beforeStand, beforeData, ref _beforeStandKey);

        bool isMaxPromotion = currentPromotion >= 8;

        if (isMaxPromotion)
        {
            afterRank.text = "최대 승급";
            afterStat.text = "";
            needsGold.text = "/ -";
            promotionButton.interactable = false;

            // 최대 승급이면 after 슬롯 비우고 릴리즈
            ReleaseStand(afterStand, ref _afterStandKey);
        }
        else
        {
            Character_RankData afterData = DataManager.Instance.GetData<Character_RankData>(currentPromotion + 1);
            UpdateStandUI(afterRank, afterStand, afterData, ref _afterStandKey);

            needsGold.text = "/ " + afterData.Character_Rank_Gold.ToString("N0");

            afterStat.text = afterData.Character_Information;

            bool canPromote = PlayerStats.Instance.CanPromote(out _);
            promotionButton.interactable = canPromote;
        }

        haveGold.text = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold).ToString("N0");
    }

    /// <summary>랭크 텍스트 + 스탠드 이미지 갱신. 이전 키 릴리즈 후 새 키 로드.</summary>
    private void UpdateStandUI(TextMeshProUGUI rankText, Image standImage, Character_RankData data, ref string trackedKey)
    {
        rankText.text = data.Character_Name;

        string newKey = data.Character_Stand;

        // 같은 키면 재로드 불필요
        if (trackedKey == newKey) return;

        // 이전 스프라이트 릴리즈
        if (!string.IsNullOrEmpty(trackedKey))
            AddressableManager.Instance.ReleaseAsset(trackedKey);

        trackedKey = newKey;

        AddressableManager.Instance.LoadAsset<Sprite>(newKey, (sprite) =>
        {
            // 로드 완료 전에 패널이 꺼졌을 경우 방어
            if (standImage != null)
                standImage.sprite = sprite;
        });
    }

    /// <summary>이미지 하나를 비우고 해당 키 릴리즈</summary>
    private void ReleaseStand(Image standImage, ref string trackedKey)
    {
        if (string.IsNullOrEmpty(trackedKey)) return;

        AddressableManager.Instance.ReleaseAsset(trackedKey);
        trackedKey = null;
        standImage.sprite = null;
    }

    /// <summary>패널 종료 시 로드된 스탠드 전부 릴리즈</summary>
    private void ReleaseAllStands()
    {
        ReleaseStand(beforeStand, ref _beforeStandKey);
        ReleaseStand(afterStand, ref _afterStandKey);
    }

    /// <summary>골드 변경 시 보유골드 텍스트 + 버튼 활성화만 갱신</summary>
    private void RefreshGold(BigInteger currentGold)
    {
        haveGold.text = currentGold.ToString("N0");

        bool canPromote = PlayerStats.Instance.CurrentPromotion < 8
                          && PlayerStats.Instance.CanPromote(out _);
        promotionButton.interactable = canPromote;
    }


    #endregion

    // ──────────────────────────────────────
    #region Button

    public void Click_Promotion()
    {
        if (!PlayerStats.Instance.CanPromote(out Character_RankData nextData)) return;

        ExchangeManager.Instance.UseMoney(MoneyType.Gold, nextData.Character_Rank_Gold);
        PlayerStats.Instance.Promote(); // → OnPromotionChanged → RefreshUI 자동 호출
    }

    #endregion
}