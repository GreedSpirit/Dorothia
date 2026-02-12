using UnityEngine;

public class PlayerIdleState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player)
    {
        player.Anima.SetBool("Run", false);
    }

    public void Execute(PlayerCtrl player)
    {
        
    }

    public void Exit(PlayerCtrl player)
    {
        
    }
}
