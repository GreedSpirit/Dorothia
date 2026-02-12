using UnityEngine;

public interface IMonsterTarget
{
    Transform Transform { get; }
    bool IsAlive { get; }
    void ApplyDamage(int amount);
}
