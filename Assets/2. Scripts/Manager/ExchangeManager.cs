using System.Collections.Generic;
using UnityEngine;

public enum MoneyType
{
    Scrap, Gold, CorePiece, FlintPiece, PinionPiece, ZincPiece
}
public class MoneyManager : MonoBehaviour
{
    //싱글톤
    public static MoneyManager Instance { get; private set; }

    //저장해둘 재화
    Dictionary<MoneyType, int> _money = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //재화불러오기
            //LoadMoney();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //잔고 조회
    public int GetMoneyAmount(MoneyType type)
    {
        CheckDictionary(type);
        return _money[type];
    }

    //재화획득시 호출할 함수
    public void GetMoney(MoneyType type, int amount)
    {
        CheckDictionary(type);
        _money[type] += amount;
    }

    //살수있는지 없는지 불값반환
    public bool UseMoney(MoneyType type, int amount)
    {
        //가진돈이 적다면 거짓반환
        if (_money[type] < amount)
        {
            return false;
        }
        //살수있다면
        else
        {
            //계산하고 참반환
            _money[type] -= amount;
            return true;
        }
        
    }

    //싱글플레이 기준 저장
    void SaveMoney()
    {
        PlayerPrefs.SetInt("Gold", _money[MoneyType.Gold]);
        PlayerPrefs.SetInt("Scrap", _money[MoneyType.Scrap]);
        PlayerPrefs.SetInt("CorePiece", _money[MoneyType.CorePiece]);

        PlayerPrefs.Save();
    }



    /*
    파이어베이스 재화불러오기
    void LoadMoney()
    {
      //파이어베이스 사용시 서버데이터 불러오고
      //Savedata data = SaveManager.Instance.Load();

      _money[MoneyType.Gold] = data.Gold;
      _money[MoneyType.Scrap] = data.Scrap;
      _money[MoneyType.GremlinPiece] = data.GremlinPiece;
    }
    */
    
    /// <summary>
    /// Dictionary에 해당 MoneyType Key가 있는지 체크하고, 없을 경우 Dictionary에 해당 Key를 추가합니다.
    /// </summary>
    /// <param name="type">Dictionary 내에서 확인해야 하는 재화 종류</param>
    public void CheckDictionary(MoneyType type)
    {
        if(!_money.ContainsKey(type))
        {
            _money.Add(type, 0);
        }
    }

    public MoneyType GetShardTargetGremlin(int id)
    {
        string name = DataManager.Instance.GetData<G_StoneData>(id).G_Stone_Name;

        MoneyType money = new MoneyType();

        switch (name)
        {
            case "플린트":
                CheckDictionary(MoneyType.FlintPiece);
                money = MoneyType.FlintPiece;
                break;
            case "피니언":
                CheckDictionary(MoneyType.PinionPiece);
                money = MoneyType.PinionPiece;
                break;
            case "코어":
                CheckDictionary(MoneyType.CorePiece);
                money = MoneyType.CorePiece;
                break;
            case "징크":
                CheckDictionary(MoneyType.ZincPiece);
                money = MoneyType.ZincPiece;
                break;
        }

        return money;
    }

    /// <summary>
    /// 그렘린 조각 획득 시의 메서드입니다.
    /// </summary>
    /// <param name="id">그렘린 조각 id값</param>
    /// <param name="amount">획득 수량</param>
    public void AddGremlinPiece(int id, int amount)
    {
        _money[GetShardTargetGremlin(id)] += amount;
    }

    /// <summary>
    /// 그렘린 조각 사용 시 재화 차감용 메서드입니다.
    /// </summary>
    /// <param name="id">그렘린 조각 id값</param>
    /// <param name="amount">사용 수량</param>
    public bool RemoveGremlinPiece(int id, int amount)
    {
        bool success = UseMoney(GetShardTargetGremlin(id), amount);
        return success;
    }
}
