using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    Collider2D m_Collider;
    [SerializeField] LayerMask m_CollisionMask;

    [SerializeField][Range(0.0f, 10.0f)] private float m_SkinWidth = 0.015f;
    [SerializeField][Range(2, 32)] private int m_HorizontalRayCount = 4;
    [SerializeField][Range(2, 32)] private int m_VerticalRayCount = 4;

    [SerializeField] public float Gravity = -10.0f;
    private float m_GravitySpeed = 0.0f;
    [SerializeField] public float MaxFallSpeed = -30.0f;

    private Vector2 m_Velocity;

    [SerializeField] private float m_HorizontalRaySpacing;
    [SerializeField] private float m_VerticalRaySpacing;

    RaycastOrigins m_RaycastOrigins;

    public bool IsGrounded { get; private set; }

    void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        CalculateRaySpacing();
    }

    private void OnValidate()
    {
        if (m_Collider != null)
        {
            CalculateRaySpacing();
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = m_Collider.bounds;
        bounds.Expand(-2 * m_SkinWidth);

        m_RaycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        m_RaycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        m_RaycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        m_RaycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void HorizontalCollisions(ref Vector2 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + m_SkinWidth;

        for (int i = 0; i < m_HorizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            rayOrigin += Vector2.up * (m_HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, m_CollisionMask);

            Debug.DrawLine(rayOrigin, rayOrigin + Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                velocity.x = (hit.distance - m_SkinWidth) * directionX;
                rayLength = hit.distance;
            }
        }
    }

    private void VerticalCollisions(ref Vector2 velocity)
    {
        IsGrounded = false;
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + m_SkinWidth;

        for (int i = 0; i < m_VerticalRayCount; i++)
        {
            Vector2 rayOrigin = directionY == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.TopLeft;
            rayOrigin += Vector2.right * (m_VerticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, m_CollisionMask);

            Debug.DrawLine(rayOrigin, rayOrigin + Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - m_SkinWidth) * directionY;
                rayLength = hit.distance;

                if (directionY == -1)
                {
                    IsGrounded = true;
                }
            }
        }
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = m_Collider.bounds;
        bounds.Expand(-2 * m_SkinWidth);

        m_HorizontalRayCount = Mathf.Clamp(m_HorizontalRayCount, 2, int.MaxValue);
        m_VerticalRayCount = Mathf.Clamp(m_VerticalRayCount, 2, int.MaxValue);

        m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
        m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
    }

    private void Update()
    {
        if (!IsGrounded)
        {
            m_GravitySpeed += Gravity * Time.deltaTime;
            m_GravitySpeed = Mathf.Max(m_GravitySpeed, MaxFallSpeed);
            m_Velocity.y += m_GravitySpeed;
        }
        else
        {
            m_GravitySpeed = 0.0f;
        }

        m_Velocity *= Time.deltaTime;

        UpdateRaycastOrigins();
        if (m_Velocity.y != 0.0f)
        {
            VerticalCollisions(ref m_Velocity);
        }
        if (m_Velocity.x != 0.0f)
        {
            HorizontalCollisions(ref m_Velocity);
        }
        transform.Translate(m_Velocity);

        m_Velocity = Vector2.zero;
    }

    public void Move(Vector2 velocity)
    {
        m_Velocity += velocity;
    }

    struct RaycastOrigins
    {
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }
}
