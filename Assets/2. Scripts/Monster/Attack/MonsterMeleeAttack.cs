using UnityEngine;

public class MonsterMeleeAttack : MonsterAttackBase
{
    public override void PerformAttack(IMonsterTarget target)
    {
        if (target == null || !target.IsAlive)
            return;

        target.ApplyDamage(_owner.Stats.Damage);
    }
}
