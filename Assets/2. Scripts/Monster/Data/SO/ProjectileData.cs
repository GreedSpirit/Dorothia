using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject
{
    public int projectileId; // CSV와 연결용 ID
    public GameObject prefab;

    public float speed = 12f;
    public float lifeTime = 3f;
}
