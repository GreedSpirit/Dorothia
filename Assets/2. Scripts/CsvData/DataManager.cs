using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private Dictionary<Type, ITable> _tables = new Dictionary<Type, ITable>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllData();
    }

    private void LoadAllData()
    {
        _tables.Clear();
        // 1:1 데이터 로드 => LoadData 사용
        LoadData<TempData>("Temp");

        LoadData<SkillData>("Skill");
        LoadData<Skill_RankData>("Skill_Rank");
        LoadData<Skill_StatusData>("Skill_Status");
        LoadData<Skill_UpgradeData>("Skill_Upgrade");
        LoadData<Skill_Upgrade_GoldData>("Skill_Upgrade_Gold");

        LoadData<EquipData>("Equip");
        LoadData<Equip_BreakData>("Equip_Break");
        LoadData<Equip_LevelData>("Equip_level");
        LoadData<Equip_RankData>("Equip_Rank");
        LoadData<Equip_UpgradeData>("Equip_Upgrade");
        
        // 1:N 데이터 로드 => LoadListData 사용
        LoadListData<Equip_SetData>("Equip_Set");
        LoadListData<Equip_Upgrade_GoldData>("Equip_Upgrade_Gold");
    }

    private void LoadData<T>(string fileName) where T : ICSVLoad, ITableKey, new()
    {
        Table<T> table = new Table<T>();
        table.Load(fileName);

        _tables[typeof(T)] = table;// 특정 타입을 키로 저장해서 이용한다는 아이디어
    }

    private void LoadListData<T>(string fileName) where T : ICSVLoad, ITableKey, new()
    {
        ListTable<T> table = new ListTable<T>();
        table.Load(fileName);

        _tables[typeof(T)] = table;
    }

    // 여기부터 다른 곳에서 사용할 public 함수들 정의

    public T GetData<T>(int id) where T : class, ICSVLoad, ITableKey, new()
    {
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            if (table is Table<T> t) return t.Get(id);
        }
        return null;
    }

    public T GetData<T>(string key) where T : class, ICSVLoad, ITableKey, new()
    {
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            if (table is Table<T> t) return t.Get(key);
        }
        return null;
    }

    public List<T> GetList<T>(int id) where T : class, ICSVLoad, ITableKey, new()
    {
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            if (table is ListTable<T> t) return t.Get(id);
        }
        return null;
    }
    public List<T> GetList<T>(string key) where T : class, ICSVLoad, ITableKey, new()
    {
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            if (table is ListTable<T> t) return t.Get(key);
        }
        return null;
    }

    public Dictionary<int, T> GetDict<T>() where T : class, ICSVLoad, ITableKey, new()
    {
        // 해당 타입의 테이블이 있는지 확인
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            // Table<T>로 캐스팅해서 딕셔너리 리턴
            if (table is Table<T> t)
            {
                return t.GetDict();
            }
        }
        Debug.LogError($"[DataManager] {typeof(T).Name} 테이블을 찾을 수 없거나 문제가 있습니다.");
        return null;
    }

    public Dictionary<string, T> GetKeyDict<T>() where T : class, ICSVLoad, ITableKey, new()
    {
        // 해당 타입의 테이블이 있는지 확인
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            // Table<T>로 캐스팅해서 딕셔너리 리턴
            if (table is Table<T> t)
            {
                return t.GetKeyDict();
            }
        }
        Debug.LogError($"[DataManager] {typeof(T).Name} 테이블을 찾을 수 없거나 문제가 있습니다.");
        return null;
    }

    // 1:N 테이블의 해당 타입 딕셔너리 가져오기
    public Dictionary<int, List<T>> GetListDict<T>() where T : class, ICSVLoad, ITableKey, new()
    {
        if (_tables.TryGetValue(typeof(T), out ITable table))
        {
            if (table is ListTable<T> t)
            {
                return t.GetListDict();
            }
        }
        return null;
    }
}
