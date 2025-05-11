using TheGame;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IPlayerVulnarableState
{
    public bool AttackFinished { get; private set; } = false;
    private Weapon m_Weapon;
    private CharacterController2D m_Controller;

    private float m_LastAttackTime = 0.0f;
    public float Impulse;
    public float ImpulseCooldown;
    public float SlowdownDuration;
    public float VelocityThreshold;

    private float m_AttackStartTime;
    private float m_InitialAttackVelocityX;
    public PlayerAttackState(Player player, Weapon weapon, float attackImpulse, float impulseCooldown, float slowdownDuration, float velocityThreshold) : base(player)
    {
        m_Controller = player.GetComponent<CharacterController2D>();
        m_Weapon = weapon;

        Impulse = attackImpulse;
        ImpulseCooldown = impulseCooldown;
        SlowdownDuration = slowdownDuration;
        VelocityThreshold = velocityThreshold;

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
        if (Time.time - m_LastAttackTime < ImpulseCooldown)
        {
            if (ImpulseCooldown > 0)
            {
                impulseScale = Mathf.Clamp01((Time.time - m_LastAttackTime) / ImpulseCooldown);
            }
        }

        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Player.Input.CursorPosition);
        Vector3 direction = (cursorPosition - Camera.main.transform.position).normalized;

        float initialVelocityX = direction.x * Impulse * impulseScale;
        float initialVelocityY = direction.y * Impulse * impulseScale;

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

        if (SlowdownDuration <= 0f)
        {
            newXVelocity = 0f;
            AttackFinished = true;
        }
        else if (elapsedTime >= SlowdownDuration)
        {
            newXVelocity = 0f;
            AttackFinished = true;
        }
        else
        {
            float t = elapsedTime / SlowdownDuration;
            newXVelocity = Mathf.Lerp(m_InitialAttackVelocityX, 0f, t);

            if (Mathf.Abs(newXVelocity) < VelocityThreshold)
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
