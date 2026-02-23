using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTargetDummy : MonoBehaviour, IMonsterTarget
{
    [SerializeField] private int _hp = 2097152;
    [SerializeField] private float _attackRange = 8f;
    [SerializeField] private int _damagePerSecond;
    [SerializeField] private LayerMask _monsterLayer;

    private readonly Collider[] _hitBuffer = new Collider[32];

    private float _damageAccumulator; // DPS 누적 버퍼

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

        if (count <= 0)
            return;

        //초당 데미지를 프레임 단위로 변환
        float damageThisFrame = _damagePerSecond * Time.deltaTime;

        //누적
        _damageAccumulator += damageThisFrame;

        //실제 적용 가능한 정수 데미지
        int finalDamage = Mathf.FloorToInt(_damageAccumulator);

        //적용한 만큼 차감
        _damageAccumulator -= finalDamage;

        //범위 내 모든 몬스터들한테 동일 데미지 적용
        for (int i = 0; i < count; i++)
        {
            IMonster monster = _hitBuffer[i].GetComponent<IMonster>();
            if (monster != null)
            {
                monster.TakeDamage(finalDamage);
            }
        }
    }

    public void ApplyDamage(int amount)
    {
        _hp -= amount;
        Debug.Log($"Damaged: {amount}, HP: {_hp}");
    }
}
