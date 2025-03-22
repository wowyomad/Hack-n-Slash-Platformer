using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{

    public float JumpVelocity;
    public float JumpDecceleration;

    public PlayerJumpState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Player.Animator.CrossFade(JumpAnimationHash, 0.0f);
    }
}

