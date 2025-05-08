using UnityEngine;
using TheGame;

public class PlayerAirState : PlayerBaseState, IPlayerVulnarableState
{
    private float m_VelocitySmoothing = 0.0f;

    public PlayerAirState(Player player) : base(player) { }

    public override void OnEnter()
    {
        Player.Animator.CrossFade(AirAnimationHash, 0.0f);
        m_VelocitySmoothing = 0f;
    }

    public override void OnUpdate()
    {
        float moveInput = Player.Input.HorizontalMovement;

        Player.Flip(moveInput);
        

        float targetVelocityX = moveInput * Player.Movement.HorizontalSpeed;

        if (Controller.Collisions.Right && targetVelocityX > 0)
        {
            targetVelocityX = 0;
        }
        else if (Controller.Collisions.Left && targetVelocityX < 0)
        {
            targetVelocityX = 0;
        }
        
        Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeAirborne);
    }
}