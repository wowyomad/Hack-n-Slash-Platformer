using UnityEngine;


[CreateAssetMenu(menuName = "Character Movement Stats")]
public class CharacterStatsSO : ScriptableObject
{
    [Header("Movement")]
    public float HorizontalSpeed = 8.0f;

    [SerializeField] private float m_JumpHeight = 4.0f;
    [SerializeField] private float m_JumpTime = 0.4f;
    [SerializeField] private float m_FallTime = 0.4f;
    public float AccelerationTimeAirborne = 0.25f;
    public float AccelerationTimeGrounded = 0.1f;
    public float DecelerationTimeAirborne = 0.1f;
    public float DecelerationTimeGrounded = 0.1f;



    [Header("Attack")]
    public float AttackCooldown = 0.4f;
    public float AttackImpulseCooldown = 0.6f;
    public float AttackImpulse = 20.0f;
    public float AttackSlowdownDuration = 0.2f;

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
        MaxGravityVelocity = -m_JumpHeight / m_FallTime;
    }

    private void RecalculateGravity2()
    {
        Gravity = 2 * m_JumpHeight / (m_FallTime * m_FallTime);

        // Step 2: Calculate initial velocity (v_initial) to reach the desired height in the desired time
        JumpVelocity = (2 * m_JumpHeight) / m_JumpTime;

        float calculatedTimeToFall = Mathf.Sqrt(2 * m_JumpHeight / Gravity);
        Debug.Log($"Verified Time to Fall: {calculatedTimeToFall} seconds");
    }

}

