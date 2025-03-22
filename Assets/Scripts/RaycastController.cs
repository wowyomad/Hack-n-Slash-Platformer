using UnityEngine;

public class RaycastController : MonoBehaviour
{
    protected RaycastOrigins m_RaycastOrigins;
    protected Collider2D m_Collider;

    [SerializeField] protected LayerMask m_CollisionMask;
    [SerializeField][Range(2, 32)] protected int m_HorizontalRayCount = 4;
    [SerializeField][Range(2, 32)] protected int m_VerticalRayCount = 4;
    [SerializeField][Range(0.001f, 1.0f)] protected float m_SkinWidth = 0.015f;

    protected float m_HorizontalRaySpacing;
    protected float m_VerticalRaySpacing;


    protected virtual void Awake()
    {
        m_Collider = GetComponent<Collider2D>();
        RecalculateRaySpacing();
    }

    protected virtual void OnValidate()
    {
        RecalculateRaySpacing();
    }

    protected virtual void Update()
    {
        RecalculateRaycastOrigins();
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


    protected struct RaycastOrigins
    {
        public Vector2 BottomLeft, BottomRight;
        public Vector2 TopLeft, TopRight;
    }
}