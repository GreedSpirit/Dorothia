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
}
