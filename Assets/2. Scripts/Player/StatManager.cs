using GameUtility;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

[Serializable]
public class FinalStat
{
    // ══════════════════════════════════════════════════════
    #region Fields

    public double baseStat;
    public float growAdditiveStat;   // 레벨 성장 / 강화 합산
    public float equipAdditiveStat;  // 장비 합산
    public float multiStat;          // 곱연산 합산 (기본 1.0)
    public float weight = 1.02f;     // 레벨업 가중치

    public bool isScaledByLevel;      // 레벨 성장 적용 여부

    // ── 강화 ──────────────────────────────────────────────
    public int upgradeLevel;         // 현재 강화 단계
    public int maxUpgradeLevel;      // 최대 강화 단계
    public float upgradeValuePerStep;  // 강화 1단계당 증가값

    private double cachedValue;
    private double? overrideValue = null;

    #endregion

    // ══════════════════════════════════════════════════════
    #region Properties

    public double FinalValue => cachedValue;
    public bool CanUpgrade => maxUpgradeLevel > 0 && upgradeLevel < maxUpgradeLevel;

    #endregion

    // ══════════════════════════════════════════════════════
    #region Constructor

    public FinalStat(double baseStat, float weight = 1.02f,
                     bool isScaledByLevel = true,
                     int maxUpgradeLevel = 0, float upgradeValuePerStep = 0f)
    {
        this.baseStat = baseStat;
        this.weight = weight;
        this.isScaledByLevel = isScaledByLevel;
        this.maxUpgradeLevel = maxUpgradeLevel;
        this.upgradeValuePerStep = upgradeValuePerStep;
        ResetModifiers();
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Calculation

    public void UpdateFinalValue(int level, float promotionMulti = 1f)
    {
        if (overrideValue.HasValue)
        {
            cachedValue = overrideValue.Value;
            return;
        }

        double totalBeforePercent;

        if (isScaledByLevel)
        {
            double characterGrowth = (baseStat + growAdditiveStat) * Mathf.Pow(weight, level - 1);
            totalBeforePercent = (characterGrowth * promotionMulti) + equipAdditiveStat;
        }
        else
        {
            // 레벨 성장 없음 : 기본값 + 강화값 + 장비값
            totalBeforePercent = baseStat + growAdditiveStat + equipAdditiveStat;
        }

        cachedValue = totalBeforePercent * multiStat;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Modifier

    public void AddGrowModifier(float add) => growAdditiveStat += add;
    public void AddEquipModifier(float add) => equipAdditiveStat += add;
    public void AddMultiModifier(float add) => multiStat += add;
    public void SetOverrideValue(double? value) => overrideValue = value;

    /// <summary>RefreshStats 시 매번 호출. 강화값은 유지, 장비·버프만 초기화.</summary>
    public void ResetModifiers()
    {
        equipAdditiveStat = 0f;
        multiStat = 1f;
        overrideValue = null;

        // 강화값은 영구 반영 (upgradeLevel 기준 재계산)
        growAdditiveStat = upgradeLevel * upgradeValuePerStep;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Upgrade

    /// <summary>
    /// 강화 1단계 진행. 성공 시 true 반환.
    /// RefreshStats는 StatManager에서 호출하므로 여기선 수치만 변경.
    /// </summary>
    public bool TryUpgrade()
    {
        if (!CanUpgrade) return false;

        upgradeLevel++;
        growAdditiveStat += upgradeValuePerStep;
        return true;
    }

    #endregion
}

// ──────────────────────────────────────────────────────────────────────────────
// StatManager : 스탯 계산 전담 싱글톤
// ──────────────────────────────────────────────────────────────────────────────
public class StatManager : MonoBehaviour
{
    // ══════════════════════════════════════════════════════
    #region Singleton

    private static StatManager _instance;
    public static StatManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Fields & Properties

    [SerializeField] private OverDriveMode _odm;

    public Dictionary<Status, FinalStat> stats = new Dictionary<Status, FinalStat>();

    /// <summary>레벨업 필요 경험치 (BigInteger이므로 별도 보관)</summary>
    public BigInteger LevelExpN { get; private set; }

    // 재계산 시 사용할 캐싱값
    private int _currentLevel;
    private float _currentPromotion;

    private const int MAX_UPGRADE_LEVEL = 50;

    public event Action OnStatsRefreshed;
    #endregion

    // ══════════════════════════════════════════════════════
    #region Init

    public void InitStats(Character_StatsData data)
    {
        _currentLevel = data.Character_Level;
        _currentPromotion = 1f;

        // 레벨 성장 O 스탯
        stats[Status.Level] = new FinalStat(data.Character_Level);
        stats[Status.HP] = new FinalStat(data.Character_Hp);
        stats[Status.ATK] = new FinalStat(data.Character_Atk);
        stats[Status.MagicATK] = new FinalStat(data.Character_Atk_M);
        stats[Status.DEF] = new FinalStat(data.Character_Def);
        stats[Status.MagicDEF] = new FinalStat(data.Character_Def_M);

        // 강화 단계별 증가값 로드
        var upgradeData = DataManager.Instance.GetData<Character_UpgradeData>(data.Character_Id);

        // 레벨 성장 X + 최대 50강 스탯
        stats[Status.AttackSpeed] = new FinalStat(data.Character_Dps, isScaledByLevel: false, maxUpgradeLevel: MAX_UPGRADE_LEVEL, upgradeValuePerStep: upgradeData.Character_Upgrade_Dps);
        stats[Status.CriticalChance] = new FinalStat(data.Character_Crt_Prob, isScaledByLevel: false, maxUpgradeLevel: MAX_UPGRADE_LEVEL, upgradeValuePerStep: upgradeData.Character_Upgrade_Crt_Prob);
        stats[Status.CriticalDamage] = new FinalStat(data.Character_Crt_Dmg, isScaledByLevel: false, maxUpgradeLevel: MAX_UPGRADE_LEVEL, upgradeValuePerStep: upgradeData.Character_Upgrade_Crt_Dmg);
        stats[Status.HPRegen] = new FinalStat(data.Character_Hp_Regen, isScaledByLevel: false, maxUpgradeLevel: MAX_UPGRADE_LEVEL, upgradeValuePerStep: upgradeData.Character_Upgrade_Hp_Regen);
        stats[Status.MoveSpeed] = new FinalStat(data.Character_Agi, isScaledByLevel: false, maxUpgradeLevel: MAX_UPGRADE_LEVEL, upgradeValuePerStep: upgradeData.Character_Upgrade_Agi);

        LevelExpN = data.Character_Level_Exp_N;
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Refresh

    public void RefreshExp(int currentLevel, float LEVEL_WEIGHT)
    {
        LevelExpN = LevelExpN.MultiplyPower(LEVEL_WEIGHT, currentLevel);
    }

    /// <summary>캐싱된 레벨·승급으로 재계산 (장비·스킬 변경 시 호출)</summary>
    public void RefreshStats() => RefreshStats(_currentLevel, _currentPromotion);

    /// <summary>
    /// 스탯에 변화가 있을 때 반드시 호출.
    /// 장비 탈착 / 스킬 탈착 / 강화 / 레벨업 / 승급
    /// </summary>
    public void RefreshStats(int level, float promotion = 1f)
    {
        _currentLevel = level;
        _currentPromotion = promotion;

        foreach (var stat in stats.Values)
            stat.ResetModifiers();

        ApplyPassiveEffects();
        ApplyEquipmentStats();
        ApplyODMModifiers();

        foreach (var stat in stats.Values)
            stat.UpdateFinalValue(level, promotion);

        OnStatsRefreshed?.Invoke();
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Apply Effects

    private void ApplyPassiveEffects()
    {
        // 패시브 스킬 적용
        if (SkillManager.Instance != null)
        {
            foreach (var skill in SkillManager.Instance.PassiveSlots)
            {
                if (skill == null) continue;
                stats[skill.Data.Affection_Skill].AddMultiModifier(skill.Data.Affection_Skill_Value);
            }
        }

        // 그렘린 패시브 적용
        if (GremlinManager.Instance?.gremlinInstance != null)
        {
            if (GremlinManager.Instance.currentGremlin._behaviour is BufferGremlin gremlin)
            {
                foreach (var passive in gremlin.PassiveStatus.Keys)
                {
                    float value = ItemCalculator.BufferGremlinValueCalc(
                        GremlinManager.Instance.currentGremlin, gremlin.PassiveStatus[passive], gremlin);
                    stats[passive].AddMultiModifier(value);
                }
            }
        }
    }

    private void ApplyEquipmentStats()
    {
        if (EquipmentSlotManager.Instance == null) return;

        foreach (var kvp in EquipmentSlotManager.Instance.EquipmentStatus)
            stats[kvp.Key].AddEquipModifier(kvp.Value);

        foreach (var kvp in EquipmentSlotManager.Instance.SetStatus)
            stats[kvp.Key].AddMultiModifier(kvp.Value);
    }

    private void ApplyODMModifiers()
    {
        if (_odm == null || !_odm.IsModeOn) return;

        stats[Status.ATK].AddMultiModifier(0.3f);          // 공격력 x1.3
        stats[Status.AttackSpeed].AddMultiModifier(0.5f);   // 공격속도 x1.5
        stats[Status.MoveSpeed].SetOverrideValue(3.0);      // 이동속도 3 고정
    }

    #endregion

    // ══════════════════════════════════════════════════════
    #region Getter

    public double GetStat(Status type) => stats[type].FinalValue;

    #endregion
}
