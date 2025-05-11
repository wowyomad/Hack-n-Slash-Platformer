using TheGame;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IPlayerVulnarableState
{
    public bool AttackFinished { get; private set; } = false;
    private Weapon m_Weapon;
    private CharacterController2D m_Controller;
    private CharacterStatsSO m_Stats;

    private float m_LastAttackTime = 0.0f;

    private ActionTimer m_AttackTimer;
    private float m_InitialAttackVelocityX;

    public PlayerAttackState(Player player) : base(player)
    {
        m_Controller = player.GetComponent<CharacterController2D>();
        m_Stats = player.Stats;
        m_Weapon = player.WeaponReference;

        m_AttackTimer = new ActionTimer();
        m_AttackTimer.SetFinishedCallback(() => AttackFinished = true);

        if (m_Weapon == null)
        {
            Debug.LogError("Weapon component not found on Player children.", player);
        }
        if (m_Controller == null)
        {
            Debug.LogError("CharacterController2D component not found on Player.", player);
        }
    }

    public override void OnEnter()
    {
        Player.TurnToCursor();
        float impulseScale = 1.0f;
        if (Time.time - m_LastAttackTime < m_Stats.AttackImpulseCooldown)
        {
            if (m_Stats.AttackImpulseCooldown > 0)
            {
                impulseScale = Mathf.Clamp01((Time.time - m_LastAttackTime) / m_Stats.AttackImpulseCooldown);
            }
        }

        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Player.Input.CursorPosition);
        Vector3 direction = (cursorPosition - Player.transform.position).normalized;

        // Snap direction to the closest of 8 directions
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = Mathf.Round(angle / 45.0f) * 45.0f;
        float snappedX = Mathf.Cos(angle * Mathf.Deg2Rad);
        float snappedY = Mathf.Sin(angle * Mathf.Deg2Rad);
        direction = new Vector3(snappedX, snappedY, 0).normalized;

        float initialVelocityX = direction.x * m_Stats.AttackImpulse * impulseScale;
        float initialVelocityY = direction.y * m_Stats.AttackImpulse * impulseScale;

        m_Controller.Velocity = new Vector2(initialVelocityX, initialVelocityY);
        m_InitialAttackVelocityX = initialVelocityX;

        m_AttackTimer.Stop();
        m_AttackTimer.Start(m_Stats.AttackDuration);

        m_Weapon?.Attack(direction);
    }

    public override void OnExit()
    {
        m_Weapon?.Stop();
        m_LastAttackTime = Time.time;

        AttackFinished = false;
    }

    public override void OnUpdate()
    {
        if (AttackFinished) return;

        m_AttackTimer.Tick();

        float newXVelocity;

        float targetSlowdownVelocityX = m_InitialAttackVelocityX * m_Stats.AttackSlowdownScale;

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

        m_Controller.Velocity.x = newXVelocity;
    }
}
