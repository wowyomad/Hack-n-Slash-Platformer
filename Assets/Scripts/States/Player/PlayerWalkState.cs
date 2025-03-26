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
        Player.Input.AttackMelee += OnAttackMelee;
        Player.Input.Move += OnMove;
        Player.Input.Jump += OnJump;

        m_VelocitySmoothing = 0.0f;
        Player.Velocity.y = 0.0f; //TODO: Be better
    }

    public override void OnExit()
    {
        Player.Input.AttackMelee -= OnAttackMelee;
        Player.Input.Move -= OnMove;
        Player.Input.Jump -= OnJump;
    }

    public override void Update()
    {
        if (!Controller.IsGrounded)
        {
            ChangeState(Player.AirState); return;
        }
        
        //TODO: use FacingWall instead.
        if (Input.HorizontalMovement == 1  && Controller.Collisions.Right ||
            Input.HorizontalMovement == -1 && Controller.Collisions.Left)
        {
            ChangeState(Player.IdleState); return;
        }

        //Abomination
        if (Mathf.Abs(Player.Velocity.x) <= 3.0f)
        {
            Player.Animator.CrossFade(IdleAnimationHash, 0.0f);
        }
        else
        {
            Player.Animator.CrossFade(WalkAnimationHash, 0.0f);
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

        Player.Velocity.x = Mathf.SmoothDamp(Player.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeGrounded);

        if (Mathf.Abs(Player.Velocity.x) <= m_VeloctiyThreshold && Time.time - m_LastInputTime > m_InputThreashold)
        {
            ChangeState(Player.IdleState); return;
        }
    }
    public void OnMove(float direction)
    {
        m_LastInputTime = Time.time;
        Player.Flip(direction);
    }

    public void OnAttackMelee()
    {
        ChangeState(new PlayerAttackingMeleeState(Player));
    }
    public void OnJump()
    {
        ChangeState(Player.JumpState); return;
    }
}