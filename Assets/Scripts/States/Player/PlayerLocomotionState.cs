using UnityEngine;

public class PlayerLocomotionState : PlayerBaseState
{
    private float m_VelocitySmoothing = 0.0f;
    public PlayerLocomotionState(Player player) : base(player) { }
    protected void Move(float targetVelocity, float acceleration, float decceleration = 0.0f)
    {

        if (Mathf.Abs(Controller.Velocity.x) < 0.01f)
        {
            Controller.Velocity.x = 0f;
        }

        Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocity, ref m_VelocitySmoothing, acceleration);

    }
}