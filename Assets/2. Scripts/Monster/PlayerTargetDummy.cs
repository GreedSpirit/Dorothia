using UnityEngine;

public class PlayerTargetDummy : MonoBehaviour, IMonsterTarget
{
    [SerializeField] private int _hp = 2097152;

    public Transform Transform => transform;
    public bool IsAlive => _hp > 0;

    public void ApplyDamage(int amout)
    {
        _hp -= amout;
        Debug.Log($"Damaged: {amout}, HP: {_hp}");
    }
}
