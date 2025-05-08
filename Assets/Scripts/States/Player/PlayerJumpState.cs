using TheGame;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState, IPlayerVulnarableState
{
    protected float m_VelocitySmoothing = 0.0f;
    public float JumpVelocity => Player.Movement.JumpVelocity;

    public PlayerJumpState(Player player) : base(player) { }
    
    public override void Enter(IState from)
    {
        Player.Animator.CrossFade(JumpAnimationHash, 0.0f);
        Controller.Velocity.y = JumpVelocity;
    }
    public override void Update()
    {
        float moveInput = Player.Input.HorizontalMovement;
        
        Player.Flip(moveInput);
        

        float targetVelocityX = moveInput * Player.Movement.HorizontalSpeed;
        Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeAirborne);

    }
}

