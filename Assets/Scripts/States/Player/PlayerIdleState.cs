using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{

    protected float HorizontalInput => Player.Input.HorizontalMovement;
    private float m_VelocitySmoothing;
    public PlayerIdleState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Player.Animator.CrossFade(IdleAnimationHash, 0.0f);
        Player.Input.Jump += OnJump;
        Player.Input.Move += OnMove;
        Player.Velocity.y = 0.0f; //TODO: Be better
    }

    public override void OnExit()
    {
        Player.Input.Jump -= OnJump;
        Player.Input.Move -= OnMove;
    }
    public override void Update()
    {
        if (!Player.Controller.IsGrounded)
        {
            Player.ChangeState(Player.AirState); return;
        }
        else if (HorizontalInput > 0 && !Controller.IsFacingWallRight || HorizontalInput < 0 && !Controller.IsFacingWallLeft)
        {
            Player.ChangeState(Player.WalkState); return;
        }

        if (Mathf.Abs(Player.Velocity.x) > 0.0f)
        {
            Player.Velocity.x = Mathf.SmoothDamp(
                Player.Velocity.x,
                0f,
                ref m_VelocitySmoothing,
                Player.Movement.DecelerationTimeGrounded
            );

            if (Mathf.Abs(Player.Velocity.x) < 0.01f)
            {
                Player.Velocity.x = 0f;
                m_VelocitySmoothing = 0f;
            }
        }
    }
    private void OnMove(float direction)
    {
        Player.Flip((int)Mathf.Sign(direction));
    }
    protected void OnJump()
    {
        ChangeState(Player.JumpState); return;
    }
}
