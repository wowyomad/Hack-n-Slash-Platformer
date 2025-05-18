using UnityEngine;

namespace TheGame
{

    public class PlayerIdleState : PlayerBaseState
    {
        private float m_VelocitySmoothing;
        public PlayerIdleState(Player player) : base(player) { }

        public override void OnUpdate()
        {
            if (Mathf.Abs(Controller.Velocity.x) > 0.0f)
            {
                Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, 0, ref m_VelocitySmoothing, Player.Stats.AccelerationTimeGrounded * 0.5f); // Faster damping to stop
                if (Mathf.Abs(Controller.Velocity.x) < 0.01f)
                {
                    Controller.Velocity.x = 0f;
                }
            }
        }
    }
}
