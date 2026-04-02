using System;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;

public enum MoneyType
{
    Scrap, Gold, CorePiece, FlintPiece, PinionPiece, ZincPiece
}
public class ExchangeManager : MonoBehaviour
{
    //싱글톤
    public static ExchangeManager Instance { get; private set; }

    //저장해둘 재화
    Dictionary<MoneyType, BigInteger> _money = new();

    // 골드
    [SerializeField] private TextMeshProUGUI _gold;

    public Action onShardValueChanged;
    int currentGremlinShard;

    // 골드 재회 획득 시, 이벤트 연결
    public event Action<BigInteger> OnGoldChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //재화불러오기
            LoadMoney();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //잔고 조회
    public BigInteger GetMoneyAmount(MoneyType type)
    {
        CheckDictionary(type);
        return _money[type];
    }

    //재화획득시 호출할 함수
    public void GetMoney(MoneyType type, BigInteger amount)
    {
        CheckDictionary(type);
        _money[type] += amount;

        UpdateGoods(type);

        SaveMoney();
    }

    //살수있는지 없는지 불값반환
    public bool UseMoney(MoneyType type, BigInteger amount)
    {
        if (_money[type] < amount)
            return false;

        _money[type] -= amount;

        UpdateGoods(type);
        SaveMoney();

        return true;
    }

    // 재화 변동 시
    private void UpdateGoods(MoneyType type) // JSONUtility 사용을 위해 딕셔너리 -> 리스트 변경
    {
        if (type == MoneyType.Gold)
        {
            _gold.text = _money[type].ToString("N0");
            OnGoldChanged?.Invoke(_money[type]);
        }
    }

    #region JSON SAVE DATA 구조
    //JsonUtility 직렬화를 위해 Dictionary 대신 List 구조 사용
    [Serializable]
    public class MoneyData
    {
        public string type;     // enum을 string으로 저장
        public string value;    // BigInteger를 string으로 저장
    }

    [Serializable]
    public class ExchangeSaveData
    {
        public List<MoneyData> moneyList = new();
    }

    public void SaveMoney()
    {
        ExchangeSaveData data = new ExchangeSaveData();

        foreach (var pair in _money)
        {
            data.moneyList.Add(new MoneyData
            {
                type = pair.Key.ToString(),
                value = pair.Value.ToString()
            });
        }

        SaveUtility.SaveEncrypted("ExchangeData", data);
    }

    public void LoadMoney()
    {
        var data = SaveUtility.LoadEncrypted<ExchangeSaveData>("ExchangeData");

        if (data == null) return;

        _money.Clear();

        //string → enum, string → BigInteger 변환
        foreach (var item in data.moneyList)
        {
            MoneyType type = (MoneyType)Enum.Parse(typeof(MoneyType), item.type);
            BigInteger value = BigInteger.Parse(item.value);

            _money[type] = value;
        }

        UpdateGoods(MoneyType.Gold);
    }

    public void CheckDictionary(MoneyType type)
    {
        if (!_money.ContainsKey(type))
            _money.Add(type, BigInteger.Zero);
    }
    #endregion

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
    public void AddGremlinPiece(int id, BigInteger amount)
    {
        _money[GetShardTargetGremlin(id)] += amount;
        onShardValueChanged?.Invoke();
    }

    public int GetCurrentShardID()
    {
        return currentGremlinShard;
    }

    public void SetCurrentShardID(int id)
    {
        currentGremlinShard = id;
    }

    public void GetCoreGremlinShard(BigInteger amount)
    {
        AddGremlinPiece(210003, amount);
    }

    /// <summary>
    /// 그렘린 조각 사용 시 재화 차감용 메서드입니다.
    /// </summary>
    /// <param name="id">그렘린 조각 id값</param>
    /// <param name="amount">사용 수량</param>
    public bool RemoveGremlinPiece(int id, BigInteger amount)
    {
        bool success = UseMoney(GetShardTargetGremlin(id), amount);
        return success;
    }
}
