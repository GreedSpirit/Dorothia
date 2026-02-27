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

    void UpdateExp(float currentExp, double maxExp)
    {
        expSlider.value = (float)(currentExp / maxExp);

        float percent = (float)(currentExp / maxExp) * 100f;
        exp.text = ($"EXP  {percent.ToString("F0")}%   ({currentExp.ToString("F0")} / {maxExp.ToString("F0")})");        
    }

    void UpdateStats()
    {
        level.text = ($"Level. {playerStats._level}");
        hp.text = StatManager.Instance.GetStat(Status.HP).ToString();
        atk.text = StatManager.Instance.GetStat(Status.ATK).ToString();
        def.text = StatManager.Instance.GetStat(Status.DEF).ToString();
        mAtk.text = StatManager.Instance.GetStat(Status.MagicATK).ToString();
        mDef.text = StatManager.Instance.GetStat(Status.MagicDEF).ToString();
        cri.text = StatManager.Instance.GetStat(Status.CriticalChance).ToString();
        criDmg.text = StatManager.Instance.GetStat(Status.CriticalDamage).ToString();
        mSpd.text = StatManager.Instance.GetStat(Status.MoveSpeed).ToString();
        aSpd.text = StatManager.Instance.GetStat(Status.AttackSpeed).ToString();
        regen.text = StatManager.Instance.GetStat(Status.HPRegen).ToString();
    }
}
