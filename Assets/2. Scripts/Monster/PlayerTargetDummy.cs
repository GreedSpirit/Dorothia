using UnityEngine;

public class PlayerTargetDummy : MonoBehaviour, IMonsterTarget
{
    [SerializeField] private int _hp = 2097152;

    public Transform Transform => transform;
    public bool IsAlive => _hp > 0;

    public void ApplyDamage(int amount)
    {
        _hp -= amount;
        Debug.Log($"Damaged: {amount}, HP: {_hp}");
    }
}
