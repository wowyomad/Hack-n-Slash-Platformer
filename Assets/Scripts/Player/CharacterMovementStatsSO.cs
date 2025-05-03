using UnityEngine;


[CreateAssetMenu(menuName = "Character Movement Stats")]
public class CharacterMovementStatsSO : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float m_JumpHeight = 4.0f;
    [SerializeField] private float m_TimeToJumpApex = 0.4f;
    [SerializeField] private float m_MaxGravityScale = 1.5f;
    public float AccelerationTimeAirborne = 0.25f;
    public float AccelerationTimeGrounded = 0.1f;
    public float DecelerationTimeAirborne = 0.1f;
    public float DecelerationTimeGrounded = 0.1f;

    [Header("Calcualted Values")]
    public float Gravity { get; private set; }
    public float MaxGravityVelocity { get; private set; }
    public float JumpVelocity { get; private set; }
    public float HorizontalSpeed = 8.0f;


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
        Gravity = -(2 * m_JumpHeight) / Mathf.Pow(m_TimeToJumpApex, 2);
        MaxGravityVelocity = Gravity * m_MaxGravityScale;
        JumpVelocity = -Gravity * m_TimeToJumpApex;
    }

}

