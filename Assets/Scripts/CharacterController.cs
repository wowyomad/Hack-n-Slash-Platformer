using System.Data;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController : MonoBehaviour
{
    private Collider2D m_Collider;

    private RaycastOrigins m_RaycastOrigins;
    private CollisionInfo m_CollisionInfo;
    public CollisionInfo Collisions { get { return m_CollisionInfo; } }

    [SerializeField] private LayerMask m_CollisionMask;
    [SerializeField][Range(2, 32)] private int m_HorizontalRayCount = 4;
    [SerializeField][Range(2, 32)] private int m_VerticalRayCount = 4;
    [SerializeField][Range(0.001f, 1.0f)] private float m_SkinWidth = 0.015f;

    private float m_HorizontalRaySpacing;
    private float m_VerticalRaySpacing;

    private Vector3 m_Displacement;

    [SerializeField] private bool m_DisplayDebugRays;

    public void Move(Vector3 displacement)
    {
        m_Displacement += displacement;
    }

    private void FixedUpdate()
    {
    }

    private void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        RecalculateRaySpacing();
    }

    private void OnValidate()
    {
        RecalculateRaySpacing();
    }

    private void Update()
    {
        RecalculateRaycastOrigins();
        VerticalCollisions();
        HorizontalCollisions();

        if (m_DisplayDebugRays)
        {
            Vector2 displacement = new Vector2(m_Displacement.x, m_Displacement.y);
            for (int i = 0; i < m_VerticalRayCount; i++)
            {
                if (m_Displacement.y > 0)
                    Debug.DrawRay(m_RaycastOrigins.TopLeft + displacement + Vector2.right * (m_VerticalRaySpacing * i), Vector2.up * Mathf.Abs(m_Displacement.y), Color.red);
                else
                    Debug.DrawRay(m_RaycastOrigins.BottomLeft + displacement + Vector2.right * (m_VerticalRaySpacing * i), Vector2.down * Mathf.Abs(m_Displacement.y), Color.red);

                if (m_Displacement.x > 0)
                    Debug.DrawRay(m_RaycastOrigins.BottomRight + displacement + Vector2.up * (m_HorizontalRaySpacing * i), Vector2.right * Mathf.Abs(m_Displacement.x), Color.red);
                else
                    Debug.DrawRay(m_RaycastOrigins.BottomLeft + displacement + Vector2.up * (m_HorizontalRaySpacing * i), Vector2.left * Mathf.Abs(m_Displacement.x), Color.red);
            }

        }

        transform.Translate(m_Displacement);
        m_Displacement = Vector3.zero;
    }
    protected void HorizontalCollisions()
    {
        float xDirection = Mathf.Sign(m_Displacement.x);
        float rayLength = Mathf.Abs(m_Displacement.x) + m_SkinWidth;

        for (int i = 0; i < m_HorizontalRayCount; i++)
        {
            Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * (m_HorizontalRaySpacing * i + m_Displacement.y);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, m_CollisionMask);
            if (hit)
            {
                m_Displacement.x = (hit.distance - m_SkinWidth) * xDirection;
                rayLength = hit.distance;
            }
        }
    }

    protected void VerticalCollisions()
    {
        float yDirection = Mathf.Sign(m_Displacement.y);
        float rayLength = Mathf.Abs(m_Displacement.y) + m_SkinWidth;

        for (int i = 0; i < m_VerticalRayCount; i++)
        {
            Vector2 origin = yDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.TopLeft;
            origin += Vector2.right * (m_VerticalRaySpacing * i + m_Displacement.x);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * yDirection, rayLength, m_CollisionMask);
            if (hit)
            {
                m_Displacement.y = (hit.distance - m_SkinWidth) * yDirection;
                rayLength = hit.distance;
            }
        }
    }


    protected void RecalculateRaycastOrigins()
    {
        Bounds bounds = m_Collider.bounds;
        bounds.Expand(-m_SkinWidth * 2.0f);
        m_RaycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        m_RaycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        m_RaycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        m_RaycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    protected void RecalculateRaySpacing()
    {
        if (m_Collider == null) return;

        Bounds bounds = m_Collider.bounds;
        bounds.Expand(-m_SkinWidth * 2.0f);
        m_HorizontalRayCount = Mathf.Clamp(m_HorizontalRayCount, 2, int.MaxValue);
        m_VerticalRayCount = Mathf.Clamp(m_VerticalRayCount, 2, int.MaxValue);
        m_HorizontalRaySpacing = bounds.size.y / (m_HorizontalRayCount - 1);
        m_VerticalRaySpacing = bounds.size.x / (m_VerticalRayCount - 1);
    }
    private struct RaycastOrigins
    {
        public Vector2 BottomLeft, BottomRight;
        public Vector2 TopLeft, TopRight;
    }

    public struct CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;

        public void Reset()
        {
            Above = Below = Left = Right = false;
        }
    }
}