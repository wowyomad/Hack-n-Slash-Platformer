using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
public class PlayerWalkState : PlayerBaseState
{
    private float m_VelocitySmoothing = 0.0f;
    private float m_LastInputTime = 0.0f;
    private float m_InputThreashold = 0.1f;
    private float m_VeloctiyThreshold = 0.03f;
    public PlayerWalkState(Player player) : base(player) { }

    public override void OnEnter(IState state)
    {
        Player.Input.Jump += OnJump;
        Player.Input.Move += OnMove;

        m_VelocitySmoothing = 0.0f;
    }

    public override void OnExit()
    {
        Player.Input.Jump -= OnJump;
        Player.Input.Move -= OnMove;
    }

    public override void Update()
    {
        if (!Controller.IsGrounded)
        {
            ChangeState(Player.AirState); return;
        }

        if (Mathf.Abs(Player.Velocity.x) <= 3.0f)
        {
            Player.Animator.CrossFade(IdleAnimationHash, 0.0f);
        }
        else
        {
            Player.Animator.CrossFade(WalkAnimationHash, 0.0f);
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

        Player.Velocity.x = Mathf.SmoothDamp(Player.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeGrounded);

        if (Mathf.Abs(Player.Velocity.x) <= m_VeloctiyThreshold && Time.time - m_LastInputTime > m_InputThreashold)
        {
            ChangeState(Player.IdleState);
        }
    }
    public void OnMove(float direction)
    {
        Player.Flip((int)direction);
        m_LastInputTime = Time.time;

    }
    public void OnJump()
    {
        ChangeState(Player.JumpState);
    }
}