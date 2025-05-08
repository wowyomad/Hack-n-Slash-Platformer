using GameActions;
using Stateless.Graph;
using TheGame;
using UnityEngine;
public class PlayerWalkState : PlayerBaseState, IPlayerVulnarableState
{
    private float m_VelocitySmoothing = 0.0f;
    public PlayerWalkState(Player player) : base(player) { }

    public override void OnEnter()
    {
        m_VelocitySmoothing = 0.0f;
        Player.Animator.CrossFade(WalkAnimationHash, 0.0f);
    }

    public override void OnUpdate()
    {
        float moveInput = Player.Input.HorizontalMovement;

        Player.Flip(moveInput);

        //Abomination
        if (Mathf.Abs(Controller.Velocity.x) <= 3.0f)
        {
            Player.Animator.CrossFade(IdleAnimationHash, 0.0f);
        }
        else
        {
            Player.Animator.CrossFade(WalkAnimationHash, 0.0f);
        }

        float targetVelocityX = moveInput * Player.Movement.HorizontalSpeed;
        if (Controller.Collisions.Right)
        {
            targetVelocityX = Mathf.Min(targetVelocityX, 0);
        }
        else if (Controller.Collisions.Left)
        {
            targetVelocityX = Mathf.Max(targetVelocityX, 0);
        }

        Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeGrounded);
    }
}