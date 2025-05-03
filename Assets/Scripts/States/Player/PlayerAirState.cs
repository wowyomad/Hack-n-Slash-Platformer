using UnityEngine;
using GameActions;
public class PlayerAirState : PlayerBaseState, IPlayerVulnarableState
{
    private float m_VelocitySmoothing = 0.0f;
    public PlayerAirState(Player player) : base(player) { }

    public override void Enter(IState from)
    {
        Input.ListenEvents(this);
        Player.Animator.CrossFade(AirAnimationHash, 0.0f);
    }

    public override void Exit()
    {
        Input.StopListening(this);
    }

    public override void Update()
    {
        if (Controller.IsGrounded)
        {
            Player.ChangeState(Player.IdleState); return;
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

        Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Movement.AccelerationTimeAirborne);
    }

    [GameAction(ActionType.Throw)]
    protected void OnThrow()
    {
        Player.ThrowKnife();
    }
    [GameAction(ActionType.Move)]
    protected void OnMove(float direction)
    {
        Player.Flip(direction);
    }
}