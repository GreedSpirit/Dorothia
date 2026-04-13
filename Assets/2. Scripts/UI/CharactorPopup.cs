using System;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharactorPopup : BaseUI
{

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
        PlayerStats.Instance.OnExpChanged += UpdateExp;
        PlayerStats.Instance.OnLevelChanged += UpdateStats;
        StatManager.Instance.OnStatsRefreshed += RefreshUI;
    }

    private void OnDisable()
    {
        PlayerStats.Instance.OnExpChanged -= UpdateExp;
        PlayerStats.Instance.OnLevelChanged -= UpdateStats;
        StatManager.Instance.OnStatsRefreshed -= RefreshUI;
    }
    protected override void OnOpen()
    {
        UpdateExp(PlayerStats.Instance.CurrentExp, PlayerStats.Instance.LevelExpN);

        UpdateStats(PlayerStats.Instance.CurrentLevel);
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
        float percent = (float)(currentExp * 100 / maxExp);

        expSlider.value = (float)percent/100;
        exp.text = ($"EXP  {percent.ToString("F0")}%   ({currentExp.ToString("F0")} / {maxExp.ToString("F0")})");        
    }

    void RefreshUI(){
        UpdateStats(PlayerStats.Instance.CurrentLevel);
    }

    void UpdateStats(int currentLvl)
    {
        int promotion = PlayerStats.Instance.CurrentPromotion;
        Character_RankData rankData = DataManager.Instance.GetData<Character_RankData>(promotion);

        rank.text = rankData.Character_Name;

        level.text = ($"Level. {currentLvl}");
        //정수 표시하고 소수점 첫째부터 내림
        hp.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.HP)).ToString();
        atk.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.ATK)).ToString();
        def.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.DEF)).ToString();
        mDef.text = Mathf.FloorToInt((float)StatManager.Instance.GetStat(Status.MagicDEF)).ToString();
        //소수점 첫째까지 표시 두번째부터 내림
        mAtk.text = (Math.Floor(StatManager.Instance.GetStat(Status.MagicATK) * 10) / 10).ToString("F1");
        cri.text = ($"{(Math.Floor(StatManager.Instance.GetStat(Status.CriticalChance) * 10) / 10).ToString("F1")}%");
        criDmg.text = (Math.Floor(StatManager.Instance.GetStat(Status.CriticalDamage) * 10) / 10).ToString("F1");
        mSpd.text = (Math.Floor(StatManager.Instance.GetStat(Status.MoveSpeed) * 10) / 10).ToString("F1");
        aSpd.text = (Math.Floor(StatManager.Instance.GetStat(Status.AttackSpeed) * 10) / 10).ToString("F1");
        regen.text = (Math.Floor(StatManager.Instance.GetStat(Status.HPRegen) * 10) / 10).ToString("F1");

        totalPower.text = $"종합 전투력 : {PlayerStats.Instance.TotalPower}";
    }
}
