using UnityEngine;
using System;

public interface IMonsterTarget
{
    Transform Transform { get; }
    bool IsAlive { get; }
    void ApplyDamage(int amount);

    event Action OnDead;
}
