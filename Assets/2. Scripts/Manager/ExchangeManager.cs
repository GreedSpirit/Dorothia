using System.Collections.Generic;
using UnityEngine;

public enum MoneyType
{
    Scrap, Gold
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

    /*
     * 재화불러오기 예시
    void LoadMoney()
    {
      Savedata data = SaveManager.Instance.Load();
      _money = data.gold;
    }
    */
}
