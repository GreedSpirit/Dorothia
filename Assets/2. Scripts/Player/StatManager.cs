using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.Rendering;
using UnityEngine;

[Serializable]
public class FinalStat
{
    public float baseStat;       // 최초 스탯 (Lv.1)
    public float additiveStat;   
    public float multiStat;      // 추가 곱셈 비율 (기본 1.0 = 100%)
    public float weight = 1.025f; // 레벨업 가중치

    public FinalStat(float baseStat, float weight = 1.025f)
    {
        this.baseStat = baseStat;
        this.weight = weight;
        ResetModifiers();
    }

    public FinalStat() : this(0f) { }

    // 최종 스탯 계산 (레벨, 승급계수, 장비합산, 장비세트, 패시브 등을 인자로 받음)
    public float GetFinalValue(int level, float promotionMulti, float equipAdd, float equipSetMult, float passiveMult)
    {
        // (기본 + 추가) * 가중치^(레벨-1) * 승급계수
        float growthStat = (baseStat + additiveStat) * Mathf.Pow(weight, level - 1) * promotionMulti;

        // 장비 스탯 합산
        float totalBeforeMult = growthStat + equipAdd;

        // 최종 퍼센트 적용: (1 + 장비세트 + 패시브)
        // multiStat이 1.1이면 10% 추가를 의미하도록 설계
        float totalMultiplier = multiStat + equipSetMult + passiveMult;

        return totalBeforeMult * totalMultiplier;
    }

    public void AddModifier(float add, float mult = 0)
    {
        additiveStat += add;
        multiStat += mult;
    }

    public void ResetModifiers()
    {
        additiveStat = 0;
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

        // 패시브 스킬 적용
        if (SkillManager.Instance != null)
        {
            foreach (var skill in SkillManager.Instance.passiveSkillSlots)
            {
                Status type = skill.Status.Affection_Skill;
                float value = skill.Status.Affection_Skill_Value;
                stats[type].AddModifier(0, value);
            }
        }

        // 장비 효과 적용
        ApplyEquipmentStats();

        Debug.Log("모든 스탯 수치가 최신화되었습니다.");
    }

    private void ApplyEquipmentStats()
    {
        // 장비 매니저 등에서 데이터를 가져와 stats[type].AddModifier 호출
    }

    // 최종 스탯이 필요할 때 호출 (예: 공격할 때)
    public float GetFinalStatValue(Status type)
    {
        // 여기서 현재 플레이어의 레벨, 승급 등을 인자로 전달
        // 예시 값 전달: 레벨 10, 승급 1.2배, 장비합 0, 장비셋 0, 패시브는 이미 multiStat에 포함됨
        return stats[type].GetFinalValue(10, 2f, 0, 0, 0);
    }
}