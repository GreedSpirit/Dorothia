using UnityEngine;

public class OverdriveOrb : MonoBehaviour
{
    private MonsterSpawnManager _owner;

    public void SetOwner(MonsterSpawnManager owner)
    {
        _owner = owner;
    }

    public void Collect()
    {
        if (_owner != null)
            _owner.ReleaseOrb(this);
    }
}
