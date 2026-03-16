// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// PassiveSkill.cs
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkill : BaseSkill
{
    // 실제로 적용된 스탯 수치 기록 (Undo 시 정확히 원복)
    //private readonly Dictionary<StatType, float> _appliedValues
    //    = new Dictionary<StatType, float>();

    private bool _isApplied = false;

    public void Apply()
    {
        if (_isApplied) return;

        StatManager.Instance.stats[Data.Affection_Skill].AddMultiModifier(Data.Affection_Skill_Value);

        _isApplied = true;
        Debug.Log($"[PassiveSkill] {Data.Skill_Name} 적용 완료");
    }

    public void Undo()
    {
        if (!_isApplied) return;

        StatManager.Instance.RefreshStats();

        _isApplied = false;

        Debug.Log($"[PassiveSkill] {Data.Skill_Name} 해제 완료");
    }

    // 패시브는 직접 실행 없음 (장착/해제가 전부)
    public override void Execute(PlayerCtrl player)
    {
        Debug.LogWarning("[PassiveSkill] 패시브 스킬은 Execute로 호출하지 않습니다. Apply()를 사용하세요.");
    }

    //// (등급, 레벨에 따른 스케일링)
    //private float CalculateStatValue()
    //{
    //    float baseValue = entry.BaseValue;
    //    float rarityMult = GetRarityMultiplier();
    //    float levelMult = 1f + (Level * entry.ValuePerLevel);

    //    return baseValue * rarityMult * levelMult;
    //}

    //private float GetRarityMultiplier()
    //{
    //    return Rarity switch
    //    {
    //        Rarity.Normal => 1.0f,
    //        Rarity.Rare => 1.3f,
    //        Rarity.Epic => 1.6f,
    //        Rarity.Legendary => 2.0f,
    //        _ => 1.0f
    //    };
    //}
}