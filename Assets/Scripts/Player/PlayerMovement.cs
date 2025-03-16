using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    public PlayerMovementData MovementData;
    [SerializeField] private Collider2D m_FeetCoolider;
    [SerializeField] private Collider2D m_BodyColllider;

    private Rigidbody2D m_Rigidbody;

    private RaycastHit2D m_GroundHit;
    private RaycastHit2D m_HeadHit;
    private bool m_IsGrounded;
    private bool m_BumpedHead;

    private Vector2 m_MoveVelocity;
    private bool m_IsFacingRight;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
