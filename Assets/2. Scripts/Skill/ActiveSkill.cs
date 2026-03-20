using UnityEngine;

public class ActiveSkill : BaseSkill
{
    public override void Execute(PlayerCtrl owner = null)
    {
        if (!IsReady) return;

        Debug.Log($"{Data.Skill_Name} 시전!");

        StartCooldown();
    }

}
