using System.Collections.Generic;
using UnityEngine;

public enum MoneyType
{
    Scrap, Gold, GremlinPiece
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
        return _money[type];
    }

    //재화획득시 호출할 함수
    public void GetMoney(MoneyType type, int amount)
    {
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
        PlayerPrefs.SetInt("GremlinPiece", _money[MoneyType.GremlinPiece]);

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
}
