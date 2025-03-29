using GameActions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;
public class PlayerWalkState : PlayerBaseState, IPlayerVulnarableState
{
    private float m_VelocitySmoothing = 0.0f;
    private float m_LastInputTime = 0.0f;
    private float m_InputThreashold = 0.1f;
    private float m_VeloctiyThreshold = 0.03f;
    public PlayerWalkState(Player player) : base(player) { }

    public override void Enter(IState state)
    {
        Input.ListenEvents(this);

        m_VelocitySmoothing = 0.0f;
        Player.Velocity.y = 0.0f; //TODO: Be better
    }

    public override void Exit()
    {
        Input.StopListening(this);
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

    [GameAction(ActionType.Dash)]
    protected void OnPass()
    {
        Controller.PassThrough();
    }

    [GameAction(ActionType.Move)]
    protected void OnMove(float direction)
    {
        m_LastInputTime = Time.time;
        Player.Flip(direction);
    }
    [GameAction(ActionType.Throw)]
    protected void OnThrow()
    {
        Player.ThrowKnife();
    }

    [GameAction(ActionType.Attack)]
    protected void OnAttack()
    {
        ChangeState(new PlayerAttackState(Player));
    }

    [GameAction(ActionType.Jump)]
    protected void OnJump()
    {
        ChangeState(Player.JumpState); return;
    }
}