using GameUtility;
using System;
using System.Numerics;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IResettable
{
    public static PlayerStats Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }

    // ══════════════════════════════════════════════════════
    #region Fields & Properties

    readonly int _playerstats_id = 70001;

    PlayerCtrl _player;

    public int CurrentLevel { get; private set; }
    public int CurrentPromotion { get; private set; } = 1;
    public float CurrentHp { get; private set; }
    private float MaxHp => (float)StatManager.Instance.GetStat(Status.HP);
    public BigInteger CurrentExp { get; private set; }
    public BigInteger LevelExpN => StatManager.Instance.LevelExpN;
    public string TotalPower { get; private set; } = "0";

    #endregion

    // ══════════════════════════════════════════════════════
    #region Events

    /// <summary>UI HP바 갱신용 (현재HP, 최대HP)</summary>
    public event Action<float, float> OnHpChanged;

    /// <summary>UI EXP바 갱신용 (현재EXP, 필요EXP)</summary>
    public event Action<BigInteger, BigInteger> OnExpChanged;

    /// <summary>레벨업 UI 갱신용 (현재레벨)</summary>
    public event Action<int> OnLevelChanged;

    /// <summary>스탯창 전체 갱신용 (장비·강화·레벨업 시 발행)</summary>
    public event Action OnStatsChanged;

    /// <summary>승급 갱신용 </summary>
    public event Action<int> OnPromotionChanged;

    /// <summary>사망 처리용</summary>
    public event Action OnDead;

    /// <summary>종합 전투력 </summary>
    public event Action<string> OnTotalPowerChanged;

    #endregion

    // ══════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Start()
    {
        _player = GetComponent<PlayerCtrl>();

        var data = DataManager.Instance.GetData<Character_StatsData>(_playerstats_id);
        StatManager.Instance.InitStats(data);

        // TODO : 세이브 데이터 불러오기 후 CurrentLevel, CurrentPromotion 세팅
        CurrentLevel = data.Character_Level;
        CurrentPromotion = 1;

        RefreshStats();
        CurrentHp = MaxHp;

        EquipmentSlotManager.Instance.OnEquipChanged += OnEquipChanged;
        StatManager.Instance.OnStatsRefreshed += OnStatsRefreshed;

        OnStatsRefreshed();

    }

    private void OnDisable()
    {
        if (EquipmentSlotManager.Instance != null)
            EquipmentSlotManager.Instance.OnEquipChanged -= OnEquipChanged;

        if (StatManager.Instance != null)
            StatManager.Instance.OnStatsRefreshed -= OnStatsRefreshed;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Stat Refresh
    void OnStatsRefreshed()
    {
        RefreshTotalPower();
        OnStatsChanged?.Invoke(); // UI에 전파
    }

    void  RefreshStats() => StatManager.Instance.RefreshStats(CurrentLevel, CurrentPromotion);

    void OnEquipChanged() => StatManager.Instance.RefreshStats();

    #endregion

    // ══════════════════════════════════════════════════════
    #region EXP / Level

    public void AddExp(BigInteger amount)
    {
        CurrentExp += amount;

        while (CurrentExp >= LevelExpN && CurrentLevel < 200)
            LevelUp();

        OnExpChanged?.Invoke(CurrentExp, LevelExpN);
    }

    private const float LEVEL_WEIGHT = 1.10f;

    public void CheatLevelUp(int amount)
    {
        if (CurrentLevel + amount > 200) return;

        BigInteger prevRequired = LevelExpN;
        CurrentLevel += amount;
        CurrentExp -= prevRequired;

        StatManager.Instance.RefreshExp(CurrentLevel, LEVEL_WEIGHT);

        RefreshStats();
        CurrentHp = MaxHp;

        OnLevelChanged?.Invoke(CurrentLevel);
    }

    void LevelUp()
    {
        if (CurrentLevel >= 200) return;

        BigInteger prevRequired = LevelExpN;
        CurrentLevel++;
        CurrentExp -= prevRequired;

        StatManager.Instance.RefreshExp(CurrentLevel, LEVEL_WEIGHT);

        RefreshStats();
        CurrentHp = MaxHp;

        OnLevelChanged?.Invoke(CurrentLevel);
    }

    public void Promote()
    {
        if (CurrentPromotion >= 8) return; // 최대 승급 방어

        CurrentPromotion++;
        OnPromotionChanged?.Invoke(CurrentPromotion);
        RefreshStats();
    }
    public bool CanPromote(out Character_RankData nextData)
    {
        nextData = null;
        if (CurrentPromotion >= 8) return false;

        nextData = DataManager.Instance.GetData<Character_RankData>(CurrentPromotion + 1);
        bool levelOk = CurrentLevel >= nextData.Character_Rank_Level;
        bool goldOk = ExchangeManager.Instance.GetMoneyAmount(MoneyType.Gold) >= nextData.Character_Rank_Gold;
        return levelOk && goldOk;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Combat

    public void TakeDamage(float amount)
    {
        if (_player.IsInvincible) return;

        // 방어력 적용
        float def = 100 / (float)StatManager.Instance.GetStat(Status.DEF);
        amount = amount * def;

        CurrentHp = Mathf.Max(0f, CurrentHp - amount);
        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0f)
        {
            //Debug.Log("플레이어 사망");
            OnDead?.Invoke();
        }
    }

    public void ResetHPToMax()
    {
        CurrentHp = MaxHp;
        OnHpChanged?.Invoke(CurrentHp, MaxHp);
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region IResettable

    public void ResetState()
    {
        ResetHPToMax();
    }

    #endregion

    #region Total Power

    private void RefreshTotalPower()
    {
        double atk = StatManager.Instance.GetStat(Status.ATK);
        double hp = StatManager.Instance.GetStat(Status.HP);
        double def = StatManager.Instance.GetStat(Status.DEF);

        // 공격 점수
        double attackScore = atk;

        // 생존 점수
        double survivalScore = hp * (100.0 + def) / 100.0;

        // 최종 전투력
        double raw = Math.Pow(attackScore * survivalScore, 1.0 / 3.0);
        long power = (long)raw; 

        TotalPower = power.ToString("N0");
        OnTotalPowerChanged?.Invoke(TotalPower);
    }

    #endregion
}
