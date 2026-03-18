using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    private Dictionary<Type, ITable> _tables = new Dictionary<Type, ITable>();
    private Dictionary<int, int> _dungeonClearCounts = new Dictionary<int, int>();
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
    //UI에서 패널 열릴 때 호출 퍼클체크
    public int GetUsedEntryCount(int dungeonId)
    {
        return _dungeonClearCounts.TryGetValue(dungeonId, out int count) ? count : 0;
    }

    //입장 가능 여부
    public bool CanEnterDungeon(int dungeonId, int maxEntry)
    {
        return GetUsedEntryCount(dungeonId) < maxEntry;
    }

    //횟수 차감
    public bool TryConsumeEntry(int dungeonId, int maxEntry, out int usedCount)
    {
        usedCount = GetUsedEntryCount(dungeonId);

        if (usedCount >= maxEntry)
            return false;

        usedCount++;
        _dungeonClearCounts[dungeonId] = usedCount;
        return true;
    }


    private void LoadAllData()
    {
        _tables.Clear();
        // 1:1 데이터 로드 => LoadData 사용
        LoadData<TempData>("Temp");
        
        //Skill 테이블
        LoadData<SkillData>("Skill");
        LoadData<Skill_RankData>("Skill_Rank");
        LoadData<Skill_StatusData>("Skill_Status");
        LoadData<Skill_UpgradeData>("Skill_Upgrade");
        LoadData<Skill_Upgrade_GoldData>("Skill_Upgrade_Gold");

        //Equip 테이블
        LoadData<EquipData>("Equip");
        LoadData<Equip_BreakData>("Equip_Break");
        LoadData<Equip_LevelData>("Equip_level");
        LoadData<Equip_RankData>("Equip_Rank");
        LoadData<Equip_UpgradeData>("Equip_Upgrade");
        LoadData<Equip_Upgrade_GoldData>("Equip_Upgrade_Gold");
        LoadData<Equip_Rank_GoldData>("Equip_Rank_Gold");

        //Character 테이블
        LoadData<CharacterData>("Character");
        LoadData<Character_RankData>("Character_Rank");
        LoadData<Character_StatsData>("Character_Stats");
        LoadData<Character_UpgradeData>("Character_Upgrade");

        //Gremlin 테이블
        LoadData<GremlinData>("Gremlin");
        LoadData<Gremlin_TierData>("Gremlin_Tier");
        LoadData<Gremlin_AtkerData>("Gremlin_Atker");
        LoadData<Gremlin_BufferData>("Gremlin_Buffer");
        LoadData<Gremlin_UpgradeData>("Gremlin_Upgrade");
        LoadListData<Gremlin_StatusData>("Gremlin_Status");

        //Monster 테이블
        LoadData<Monster_Data>("Monster");
        LoadData<Monster_SpawnData>("Monster_Spawn");
        LoadData<Monster_ValueData>("Monster_Value");
        LoadListData<Monster_GroupData>("Monster_Group");

        //Stage 테이블
        LoadData<StageData>("Stage");
        LoadData<Stage_RewardData>("Stage_Reward");
        LoadData<Stage_SectionData>("Stage_Section");
        LoadData<Equip_Drop_RankData>("Equip_Drop_Rank");

        //Dungeon 테이블
        LoadData<DungeonData>("Dungeon");
        LoadData<Dungeon_StepData>("Dungeon_Step");
        LoadData<Dungeon_RewardData>("Dungeon_Reward");
        LoadData<ConsumData>("Consum");
        LoadData<Sk_SclData>("Sk_Scl");
        LoadData<G_StoneData>("G_Stone");
        
        // 1:N 데이터 로드 => LoadListData 사용
        LoadListData<Equip_SetData>("Equip_Set");
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
