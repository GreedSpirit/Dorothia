using System;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharactorPopup : BaseUI
{
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI exp;
    [SerializeField] private Slider expSlider;

    [SerializeField] private TextMeshProUGUI totalPower;

    [SerializeField] private TextMeshProUGUI hp;
    [SerializeField] private TextMeshProUGUI atk;
    [SerializeField] private TextMeshProUGUI def;
    [SerializeField] private TextMeshProUGUI mAtk;
    [SerializeField] private TextMeshProUGUI mDef;
    [SerializeField] private TextMeshProUGUI cri;
    [SerializeField] private TextMeshProUGUI criDmg;
    [SerializeField] private TextMeshProUGUI mSpd;
    [SerializeField] private TextMeshProUGUI aSpd;
    [SerializeField] private TextMeshProUGUI regen;

    [SerializeField] private StatUpgradePopup statUpgradePopup;

    private void OnEnable()
    {
        playerStats.OnExpChanged += UpdateExp;
        playerStats.LevelChanged += UpdateStats;
    }

    private void OnDisable()
    {
        playerStats.OnExpChanged -= UpdateExp;
        playerStats.LevelChanged -= UpdateStats;
    }
    protected override void OnOpen()
    {
        //rank.text = 
        //totalPower = 


        UpdateExp(playerStats._currentExp, playerStats._level_exp_n);

        UpdateStats();
    }

    protected override void OnClose()
    {
    }


    public void Click_UpgradePopup(int type)
    {
        statUpgradePopup.SetType((StatUpgradePopup.Type)type);
        UIManager.Instance.OpenPanel(statUpgradePopup);
    }

    void UpdateExp(BigInteger currentExp, BigInteger maxExp)
    {
        expSlider.value = (float)(currentExp / maxExp);

        float percent = (float)(currentExp / maxExp) * 100f;
        exp.text = ($"EXP  {percent.ToString("F0")}%   ({currentExp.ToString("F0")} / {maxExp.ToString("F0")})");        
    }

    void UpdateStats()
    {
        level.text = ($"Level. {playerStats._level}");
        //정수 표시하고 소수점 첫째부터 내림
        hp.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.HP)).ToString();
        atk.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.ATK)).ToString();
        def.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.DEF)).ToString();
        mDef.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.MagicDEF)).ToString();
        //소수점 첫째까지 표시 두번째부터 내림
        mAtk.text = (Math.Floor(StatManager.Instance.GetStat(Status.MagicATK) * 10) / 10).ToString("F1");
        cri.text = (Math.Floor(StatManager.Instance.GetStat(Status.CriticalChance) * 10) / 10).ToString("F1");
        criDmg.text = (Math.Floor(StatManager.Instance.GetStat(Status.CriticalDamage) * 10) / 10).ToString("F1");
        mSpd.text = (Math.Floor(StatManager.Instance.GetStat(Status.MoveSpeed) * 10) / 10).ToString("F1");
        aSpd.text = (Math.Floor(StatManager.Instance.GetStat(Status.AttackSpeed) * 10) / 10).ToString("F1");
        regen.text = (Math.Floor(StatManager.Instance.GetStat(Status.HPRegen) * 10) / 10).ToString("F1");
    }
}
