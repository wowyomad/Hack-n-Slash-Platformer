using GameActions;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{

    protected float HorizontalInput => Player.Input.HorizontalMovement;
    private float m_VelocitySmoothing;
    public PlayerIdleState(Player player) : base(player) { }

    public override void Enter(IState from)
    {
        Input.ListenEvents(this);
        Player.Animator.CrossFade(IdleAnimationHash, 0.0f);
        Player.Velocity.y = 0.0f; //TODO: Be better
    }

    public override void Exit()
    {
        Input.StopListening(this);
    }
    public override void Update()
    {
        if (!Player.Controller.IsGrounded)
        {
            ChangeState(Player.AirState); return;
        }
        if (HorizontalInput > 0 && !Controller.IsFacingWallRight || HorizontalInput < 0 && !Controller.IsFacingWallLeft)
        {
            ChangeState(Player.WalkState); return;
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

    [GameAction(ActionType.Throw)]
    protected void OnThrow()
    {
        Player.ThrowFirebottle();
    }

    [GameAction(ActionType.Attack)]
    protected void OnAttack()
    {
        Debug.Log("OnAttack");
        ChangeState(new PlayerAttackMeleeState(Player));
    }

    [GameAction(ActionType.Move)]
    protected void OnMove(float direction)
    {
        Player.Flip((int)Mathf.Sign(direction));
    }

    [GameAction(ActionType.Jump)]
    protected void OnJump()
    {
        ChangeState(Player.JumpState);
    }
}
