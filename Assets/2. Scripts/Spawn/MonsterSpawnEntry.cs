using UnityEngine;

[System.Serializable]
public class MonsterSpawnEntry
{
    public MonsterController prefab;
    [Range(0f, 1f)] public float weight = 1f;
}
