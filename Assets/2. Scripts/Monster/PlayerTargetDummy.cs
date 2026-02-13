using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTargetDummy : MonoBehaviour, IMonsterTarget
{
    [SerializeField] private int _hp = 2097152;
    [SerializeField] private float _attackRange = 8f;
    [SerializeField] private int _damagePerSecond = 100;
    [SerializeField] private LayerMask _monsterLayer;

    private readonly Collider[] _hitBuffer = new Collider[32];

    public Transform Transform => transform;
    public bool IsAlive => _hp > 0;

    private void Update()
    {
        AutoDamage();
    }

    private void AutoDamage()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            _attackRange,
            _hitBuffer,
            _monsterLayer);

        float damageThisFrame = _damagePerSecond * Time.deltaTime;

        for (int i = 0; i < count; i++)
        {
            IMonster monster = _hitBuffer[i].GetComponent<IMonster>();
            if (monster != null)
            {
                monster.TakeDamage(Mathf.RoundToInt(damageThisFrame));
            }
        }
    }

    public void ApplyDamage(int amount)
    {
        _hp -= amount;
        Debug.Log($"Damaged: {amount}, HP: {_hp}");
    }
}
