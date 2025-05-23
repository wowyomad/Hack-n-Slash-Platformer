using UnityEngine;

namespace TheGame
{
    public class PlayerAttackState : PlayerBaseState
    {
        public bool AttackFinished { get; private set; } = false;
        private MeleeCombat m_Melee;
        private CharacterStatsSO m_Stats;

        private float m_LastAttackTime = 0.0f;
        private Vector2 m_LastAttackDirection = Vector2.zero;

        private ActionTimer m_AttackTimer;
        private float m_InitialAttackVelocityX;

        public PlayerAttackState(Player player) : base(player)
        {
            m_Stats = player.Stats;
            m_Melee = player.GetComponent<MeleeCombat>();

            m_AttackTimer = new ActionTimer();
            m_AttackTimer.SetFinishedCallback(() => AttackFinished = true);

            if (m_Melee == null)
            {
                Debug.LogError("MeleeCombat component not found on Player.", player);
            }
        }

        public override void OnEnter()
        {
            Player.TurnToCursor();

            float impulseScale = 1.0f;

            //resets the cd
            if (Controller.IsGrounded && Vector2.Dot(m_LastAttackDirection.normalized, Vector2.up) > 0.9f)
            {
                m_LastAttackTime = Time.time - m_Stats.AttackImpulseCooldown;
            }

            if (Time.time - m_LastAttackTime < m_Stats.AttackImpulseCooldown)
            {
                if (m_Stats.AttackImpulseCooldown > 0)
                {
                    impulseScale = Mathf.Clamp01((Time.time - m_LastAttackTime) / m_Stats.AttackImpulseCooldown);
                }
            }

            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Player.Input.CursorPosition);
            Vector2 direction = (cursorPosition - Player.transform.position).normalized;

            direction = SnapDirectionTo8(direction);
            m_LastAttackDirection = direction;

            if (Controller.IsGrounded && (direction == Vector2.left || direction == Vector2.right))
            {
                impulseScale *= m_Stats.AttackHorizontalImpulseReductionFactor;
            }

            float initialVelocityX = direction.x * m_Stats.AttackImpulse * impulseScale;
            float initialVelocityY = direction.y * m_Stats.AttackImpulse * impulseScale;

            Controller.Velocity = new Vector2(initialVelocityX, initialVelocityY);
            m_InitialAttackVelocityX = initialVelocityX;

            m_AttackTimer.Stop();
            m_AttackTimer.Start(m_Stats.AttackDuration);

            HitData hitData = new HitData(Player.gameObject);
            hitData.Direction = direction;

            m_Melee.OnTargetHit += HandleTargetHitResult;
            m_Melee.StartAttack(hitData);
        }

        public override void OnExit()
        {
            m_Melee.CancellAttack();
            m_LastAttackTime = Time.time;

            m_Melee.OnTargetHit -= HandleTargetHitResult;

            AttackFinished = false;
        }

        public override void OnUpdate()
        {
            if (AttackFinished) return;

            m_AttackTimer.Tick();

            float newXVelocity;

            float targetSlowdownVelocityX = m_InitialAttackVelocityX * m_Stats.AttackSlowdownFactor;

            if (m_AttackTimer.ElapsedTime >= m_Stats.AttackDuration)
            {
                newXVelocity = targetSlowdownVelocityX;
                AttackFinished = true;
            }
            else
            {
                float t = m_AttackTimer.ElapsedTime / m_Stats.AttackDuration;
                newXVelocity = Mathf.Lerp(m_InitialAttackVelocityX, targetSlowdownVelocityX, t);
            }

            Controller.Velocity.x = newXVelocity;
        }

        private Vector3 SnapDirectionTo8(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            angle = Mathf.Round(angle / 45.0f) * 45.0f;
            float snappedX = Mathf.Cos(angle * Mathf.Deg2Rad);
            float snappedY = Mathf.Sin(angle * Mathf.Deg2Rad);
            return new Vector2(snappedX, snappedY).normalized;
        }

        private void HandleTargetHitResult(HitResult hitResult, GameObject target)
        {
            
        }
    }

}
