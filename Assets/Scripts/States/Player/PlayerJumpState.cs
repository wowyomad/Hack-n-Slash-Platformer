using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    protected float m_VelocitySmoothing = 0.0f;
    public float JumpVelocity => Player.Movement.JumpVelocity;

    public PlayerJumpState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Input.Move += OnMove;
        Player.Animator.CrossFade(JumpAnimationHash, 0.0f);
        Player.Velocity.y = JumpVelocity;
    }
    public override void OnExit()
    {
        Input.Move -= OnMove;
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

    public void OnMove(float direction)
    {
            Player.Flip(direction);
    }
}

