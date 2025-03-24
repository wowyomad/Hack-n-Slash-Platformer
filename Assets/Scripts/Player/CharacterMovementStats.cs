using UnityEngine;


[CreateAssetMenu(menuName = "Character Movement Stats")]
public class CharacterMovementStats : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float m_JumpHeight = 4.0f;
    [SerializeField] private float m_TimeToJumpApex = 0.4f;
    [SerializeField] private float m_MaxGravityScale = 1.5f;

    [Header("Calcualted Values")]
    public float Gravity { get; private set; }
    public float MaxGravityVelocity { get; private set; }
    public float JumpVelocity { get; private set; }
    public float HorizontalSpeed = 8.0f;
    public float AccelerationTimeAirborne = 0.25f;
    public float AccelerationTimeGrounded = 0.1f;

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

