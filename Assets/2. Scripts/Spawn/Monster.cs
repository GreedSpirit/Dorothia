using UnityEngine;

public class Monster : MonoBehaviour
{
    private MonsterSpawnManager _owner;

    public void Initialize(MonsterSpawnManager spawnManager)
    {
        _owner = spawnManager;

        //추후 NavMesh
    }

    public void Despawn()
    {
        _owner.ReleaseMonster(this);
    }
}
