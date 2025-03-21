using System;
using System.Data;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController : MonoBehaviour
{
    private Collider2D m_Collider;

    private RaycastOrigins m_RaycastOrigins;
    [SerializeField] private CollisionInfo m_Collisions;
    public CollisionInfo Collisions { get { return m_Collisions; } }

    [SerializeField] private LayerMask m_CollisionMask;
    [SerializeField][Range(2, 32)] private int m_HorizontalRayCount = 4;
    [SerializeField][Range(2, 32)] private int m_VerticalRayCount = 4;
    [SerializeField][Range(0.001f, 1.0f)] private float m_SkinWidth = 0.015f;

    [SerializeField] private float m_MaxSlopeAngle = 80.0f;
    [SerializeField] private float m_MaxDescendAngle = 80.0f;

    private float m_HorizontalRaySpacing;
    private float m_VerticalRaySpacing;

    private Vector3 m_Displacement;

    [SerializeField] private bool m_DisplayDebugRays;

    static private readonly float Epsilon = 0.00001f;

    public void Move(Vector3 displacement)
    {
        m_Displacement += displacement;
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

    public void OnUpdate()
    {
        RecalculateRaycastOrigins();
        m_Collisions.Reset();

        if (m_Displacement.y <= -Epsilon && Mathf.Abs(m_Displacement.x) >= Epsilon)
        {
            DescdendSlope();
        }

        if (Mathf.Abs(m_Displacement.x) >= Epsilon)
            HorizontalCollisions();
        if (Mathf.Abs(m_Displacement.y) >= Epsilon)
            VerticalCollisions();

        transform.Translate(m_Displacement);
        m_Displacement = Vector3.zero;
    }
    protected void HorizontalCollisions()
    {
        int xDirection = (int)Mathf.Sign(m_Displacement.x);
        float rayLength = Mathf.Abs(m_Displacement.x) + m_SkinWidth;

        for (int i = 0; i < m_HorizontalRayCount; i++)
        {
            Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * (m_HorizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, m_CollisionMask);
            Debug.DrawRay(origin, Vector2.right * xDirection * rayLength, Color.green);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= m_MaxSlopeAngle)
                {
                    float distanceToSlope = 0.0f;
                    if (slopeAngle != m_Collisions.PreviousSlopeAngle)
                    {
                        distanceToSlope = hit.distance - m_SkinWidth;
                        m_Displacement.x -= distanceToSlope * xDirection;
                    }
                    ClimbSlope(slopeAngle);
                    m_Displacement.x += distanceToSlope * xDirection;

                }

                if (!m_Collisions.ClimbingSlope || slopeAngle > m_MaxSlopeAngle)
                {
                    m_Displacement.x = (hit.distance - m_SkinWidth) * xDirection;
                    rayLength = hit.distance;

                    if (m_Collisions.ClimbingSlope)
                    {
                        m_Displacement.y = Mathf.Tan(m_Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(m_Displacement.x);
                    }

                    m_Collisions.Left = xDirection == -1;
                    m_Collisions.Right = xDirection == 1;
                }

            }

            if (Mathf.Abs(m_Displacement.x) <= Epsilon)
            {
                m_Displacement.x = 0.0f;
                break;
            }
        }
    }

    protected void VerticalCollisions()
    {
        int yDirection = (int)Mathf.Sign(m_Displacement.y);
        float rayLength = Mathf.Abs(m_Displacement.y) + m_SkinWidth;

        for (int i = 0; i < m_VerticalRayCount; i++)
        {
            Vector2 origin = yDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.TopLeft;
            origin += Vector2.right * (m_VerticalRaySpacing * i + m_Displacement.x);

            Debug.DrawRay(origin, Vector2.up * yDirection * rayLength, Color.green);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * yDirection, rayLength, m_CollisionMask);
            if (hit)
            {
                m_Displacement.y = (hit.distance - m_SkinWidth) * yDirection;
                rayLength = hit.distance;

                m_Collisions.Below = yDirection == -1;
                m_Collisions.Above = yDirection == 1;

                if (m_Collisions.ClimbingSlope && m_Collisions.Above)
                {
                    m_Displacement.x = m_Displacement.y / Mathf.Tan(m_Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(m_Displacement.x);
                }

                if (Mathf.Abs(m_Displacement.y) <= Epsilon)
                {
                    m_Displacement.y = 0.0f;
                    break;
                }
            }
        }

        //Fixes player getting in the air when going to a different slope
        if (m_Collisions.ClimbingSlope)
        {
            float xDirection = Mathf.Sign(m_Displacement.x);
            rayLength = Mathf.Abs(m_Displacement.x) + m_SkinWidth;
            Vector2 origin = (xDirection == -1) ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * m_Displacement.y;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, m_CollisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != m_Collisions.SlopeAngle)
                {
                    m_Displacement.x = (hit.distance - m_SkinWidth) * xDirection;
                    m_Collisions.SlopeAngle = slopeAngle;
                }
            }
        }
    }

    private void ClimbSlope(float slopeAngle)
    {
        float distance = Mathf.Abs(m_Displacement.x);
        float yClimbDistance = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distance;
        if (m_Displacement.y <= yClimbDistance)
        {
            m_Displacement.y = yClimbDistance;
            m_Displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(m_Displacement.x);
            m_Collisions.Below = true;
            m_Collisions.ClimbingSlope = true;
            m_Collisions.SlopeAngle = slopeAngle;
        }
    }

    private void DescdendSlope()
    {
        int xDirection = (int)Mathf.Sign(m_Displacement.x);
        Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomRight : m_RaycastOrigins.BottomLeft;
        var hit = Physics2D.Raycast(origin, Vector2.down, Mathf.Infinity, m_CollisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0.0f && slopeAngle <= m_MaxDescendAngle)
            {
                if ((int)Mathf.Sign(hit.normal.x) == xDirection)
                {
                    if (hit.distance - m_SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(m_Displacement.x))
                    {
                        float distance = Mathf.Abs(m_Displacement.x);
                        float yDescendDistance = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distance;
                        m_Displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(m_Displacement.x);
                        m_Displacement.y -= yDescendDistance;
                        m_Collisions.SlopeAngle = slopeAngle;
                        m_Collisions.Below = true;
                        m_Collisions.DescendingSlope = true;
                    }
                }
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

    [System.Serializable]
    public class CollisionInfo
    {
        public bool Above, Below;
        public bool Left, Right;
        public bool ClimbingSlope, DescendingSlope;
        public float SlopeAngle, PreviousSlopeAngle;

        public void Reset()
        {
            Above = Below = Left = Right = false;
            ClimbingSlope = DescendingSlope = false;
            PreviousSlopeAngle = SlopeAngle;
            SlopeAngle = 0.0f;
        }
    }
}