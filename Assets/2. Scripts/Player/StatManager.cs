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

    //{(캐릭터 스테이터스 * 레벨업 스테이터스 가중치 * 승급}+ 장비스탯} * (1 + 장비세트 효과 * 패시브)

    //todo : 계산식 고치기
    /// <summary>
    /// Level : 플레이어 레벨
    /// promotion : 플레이어 승급 단계
    /// equipAdd : 장비 수치?(지워야할듯)
    /// </summary>
    /// <param name="level"></param>
    /// <param name="promotionMulti"></param>
    /// <param name="equipAdd"></param>
    /// <returns></returns>
    public void UpdateFinalValue(int level, float promotionMulti = 1)
    {

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
        multiStat = 1f; // 기본 배율은 100%
    }
}

public class StatManager : MonoBehaviour
{
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

    // 스탯에 변화가 있을 때 무조건 호출
    // 장비탈착, 스킬탈착, 스탯 업그레이드, 레벨업, 승급
    public void RefreshStats(int level)
    {
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



    public double GetStat(Status type)
    {
        return stats[type].FinalValue;
    }
}