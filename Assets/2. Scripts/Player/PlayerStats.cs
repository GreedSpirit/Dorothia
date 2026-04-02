using GameUtility;
using System;
using System.Collections;
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

    private readonly int _playerstats_id = 70001;
    private PlayerCtrl _player;
    private OverDriveMode _odm;

    public bool IsLoaded { get; private set; } = false;
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

    /// <summary>데이터 로드</summary>
    public event Action OnLoaded;

    #endregion

    // ══════════════════════════════════════════════════════
    #region Unity Lifecycle

    private void Start()
    {
        _player = GetComponent<PlayerCtrl>();
        _odm = GetComponent<OverDriveMode>();

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

        float statDef = (float)StatManager.Instance.GetStat(Status.DEF);

        float denominator = 100f + statDef;
        if (denominator <= 0) denominator = 1f; 

        float defMultiplier = 100f / denominator;

        float finalDamage = amount * defMultiplier;
        CurrentHp = Mathf.Max(0f, CurrentHp - finalDamage);

        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        if (CurrentHp <= 0f)
        {
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

    #region Save / Load

    public PlayerSaveData GetSaveData()
    {
        return new PlayerSaveData
        {
            level = CurrentLevel,
            currentExpStr = CurrentExp.ToString(),
            promotion = CurrentPromotion,
            statUpgrades = StatManager.Instance.GetStatUpgrades(),
            overdriveGauge = _odm != null ? _odm.Gauge : 0f,
            isAutoMode = _player != null && _player.IsAutoMode
        };
    }

    public void LoadFromSaveData(PlayerSaveData data)
    {
        if (data == null) return;

        // 레벨 / 경험치
        CurrentLevel = Mathf.Max(1, data.level);
        CurrentExp = System.Numerics.BigInteger.TryParse(data.currentExpStr, out var parsed)
                       ? parsed : System.Numerics.BigInteger.Zero;

        // 경험치 임계값 재계산
        StatManager.Instance.RefreshExp(CurrentLevel, 1.10f);

        // 승급
        CurrentPromotion = Mathf.Clamp(data.promotion, 1, 8);

        // 강화 단계 적용 후 전체 스탯 재계산
        StatManager.Instance.ApplyStatUpgrades(data.statUpgrades);
        RefreshStats();
        CurrentHp = MaxHp;

        // UI 이벤트 발행 (외형 포함)
        OnLevelChanged?.Invoke(CurrentLevel);
        OnExpChanged?.Invoke(CurrentExp, LevelExpN);
        // PlayerVisual.SetGrade 자동 호출
        OnPromotionChanged?.Invoke(CurrentPromotion);   
        OnHpChanged?.Invoke(CurrentHp, MaxHp);

        // 오버드라이브 게이지
        if (_odm != null)
            _odm.Gauge = data.overdriveGauge;

        // 자동전투 상태
        if (_player != null && data.isAutoMode != _player.IsAutoMode)
            _player.SetAutoMode(data.isAutoMode);

        IsLoaded = true;
        OnLoaded?.Invoke();
    }

    #endregion

}
