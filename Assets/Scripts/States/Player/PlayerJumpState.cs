using GameActions;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    protected float m_VelocitySmoothing = 0.0f;
    public float JumpVelocity => Player.Movement.JumpVelocity;

    public PlayerJumpState(Player player) : base(player) { }

    public override void Enter(IState from)
    {
        Input.ListenEvents(this);
        Player.Animator.CrossFade(JumpAnimationHash, 0.0f);
        Player.Velocity.y = JumpVelocity;
    }
    public override void Exit()
    {
        Input.StopListening(this);
    }
    public override void Update()
    {
        Player.ApplyGravity();

        if (Player.Velocity.y <= 0.0f)
        {
            Player.ChangeState(Player.AirState); return;
        }

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

    [GameAction(ActionType.Throw)]
    protected void OnThrow()
    {
        Player.ThrowFirebottle();
    }

    [GameAction(ActionType.Move)]
    protected void OnMove(float direction)
    {
        Player.Flip(direction);
    }
}

