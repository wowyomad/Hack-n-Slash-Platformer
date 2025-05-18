using UnityEngine;

namespace TheGame
{
    public class PlayerWalkState : PlayerBaseState
    {
        private float m_VelocitySmoothing = 0.0f;
        public PlayerWalkState(Player player) : base(player) { }

        public override void OnEnter()
        {
            m_VelocitySmoothing = 0.0f;
        }

        public override void OnUpdate()
        {
            float moveInput = Player.Input.Horizontal;

            Player.Flip(moveInput);

            //Abomination

            float targetVelocityX = moveInput * Player.Stats.HorizontalSpeed;
            if (Controller.Collisions.Right)
            {
                targetVelocityX = Mathf.Min(targetVelocityX, 0);
            }
            else if (Controller.Collisions.Left)
            {
                targetVelocityX = Mathf.Max(targetVelocityX, 0);
            }

            Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Stats.AccelerationTimeGrounded);
        }
    }
}
