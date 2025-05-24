using UnityEngine;


[CreateAssetMenu(menuName = "Character Movement Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float HorizontalSpeed = 8.0f;

    [SerializeField] private float m_JumpHeight = 4.0f;
    [SerializeField] private float m_JumpTime = 0.4f;
    [SerializeField] private float m_FallTime = 0.4f;
    [SerializeField] private float m_MaxGravityVelocityScale = 1.0f;
    public float AccelerationTimeAirborne = 0.25f;
    public float AccelerationTimeGrounded = 0.1f;
    public float DecelerationTimeAirborne = 0.1f;
    public float DecelerationTimeGrounded = 0.1f;



    [Header("Attack")]
    public float AttackCooldown = 0.4f;
    public float AttackDuration = 0.2f;
    public float AttackSlowdownFactor = 0.5f;
    public float AttackImpulseCooldown = 0.6f;
    public float AttackHorizontalImpulseReductionFactor = 0.25f;
    public float AttackImpulse = 20.0f;

    public float Gravity { get; private set; }
    public float MaxGravityVelocity { get; private set; }
    public float JumpVelocity { get; private set; }


    [Header("Dash")]
    public float DashCooldown = 0.4f;
    public float DashSpeed = 20f;
    public float DashDuration = 0.25f;



    [Header("Other")]
    public float VelocityThreshold = 0.01f;

    private void Awake()
    {
        RecalculateGravity();
    }

    private void OnValidate()
    {
        RecalculateGravity();

    }

    private void RecalculateGravity()
    {
        Gravity = -(2 * m_JumpHeight) / Mathf.Pow(m_JumpTime, 2);
        JumpVelocity = -Gravity * m_JumpTime;
        MaxGravityVelocity = m_MaxGravityVelocityScale * -m_JumpHeight / m_FallTime;
    }
}

