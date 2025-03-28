using UnityEngine;

public class RaycastController : MonoBehaviour
{
    protected RaycastOrigins m_RaycastOrigins;
    protected Collider2D m_Collider;

    [SerializeField] protected LayerMask CollisionMask;
    [SerializeField] protected LayerMask PassThroughMask;

    [SerializeField][Range(2, 32)] protected int HorizontalRayCount = 4;
    [SerializeField][Range(2, 32)] protected int VerticalRayCount = 4;
    [SerializeField][Range(0.001f, 1.0f)] protected float SkinWidth = 0.015f;

    protected float HorizontalRaySpacing;
    protected float VerticalRaySpacing;


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
        bounds.Expand(-SkinWidth * 2.0f);
        m_RaycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        m_RaycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        m_RaycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        m_RaycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    protected void RecalculateRaySpacing()
    {
        if (m_Collider == null) return;

        Bounds bounds = m_Collider.bounds;
        bounds.Expand(-SkinWidth * 2.0f);
        HorizontalRayCount = Mathf.Clamp(HorizontalRayCount, 2, int.MaxValue);
        VerticalRayCount = Mathf.Clamp(VerticalRayCount, 2, int.MaxValue);
        HorizontalRaySpacing = bounds.size.y / (HorizontalRayCount - 1);
        VerticalRaySpacing = bounds.size.x / (VerticalRayCount - 1);
    }


    protected struct RaycastOrigins
    {
        public Vector2 BottomLeft, BottomRight;
        public Vector2 TopLeft, TopRight;
    }
}