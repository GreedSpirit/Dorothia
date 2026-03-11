using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FinalStat
{
    public double baseStat;       // 최초 스탯 (Lv.1)
    public float growAdditiveStat;
    public float equipAdditiveStat;
    public float multiStat;      // 추가 곱셈 비율 (기본 1.0 = 100%)
    public float weight = 1.025f; // 레벨업 가중치
    

    public FinalStat(double baseStat, float weight = 1.025f)
    {
        this.baseStat = baseStat;
        this.weight = weight;
        ResetModifiers();
    }
    // 캐싱된 최종 값
    private double cachedValue;

    // 외부에서는 이 프로퍼티만 읽어감 (추가 연산 없음)
    public double FinalValue => cachedValue;

    private double? overrideValue = null; // 고정값 사용 여부

    // 외부에서 고정값을 지정할 때 사용
    public void SetOverrideValue(double? value)
    {
        overrideValue = value;
    }

    public void UpdateFinalValue(int level, float promotionMulti = 1)
    {
        // 고정값이 설정되어 있다면 계산을 무시하고 해당 값 사용
        if (overrideValue.HasValue)
        {
            cachedValue = overrideValue.Value;
            return;
        }

        double characterGrowth = (baseStat + growAdditiveStat) * Mathf.Pow(weight, level - 1);
        double totalBeforePercent = (characterGrowth * promotionMulti) + equipAdditiveStat;
        cachedValue = totalBeforePercent * multiStat;
    }

    public void AddGrowModifier(float add)
    {
        growAdditiveStat += add;
    }
    public void AddEquipModifier(float add)
    {
        equipAdditiveStat += add;
    }
    public void AddMultiModifier(float add)
    {
        multiStat += add;
    }
    public void ResetModifiers()
    {
        growAdditiveStat = 0;
        equipAdditiveStat = 0;
        multiStat = 1f;
        overrideValue = null; // 리셋 시 고정값 해제
    }
}

public class StatManager : MonoBehaviour
{
    //캐싱시킬 레벨변수
    int _currentLevel;

    private static StatManager instance;
    public static StatManager Instance => instance;

    public Dictionary<Status, FinalStat> stats = new Dictionary<Status, FinalStat>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        //InitStats();
    }

    [SerializeField] private OverDriveMode _odm;

    //private void InitStats()
    //{
    //    foreach (Status type in Enum.GetValues(typeof(Status)))
    //    {
    //        // todo : 실제 데이터에서 기본값을 가져와 생성하도록 수정 필요
    //        stats[type] = new FinalStat(10f);
    //    }
    //}


    public void InitStats(Character_StatsData data)
    {
        //초기화할때 레벨 캐싱
        _currentLevel = data.Character_Level;

        stats[Status.Level] = new FinalStat(data.Character_Level);
        stats[Status.HP] = new FinalStat(data.Character_Hp);
        stats[Status.ATK] = new FinalStat(data.Character_Atk);
        stats[Status.MagicATK] = new FinalStat(data.Character_Atk_M);
        stats[Status.AttackSpeed] = new FinalStat(data.Character_Dps);
        stats[Status.CriticalChance] = new FinalStat(data.Character_Crt_Prob);
        stats[Status.CriticalDamage] = new FinalStat(data.Character_Crt_Dmg);
        stats[Status.DEF] = new FinalStat(data.Character_Def);
        stats[Status.MagicDEF] = new FinalStat(data.Character_Def_M);
        stats[Status.HPRegen] = new FinalStat(data.Character_Hp_Regen);
        stats[Status.MoveSpeed] = new FinalStat(data.Character_Agi);
        stats[Status.Level_Exp_N] = new FinalStat(data.Character_Level_Exp_N);

    }

    //슬롯매니저에서 매개변수 없이 캐싱된 레벨을 호출하기위해 오버로딩
    public void RefreshStats()
    {
        RefreshStats(_currentLevel);        
    }

    // 스탯에 변화가 있을 때 무조건 호출
    // 장비탈착, 스킬탈착, 스탯 업그레이드, 레벨업, 승급
    public void RefreshStats(int level)
    {
        _currentLevel = level;

        // 모든 스탯을 초기화
        foreach (var stat in stats.Values)
        {
            stat.ResetModifiers();
        }

        //스탯 업그레이드 내역 적용
        //ApplyGrowStat();

        // 패시브 효과 적용
        ApplyPassiveEffects();

        // 장비 효과 적용
        ApplyEquipmentStats();

        ApplyODMModifiers();

        //int currentLevel = PlayerManager.Instance.Level;
        //float promotion = PlayerManager.Instance.Promotion;

        foreach (var stat in stats.Values)
        {
            //stat.UpdateFinalValue(currentLevel, promotion, 0);
            stat.UpdateFinalValue(level);
        }
        
    }

    private void ApplyGrowStats()
    {

    }

    private void ApplyPassiveEffects()
    {
        // 패시브 스킬 적용
        if (SkillManager.Instance != null)
        {
            foreach (var skill in SkillManager.Instance.passiveSkillSlots)
            {
                Status type = skill.Status.Affection_Skill;
                float value = skill.Status.Affection_Skill_Value;
                stats[type].AddMultiModifier(value);
            }
        }
        //그렘린 패시브 적용
        if (GremlinManager.Instance != null && GremlinManager.Instance.gremlinInstance != null)
        {
            BufferGremlin gremlin = GremlinManager.Instance.currentGremlin._behaviour as BufferGremlin;
            if(gremlin != null)
            {
                foreach (var passive in gremlin.PassiveStatus.Keys)
                {
                    stats[passive].AddMultiModifier(gremlin.PassiveStatus[passive]);
                }
            }
        }
    }

    private void ApplyEquipmentStats()
    {
        if (EquipmentSlotManager.Instance != null)
        {
            // 장비 장착 효과
            foreach (var stat in EquipmentSlotManager.Instance.EquipmentStatus.Keys)
            {
                stats[stat].AddEquipModifier(EquipmentSlotManager.Instance.EquipmentStatus[stat]);
            }

            // 장비 세트 효과
            foreach (var stat in EquipmentSlotManager.Instance.SetStatus.Keys)
            {
                stats[stat].AddMultiModifier(EquipmentSlotManager.Instance.SetStatus[stat]);
            }
        }
    }

    private void ApplyODMModifiers()
    {
        if (_odm != null && _odm.IsModeOn)
        {
            // 공격력 1.3배 (증가량 0.3을 더함)
            if (stats.ContainsKey(Status.ATK))
                stats[Status.ATK].AddMultiModifier(0.3f);

            // 공격 속도 1.5배 (증가량 0.5를 더함)
            if (stats.ContainsKey(Status.AttackSpeed))
                stats[Status.AttackSpeed].AddMultiModifier(0.5f);

            // 이동 속도 3으로 고정
            if (stats.ContainsKey(Status.MoveSpeed))
                stats[Status.MoveSpeed].SetOverrideValue(3.0);
        }
    }

    public double GetStat(Status type)
    {
        return stats[type].FinalValue;
    }
}