using System.Collections.Generic;
using UnityEngine;

public class ListTable<T> : ITable where T : ICSVLoad, ITableKey, new()
{
    private Dictionary<int, List<T>> _idDict = new Dictionary<int, List<T>>();
    private Dictionary<string, List<T>> _keyDict = new Dictionary<string, List<T>>();
    public void Load(string filename)
    {
        _idDict.Clear();
        List<T> list = CSVReader.Read<T>(filename);

        foreach(var item in list)
        {
            if (!_idDict.ContainsKey(item.Id))
            {
                _idDict.Add(item.Id, new List<T>());
            }
            _idDict[item.Id].Add(item);

            if (!string.IsNullOrEmpty(item.Key) && !_keyDict.ContainsKey(item.Key))
            {
                _keyDict.Add(item.Key, new List<T>());
            }
            _keyDict[item.Key].Add(item);
        }
    }

    public List<T> Get(int id) => _idDict.TryGetValue(id, out var v) ? v : null;
    public List<T> Get(string key) => _keyDict.TryGetValue(key, out var v) ? v : null;
    public Dictionary<int, List<T>> GetListDict() => _idDict;
    public Dictionary<string, List<T>> GetListKeyDict() => _keyDict;
}
