using UnityEngine;

namespace TheGame
{
    public class PlayerAirState : PlayerBaseState
    {
        private float m_VelocitySmoothing = 0.0f;

        public PlayerAirState(Player player) : base(player) { }

        public override void OnEnter()
        {
            m_VelocitySmoothing = 0f;
        }

        public override void OnUpdate()
        {
            float moveInput = Player.Input.Horizontal;

            Player.Flip(moveInput);


            float targetVelocityX = moveInput * Player.Stats.HorizontalSpeed;

            if (Controller.Collisions.Right && targetVelocityX > 0)
            {
                targetVelocityX = 0;
            }
            else if (Controller.Collisions.Left && targetVelocityX < 0)
            {
                targetVelocityX = 0;
            }

            Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Stats.AccelerationTimeAirborne);
        }
    }
}
