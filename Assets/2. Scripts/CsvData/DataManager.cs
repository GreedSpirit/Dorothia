using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;

    public Dictionary<int, TempData> TempDict { get; private set; }
    public Dictionary<string, TempData> TempKeyDict { get; private set;}


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
}
