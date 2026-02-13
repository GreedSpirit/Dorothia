using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FinalStat
{
    public float baseStat;
    public float additiveStat = 0f;
    public float multiStat = 1f;

    public float finalStat => (baseStat + additiveStat) * multiStat;

    public FinalStat()
    {
        additiveStat = 0f;
        multiStat = 1f;
    }

    public FinalStat(float baseStat) : base()
    {
        this.baseStat = baseStat;
    }

    public void AddModifier(float add, float mult)
    {
        additiveStat += add;
        multiStat += mult; // 여기서 mult는 0.1(10%) 같은 소수점 값
    }

    public void ResetModifiers()
    {
        additiveStat = 0;
        multiStat = 1f;
    }
}

public class StatManager : MonoBehaviour
{
    static private StatManager instance;

    static public StatManager Instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;

    }

    public Dictionary<Status, FinalStat> status = new Dictionary<Status, FinalStat>();
    public FinalStat GetStat(Status type) => status[type];

    private void Start()
    {
        InitStats();
    }

    private void InitStats()
    {
        foreach (Status type in Enum.GetValues(typeof(Status)))
        {
            status[type] = new FinalStat();
        }
    }

    public void ResetStats()
    {
        foreach (var stat in status.Values)
        {
            stat.ResetModifiers();
        }
    }

    public void UpdatePassiveSkillAffect()
    {
        ResetStats();

        foreach (var skill in SkillManager.Instance.passiveSkillSlots)
        {
            Status type = skill.Status.Affection_Skill;
            float value = skill.Status.Affection_Skill_Value;

            status[type].AddModifier(0, value);
        }

        Debug.Log("패시브효과가 갱신되었습니다.");
    }

    public void UpdateEquipmentAffect()
    {
        ResetStats();

        Debug.Log("장비효과가 갱신되었습니다.");
    }

}
