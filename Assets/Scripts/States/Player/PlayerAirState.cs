using UnityEngine;

public class PlayerAirState : PlayerBaseState
{
    public PlayerAirState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Player.Animator.CrossFade(AirAnimationHash, 0.0f);
    }
}