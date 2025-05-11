using TheGame;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IPlayerVulnarableState
{
    public bool AttackFinished { get; private set; } = false;
    private Weapon m_Weapon;
    private CharacterController2D m_Controller;
    private CharacterStatsSO m_Stats;

    private float m_LastAttackTime = 0.0f;
   
    private float m_AttackStartTime;
    private float m_InitialAttackVelocityX;
    public PlayerAttackState(Player player) : base(player)
    {
        m_Controller = player.GetComponent<CharacterController2D>();
        m_Stats = player.Stats;
        m_Weapon = player.WeaponReference;

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
        AttackFinished = false;
        m_AttackStartTime = Time.time;
        float impulseScale = 1.0f;
        if (Time.time - m_LastAttackTime < m_Stats.AttackImpulseCooldown)
        {
            if (m_Stats.AttackImpulseCooldown > 0)
            {
                impulseScale = Mathf.Clamp01((Time.time - m_LastAttackTime) / m_Stats.AttackImpulseCooldown);
            }
        }

        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Player.Input.CursorPosition);
        Vector3 direction = (cursorPosition - Camera.main.transform.position).normalized;

        float initialVelocityX = direction.x * m_Stats.AttackImpulse * impulseScale;
        float initialVelocityY = direction.y * m_Stats.AttackImpulse * impulseScale;

        m_Controller.Velocity = new Vector2(initialVelocityX, initialVelocityY);
        m_InitialAttackVelocityX = initialVelocityX;

        m_Weapon?.Attack(direction);
    }

    public override void OnExit()
    {
        m_Controller.Velocity.x = 0.0f;
        m_Weapon?.Stop();
        m_LastAttackTime = Time.time;
    }

    public override void OnUpdate()
    {
        if (AttackFinished) return;

        float elapsedTime = Time.time - m_AttackStartTime;
        float newXVelocity;

        if (m_Stats.AttackSlowdownDuration <= 0f)
        {
            newXVelocity = 0f;
            AttackFinished = true;
        }
        else if (elapsedTime >= m_Stats.AttackSlowdownDuration)
        {
            newXVelocity = 0f;
            AttackFinished = true;
        }
        else
        {
            float t = elapsedTime / m_Stats.AttackSlowdownDuration;
            newXVelocity = Mathf.Lerp(m_InitialAttackVelocityX, 0f, t);

            if (Mathf.Abs(newXVelocity) < m_Stats.VelocityThreshold)
            {
                newXVelocity = 0f;
                AttackFinished = true;
            }
        }

        m_Controller.Velocity.x = newXVelocity;

        if (AttackFinished && m_Controller.Velocity.x != 0f)
        {
            m_Controller.Velocity.x = 0.0f;
        }
    }
}
