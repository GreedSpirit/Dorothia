
using System;
[Serializable]
public class PassiveSkill : BaseSkill
{
    public override void Execute()
    {
        //플레이어 최종 스탯에 패시브 값 적용
        //StatManager.Instance.RefreshStats();
    }

    public override void Undo()
    {
        //StatManager.Instance.RefreshStats();
    }
}
