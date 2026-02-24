using System;
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

    public event Action<float> OnHpChanged;
    public event Action<float> OnExpChanged;
    public event Action OnDead;

    PlayerCtrl _player;

    public float Attack => _atk;

    //테스트용
    [SerializeField] private bool _useTestStats = true;
    [SerializeField] private float _testMaxHp;
    [SerializeField] private float _testAttack;


    //TODO 추후 datamanager를 타이틀씬에 배치해두고 Awake로 변경예정
    private void Start()
    {
        if (_useTestStats)
        {
            _maxHp = _testMaxHp;
            _atk = _testAttack;

            Debug.Log("테스트모드");
        }
        else
        {
            //CSV 기본값 셋팅
            _data = DataManager.Instance.GetData<Character_StatsData>(_playerstats_id);

            _level = _data.Character_Level;
            _maxHp = _data.Character_Hp;
            _atk = _data.Character_Atk;
            _atk_m = _data.Character_Atk_M;
            _dps = _data.Character_Dps;
            _crt_prob = _data.Character_Crt_Prob;
            _crt_dmg = _data.Character_Crt_Dmg;
            _def = _data.Character_Def;
            _def_m = _data.Character_Def_M;
            _hp_regen = _data.Character_Hp_Regen;
            _agi = _data.Character_Agi;
            _upgrade_scrap_n = _data.Character_Upgrade_Scrap_N;
            _level_exp_n = _data.Character_Level_Exp_N;
        }

        _currentHp = _maxHp;

        Debug.Log($"HP: {_currentHp}/{_maxHp}  ATK: {_atk}");
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
    public void AddExp()
    {

    }

    public void TakeDamage(int amount)
    {
        _currentHp -= amount;

        Debug.Log($"Damaged: {amount}, HP: {_currentHp}");

        OnHpChanged?.Invoke(_currentHp);

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
        OnHpChanged?.Invoke(_currentHp);
    }
}
