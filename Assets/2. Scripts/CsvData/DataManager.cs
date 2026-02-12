using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public Dictionary<int, TempData> TempDict { get; private set; }
    public Dictionary<string, TempData> TempKeyDict { get; private set;}

    [Header("Skill")]
    public Dictionary<int, SkillData> SkillDict { get; private set; }
    public Dictionary<string, SkillData> SkillKeyDict { get; private set;}
    public Dictionary<int, Skill_RankData> Skill_RankDict { get; private set; }
    public Dictionary<string, Skill_RankData> Skill_RankKeyDict { get; private set;}
    public Dictionary<int, Skill_StatusData> Skill_StatusDict { get; private set; }
    public Dictionary<string, Skill_StatusData> Skill_StatusKeyDict { get; private set;}
    public Dictionary<int, Skill_UpgradeData> Skill_UpgradeDict { get; private set; }
    public Dictionary<string, Skill_UpgradeData> Skill_UpgradeKeyDict { get; private set;}

    [Header("Equip")]
    public Dictionary<int, EquipData> EquipDict { get; private set; }
    public Dictionary<string, EquipData> EquipKeyDict { get; private set;}
    public Dictionary<int, Equip_BreakData> Equip_BreakDict { get; private set; }
    public Dictionary<string, Equip_BreakData> Equip_BreakKeyDict { get; private set;}
    public Dictionary<int, Equip_LevelData> Equip_LevelDict { get; private set; }
    public Dictionary<string, Equip_LevelData> Equip_LevelKeyDict { get; private set;}
    public Dictionary<int, Equip_RankData> Equip_RankDict { get; private set; }
    public Dictionary<string, Equip_RankData> Equip_RankKeyDict { get; private set;}
    public Dictionary<int, Equip_SetData> Equip_SetDict { get; private set; }
    public Dictionary<string, Equip_SetData> Equip_SetKeyDict { get; private set;}
    public Dictionary<int, Equip_StatusData> Equip_StatusDict { get; private set; }
    public Dictionary<string, Equip_StatusData> Equip_StatusKeyDict { get; private set;}
    public Dictionary<int, Equip_Upgrade_GoldData> Equip_Upgrade_GoldDict { get; private set; }
    public Dictionary<string, Equip_Upgrade_GoldData> Equip_Upgrade_GoldKeyDict { get; private set;}
    public Dictionary<int, Equip_UpgradeData> Equip_UpgradeDict { get; private set; }
    public Dictionary<string, Equip_UpgradeData> Equip_UpgradeKeyDict { get; private set;}


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
        TempDict = LoadAndCreateDict(CSVReader.Read<TempData>("Temp"), out Dictionary<string, TempData> tempTempKeyDict);
        TempKeyDict = tempTempKeyDict;

        // Skill Data Load
        SkillDict = LoadAndCreateDict(CSVReader.Read<SkillData>("Skill"), out Dictionary<string, SkillData> tempSkillKeyDict);
        SkillKeyDict = tempSkillKeyDict;

        Skill_RankDict = LoadAndCreateDict(CSVReader.Read<Skill_RankData>("Skill_Rank"), out Dictionary<string, Skill_RankData> tempSkill_RankKeyDict);
        Skill_RankKeyDict = tempSkill_RankKeyDict;

        Skill_StatusDict = LoadAndCreateDict(CSVReader.Read<Skill_StatusData>("Skill_Status"), out Dictionary<string, Skill_StatusData> tempSkill_StatusKeyDict);
        Skill_StatusKeyDict = tempSkill_StatusKeyDict;

        Skill_UpgradeDict = LoadAndCreateDict(CSVReader.Read<Skill_UpgradeData>("Skill_Upgrade"), out Dictionary<string, Skill_UpgradeData> tempSkill_UpgradeKeyDict);
        Skill_UpgradeKeyDict = tempSkill_UpgradeKeyDict;    

        // Equip Data Load
        EquipDict = LoadAndCreateDict(CSVReader.Read<EquipData>("Equip"), out Dictionary<string, EquipData> tempEquipKeyDict);
        EquipKeyDict = tempEquipKeyDict;

        Equip_BreakDict = LoadAndCreateDict(CSVReader.Read<Equip_BreakData>("Equip_Break"), out Dictionary<string, Equip_BreakData> tempEquip_BreakKeyDict);
        Equip_BreakKeyDict = tempEquip_BreakKeyDict;

        Equip_LevelDict = LoadAndCreateDict(CSVReader.Read<Equip_LevelData>("Equip_Level"), out Dictionary<string, Equip_LevelData> tempEquip_LevelKeyDict);
        Equip_LevelKeyDict = tempEquip_LevelKeyDict;

        Equip_RankDict = LoadAndCreateDict(CSVReader.Read<Equip_RankData>("Equip_Rank"), out Dictionary<string, Equip_RankData> tempEquip_RankKeyDict);
        Equip_RankKeyDict = tempEquip_RankKeyDict;

        Equip_SetDict = LoadAndCreateDict(CSVReader.Read<Equip_SetData>("Equip_Set"), out Dictionary<string, Equip_SetData> tempEquip_SetKeyDict);
        Equip_SetKeyDict = tempEquip_SetKeyDict;

        Equip_StatusDict = LoadAndCreateDict(CSVReader.Read<Equip_StatusData>("Equip_Status"), out Dictionary<string, Equip_StatusData> tempEquip_StatusKeyDict);
        Equip_StatusKeyDict = tempEquip_StatusKeyDict;

        Equip_Upgrade_GoldDict = LoadAndCreateDict(CSVReader.Read<Equip_Upgrade_GoldData>("Equip_Upgrade_Gold"), out Dictionary<string, Equip_Upgrade_GoldData> tempEquip_Upgrade_GoldKeyDict);
        Equip_Upgrade_GoldKeyDict = tempEquip_Upgrade_GoldKeyDict;

        Equip_UpgradeDict = LoadAndCreateDict(CSVReader.Read<Equip_UpgradeData>("Equip_Upgrade"), out Dictionary<string, Equip_UpgradeData> tempEquip_UpgradeKeyDict);
        Equip_UpgradeKeyDict = tempEquip_UpgradeKeyDict;
    }

    private Dictionary<int, T> LoadAndCreateDict<T>(List<T> list, out Dictionary<string, T> keyDict) where T : ICSVLoad, ITableKey
    {
        Dictionary<int, T> idDict = new Dictionary<int, T>();
        keyDict = new Dictionary<string, T>();

        foreach(T data in list)
        {
            // ID 딕셔너리
            if (!idDict.ContainsKey(data.Id))
            {
                idDict.Add(data.Id, data);
            }
            else
            {
                Debug.LogWarning($"ID 중복 발생 (무시됨): {typeof(T).Name} 테이블 - ID {data.Id}");
            }

            // Key 딕셔너리
            if (!string.IsNullOrEmpty(data.Key) && !keyDict.ContainsKey(data.Key))
            {
                keyDict.Add(data.Key, data);
            }
            else if (!string.IsNullOrEmpty(data.Key))
            {
                Debug.LogWarning($"Key 중복 발생 (무시됨): {typeof(T).Name} 테이블 - Key {data.Key}");
            }
        }

        return idDict;
    }

    // 데이터 접근

    public TempData GetTemp(int id) => TempDict.TryGetValue(id, out var data) ? data : null;
    public TempData GetTemp(string key) => TempKeyDict.TryGetValue(key, out var data) ? data : null;

    public SkillData GetSkill(int id) => SkillDict.TryGetValue(id, out var data) ? data : null;
    public SkillData GetSkill(string key) => SkillKeyDict.TryGetValue(key, out var data) ? data : null;
    public Skill_RankData GetSkill_Rank(int id) => Skill_RankDict.TryGetValue(id, out var data) ? data : null;
    public Skill_RankData GetSkill_Rank(string key) => Skill_RankKeyDict.TryGetValue(key, out var data) ? data : null;
    public Skill_StatusData GetSkill_Status(int id) => Skill_StatusDict.TryGetValue(id, out var data) ? data : null;
    public Skill_StatusData GetSkill_Status(string key) => Skill_StatusKeyDict.TryGetValue(key, out var data) ? data : null;
    public Skill_UpgradeData GetSkill_Upgrade(int id) => Skill_UpgradeDict.TryGetValue(id, out var data) ? data : null;
    public Skill_UpgradeData GetSkill_Upgrade(string key) => Skill_UpgradeKeyDict.TryGetValue(key, out var data) ? data : null;

    public EquipData GetEquip(int id) => EquipDict.TryGetValue(id, out var data) ? data : null;
    public EquipData GetEquip(string key) => EquipKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_BreakData GetEquip_Break(int id) => Equip_BreakDict.TryGetValue(id, out var data) ? data : null;
    public Equip_BreakData GetEquip_Break(string key) => Equip_BreakKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_LevelData GetEquip_Level(int id) => Equip_LevelDict.TryGetValue(id, out var data) ? data : null;
    public Equip_LevelData GetEquip_Level(string key) => Equip_LevelKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_RankData GetEquip_Rank(int id) => Equip_RankDict.TryGetValue(id, out var data) ? data : null;
    public Equip_RankData GetEquip_Rank(string key) => Equip_RankKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_SetData GetEquip_Set(int id) => Equip_SetDict.TryGetValue(id, out var data) ? data : null;
    public Equip_SetData GetEquip_Set(string key) => Equip_SetKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_StatusData GetEquip_Status(int id) => Equip_StatusDict.TryGetValue(id, out var data) ? data : null;
    public Equip_StatusData GetEquip_Status(string key) => Equip_StatusKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_Upgrade_GoldData GetEquip_Upgrade_Gold(int id) => Equip_Upgrade_GoldDict.TryGetValue(id, out var data) ? data : null;
    public Equip_Upgrade_GoldData GetEquip_Upgrade_Gold(string key) => Equip_Upgrade_GoldKeyDict.TryGetValue(key, out var data) ? data : null;
    public Equip_UpgradeData GetEquip_Upgrade(int id) => Equip_UpgradeDict.TryGetValue(id, out var data) ? data : null;
    public Equip_UpgradeData GetEquip_Upgrade(string key) => Equip_UpgradeKeyDict.TryGetValue(key, out var data) ? data : null;

}
    
