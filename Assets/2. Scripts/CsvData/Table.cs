using System.Collections.Generic;
using UnityEngine;

public class Table<T> : ITable where T : ICSVLoad, ITableKey, new()
{
    public Dictionary<int, T> _idDict = new Dictionary<int, T>();
    public Dictionary<string, T> _keyDict = new Dictionary<string, T>();

    public void Load(string filename)
    {
        _idDict.Clear();
        _keyDict.Clear();

        List<T> list = CSVReader.Read<T>(filename);

        foreach(T data in list)
        {
            if (!_idDict.ContainsKey(data.Id)) _idDict.Add(data.Id, data);
            if (!string.IsNullOrEmpty(data.Key) && !_keyDict.ContainsKey(data.Key)) _keyDict.Add(data.Key, data);
        }
    }

    public T Get(int id) => _idDict.TryGetValue(id, out var data) ? data : default;
    public T Get(string key) => _keyDict.TryGetValue(key, out var data) ? data : default;

    public Dictionary<int, T> GetDict() => _idDict;
    public Dictionary<string, T> GetKeyDict() => _keyDict;
}
