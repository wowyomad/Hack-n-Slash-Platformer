using UnityEngine;

namespace TheGame
{
    public class PlayerDashState : PlayerBaseState
    {
        public bool DashFinished { get; private set; } = false;
        private CharacterController2D m_Controller;

        private CharacterStatsSO m_Stats;

        private float m_CalculatedDashSpeed;

        private ActionTimer m_DashTimer;
        public PlayerDashState(Player player) : base(player)
        {
            m_Controller = player.GetComponent<CharacterController2D>();
            m_Stats = player.Stats;

            m_DashTimer = new ActionTimer();
            m_DashTimer.SetFinishedCallback(() => DashFinished = true);

            if (m_Controller == null)
            {
                Debug.LogError("CharacterController2D component not found on Player.", player);
            }
        }

        public override void OnEnter()
        {
            float dashDirection = Player.Input.Horizontal != 0 ? Mathf.Sign(Player.Input.Horizontal) : Player.FacingDirection;
            if (dashDirection == 0)
            {
                dashDirection = 1;
            }
            m_CalculatedDashSpeed = dashDirection * m_Stats.DashSpeed;

            m_Controller.Velocity.y = 0f;
            m_Controller.ApplyGravity = false;

            m_Controller.Velocity.x = m_CalculatedDashSpeed;

            m_DashTimer.Stop();
            m_DashTimer.Start(m_Stats.DashDuration);
        }

        public override void OnExit()
        {
            DashFinished = false;
            m_Controller.ApplyGravity = true;
        }

        public override void OnUpdate()
        {
            if (m_Controller.Velocity.x == 0.0f)
            {
                DashFinished = true;
            }

            if (DashFinished) return;

            m_DashTimer.Tick();
        }
    }
}
