using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Player.Animator.CrossFade(IdleAnimationHash, 0.0f);
    }
    public override void Update()
    {
       
    }
}
