using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerAirState : PlayerBaseState
{
    private float m_VelocitySmoothing = 0.0f;
    public PlayerAirState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Player.Animator.CrossFade(AirAnimationHash, 0.0f);
        Input.Move += OnMove;
        Input.Throw += OnThrow;
    }

    public override void OnExit()
    {
        Input.Throw -= OnThrow;
        Input.Move -= OnMove;
    }

    public override void Update()
    {
        if (Player.Controller.IsGrounded)
        {
            Player.ChangeState(Player.IdleState); return;
        }

        Player.ApplyGravity();

        float input = Input.HorizontalMovement;

        float targetVelocityX = Input.HorizontalMovement * Player.Movement.HorizontalSpeed;
        if (Controller.Collisions.Right)
        {
            targetVelocityX = Mathf.Min(targetVelocityX, 0);
        }
        else if (Controller.Collisions.Left)
        {
            targetVelocityX = Mathf.Max(targetVelocityX, 0);
        }
        Player.Velocity.x = Mathf.SmoothDamp(Player.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeAirborne);
    }

    public void Move(float direction)
    {

    }

    public void OnThrow()
    {
        Player.ThrowFirebottle();
    }

    public void OnMove(float direction)
    {
        Player.Flip(direction);
    }
}