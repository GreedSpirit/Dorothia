using System;
using UnityEditor;
using UnityEngine;
public class PlayerStats : MonoBehaviour
{
    //CSV 캐릭터 스텟 데이터
    Character_StatsData _data;
    int _playerstats_id = 70001;

    public int _level;         //레벨
    public float _currentExp;  //현재 경험치
    public float _maxHp;       //체력
    public float _currentHp;   //현재 체력
    public float _atk;         //공격력
    public float _atk_m;       //마법공격력
    public float _dps;         //공격속도
    public float _crt_prob;    //크리티컬확률
    public float _crt_dmg;     //크리티컬대미지
    public float _def;         //방여력
    public float _def_m;       //마법방어력
    public float _hp_regen;    //체력재생력
    public float _agi;         //이동속도
    public int _upgrade_scrap_n; //첫업그레이드 시 소비하는 스크랩
    public double _level_exp_n;     //첫레벨업 시 필요한 경험치

    public event Action<float,float> OnHpChanged;
    public event Action<float,double> OnExpChanged;
    public event Action OnDead;

    PlayerCtrl _player;

    public float Attack => _atk;

    //테스트용
    //[SerializeField] private bool _useTestStats = true;
    //[SerializeField] private float _testMaxHp;
    //[SerializeField] private float _testAttack;


    //TODO 추후 datamanager를 타이틀씬에 배치해두고 Awake로 변경예정
    private void Start()
    {
        //CSV 기본값 셋팅
        _data = DataManager.Instance.GetData<Character_StatsData>(_playerstats_id);
        
        //스탯매니저셋팅
        StatManager.Instance.InitStats(_data);

        //TODO : 불러오기 함수 호출


        //스탯매니저계산값 적용
        StatManager.Instance.RefreshStats(_level);


        //스탯 적용
        _maxHp = (float)StatManager.Instance.GetStat(Status.HP);
        _atk = (float)StatManager.Instance.GetStat(Status.ATK);
        _atk_m = (float)StatManager.Instance.GetStat(Status.MagicATK);
        _dps = (float)StatManager.Instance.GetStat(Status.AttackSpeed);
        _crt_prob = (float)StatManager.Instance.GetStat(Status.CriticalChance);
        _crt_dmg = (float)StatManager.Instance.GetStat(Status.CriticalDamage);
        _def = (float)StatManager.Instance.GetStat(Status.DEF);
        _def_m = (float)StatManager.Instance.GetStat(Status.MagicDEF);
        _hp_regen = (float)StatManager.Instance.GetStat(Status.HPRegen);
        _agi = (float)StatManager.Instance.GetStat(Status.MoveSpeed);
        _level_exp_n = StatManager.Instance.GetStat(Status.Level_Exp_N);

        _currentHp = _maxHp;
    }

    private void OnEnable()
    {
        MonsterController.OnMonsterKilled += AddExp;
    }

    private void OnDisable()
    {
        MonsterController.OnMonsterKilled -= AddExp;
    }

    //TODO 추후 계산 공식 적용해야됨 현재는 테스트용 가데이터

    //저장된 데이터 불러오기
    void LoadStats()
    {
        /*
        _currentHp = 저장해둔클래스 hp값
        _currentExp = 저장해둔클래스 exp값

        OnHpChanged?.Invoke(_currentHp);
        OnExpChanged?.Invoke(_currentExp);
        */
    }

    //경험치 변화 알림
    public void AddExp(int mosterId, bool isBoss)
    {
        _currentExp += 500f;

        if (_currentExp >= _level_exp_n)
        {
            LevelUp();
        }

        OnExpChanged?.Invoke(_currentExp, _level_exp_n);
    }

    public void LevelUp()
    {
        if (_level >= 200) return;

        //요구했던 경험치량 저장
        double save_Level_Exp_N = _level_exp_n;

        //레벨업
        _level++;

        //업된 레벨기준 재계산
        StatManager.Instance.RefreshStats(_level);

        //스탯 적용
        _maxHp = (float)StatManager.Instance.GetStat(Status.HP);
        _atk = (float)StatManager.Instance.GetStat(Status.ATK);
        _atk_m = (float)StatManager.Instance.GetStat(Status.MagicATK);
        _dps = (float)StatManager.Instance.GetStat(Status.AttackSpeed);
        _crt_prob = (float)StatManager.Instance.GetStat(Status.CriticalChance);
        _crt_dmg = (float)StatManager.Instance.GetStat(Status.CriticalDamage);
        _def = (float)StatManager.Instance.GetStat(Status.DEF);
        _def_m = (float)StatManager.Instance.GetStat(Status.MagicDEF);
        _hp_regen = (float)StatManager.Instance.GetStat(Status.HPRegen);
        _agi = (float)StatManager.Instance.GetStat(Status.MoveSpeed);        
        _level_exp_n = StatManager.Instance.GetStat(Status.Level_Exp_N);

        _currentHp = _maxHp;
        _currentExp -= (float)save_Level_Exp_N;
    }

    public void TakeDamage(int amount)
    {
        _currentHp -= amount;

        Debug.Log($"Damaged: {amount}, HP: {_currentHp}");

        OnHpChanged?.Invoke(_currentHp, _maxHp);

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Debug.Log("플레이어 사망");
            OnDead?.Invoke();
        }
    }

    public void ResetHPToMax()
    {
        _currentHp = _maxHp;
        OnHpChanged?.Invoke(_currentHp, _maxHp);
    }
}
