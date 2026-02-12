using UnityEngine;

public abstract class MonsterAttackBase : MonoBehaviour
{
    protected MonsterController _owner;

    public void Bind(MonsterController owner)
    {
        _owner = owner;
    }

    /// <summary>
    /// 공격 실행(사거리, 쿨다운 체크는 컨트롤러에서)
    /// </summary>
    /// <param name="target"></param>
    public abstract void PerformAttack(IMonsterTarget target);
}
