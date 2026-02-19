using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FinalStat
{
    public float baseStat;       // 최초 스탯 (Lv.1)
    public float growAdditiveStat;
    public float equipAdditiveStat;
    public float multiStat;      // 추가 곱셈 비율 (기본 1.0 = 100%)
    public float weight = 1.025f; // 레벨업 가중치

    public FinalStat(float baseStat, float weight = 1.025f)
    {
        this.baseStat = baseStat;
        this.weight = weight;
        ResetModifiers();
    }

    //{(캐릭터 스테이터스 * 레벨업 스테이터스 가중치 * 승급}+ 장비스탯} * (1 + 장비세트 효과 * 패시브)
    public float GetFinalValue(int level, float promotionMulti, float equipAdd)
    {
        // 순수 캐릭터 성장치 계산 (승급 포함)
        float characterGrowth = baseStat * Mathf.Pow(weight, level - 1);

        // 장비 고정 스탯 합산
        float totalBeforePercent = (characterGrowth * promotionMulti) + equipAdd;

        // 최종 퍼센트 효과 적용 (1 + 장비세트 + 패시브 등등의 합산 결과가 multiStat)
        return totalBeforePercent * multiStat;
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
    public static StatManager Instance;

    public Dictionary<Status, FinalStat> stats = new Dictionary<Status, FinalStat>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        InitStats();
    }

    private void InitStats()
    {
        foreach (Status type in Enum.GetValues(typeof(Status)))
        {
            // todo : 실제 데이터에서 기본값을 가져와 생성하도록 수정 필요
            stats[type] = new FinalStat(10f);
        }
    }

    // 모든 스탯 영향을 한 번에 계산
    public void RefreshStats()
    {
        // 모든 수치 초기화
        foreach (var stat in stats.Values) stat.ResetModifiers();



        // 장비 효과 적용
        ApplyEquipmentStats();

        Debug.Log("모든 스탯 수치가 최신화되었습니다.");
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
        // 장비의 고정 수치는 add에, 세트효과는 multi에 더함
        // stats[type].AddModifier(equip.power, equip.powerPercent);
    }

    public float GetFinalStatValue(Status type)
    {
        if (!stats.ContainsKey(type)) return 0f;

        // 현재 플레이어 데이터 매니저 등에서 값 가져오기
        //int level = Player.Level;
        //float promotion = Player.PromotionMultiplier;

        //return stats[type].GetFinalValue(level, promotion, equipAdd);

        return 0f;
    }
}