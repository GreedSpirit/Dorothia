using UnityEngine;

public class PlayerIdleState : IPlayerState<PlayerCtrl>
{
    public void Enter(PlayerCtrl player)
    {
        player.NavMesh.ResetPath();
        player.Anima.SetBool("Run", false);
    }

    public void Execute(PlayerCtrl player)
    {
        
    }

    public void Exit(PlayerCtrl player)
    {
        
    }
}
