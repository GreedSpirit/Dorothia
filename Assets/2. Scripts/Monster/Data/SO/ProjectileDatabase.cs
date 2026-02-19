using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProjectileDatabase", menuName = "Scriptable Objects/ProjectileDatabase")]
public class ProjectileDatabase : ScriptableObject
{
    public List<ProjectileData> list;

    private Dictionary<int, ProjectileData> _dict;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        _dict = new Dictionary<int, ProjectileData>();

        foreach (var data in list)
        {
            if (data == null)
                continue;

            if (!_dict.ContainsKey(data.projectileId))
                _dict.Add(data.projectileId, data);
        }
    }

    public ProjectileData Get(int id)
    {
        if (_dict == null || _dict.Count == 0)
            Initialize();

        _dict.TryGetValue(id, out var data);
        return data;
    }
}
