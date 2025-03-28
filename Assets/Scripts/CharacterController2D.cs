using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CharacterController2D : RaycastController
{
    [SerializeField] protected CollisionInfo m_Collisions;
    [SerializeField] protected bool m_IsGrounded;
    [SerializeField] protected bool m_IsFacingWallLeft;
    [SerializeField] protected bool m_IsFacingWallRight;
    [SerializeField] protected bool m_IsPassingThrough;
    [SerializeField] protected bool m_CanPassthroughGround;
    public bool IsFacingWallLeft => m_IsFacingWallLeft;
    public bool IsFacingWallRight => m_IsFacingWallRight;
    public bool IsGrounded => m_IsGrounded || Collisions.DescendingSlope ;//Hack to make 'Descending' state count as Grounded. TODO: Be better.
    public bool CanPassthroughGround => m_CanPassthroughGround;
    public CollisionInfo Collisions => m_Collisions;

    [SerializeField][Range(0.01f, 90.01f)] protected float m_MaxSlopeAngle = 80.0f;
    [SerializeField][Range(0.01f, 90.01f)] protected float m_MaxDescendAngle = 80.0f;

    protected Vector3 m_Displacement = Vector3.zero;

    [SerializeField] protected bool m_DisplayDebugRays;

    static protected readonly float Epsilon = 0.00001f;

    public virtual void Move(Vector3 displacement)
    {
        m_Displacement += displacement;
    }
    
    public void PassThrough()
    {
        if (CanPassthroughGround)
        {
            m_IsPassingThrough = true;
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        m_Collisions.Reset();

        if (Mathf.Abs(m_Displacement.x) >= Epsilon)
        {
            DescdendSlope();
        }

        if (Mathf.Abs(m_Displacement.x) >= Epsilon)
        {
            HorizontalCollisions();
        }
        if (Mathf.Abs(m_Displacement.y) >= Epsilon)
        {
            VerticalCollisions();
        }

        m_IsGrounded = Grounded();
        m_CanPassthroughGround = CanPassThrough();
        (m_IsFacingWallLeft, m_IsFacingWallRight) = IsFacingWall();

        transform.position += m_Displacement;
        m_Displacement = Vector3.zero;
    }
    protected void HorizontalCollisions()
    {
        int xDirection = (int)Mathf.Sign(m_Displacement.x);
        float rayLength = Mathf.Abs(m_Displacement.x) + SkinWidth;

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * (HorizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, CollisionMask);
            Debug.DrawRay(origin, Vector2.right * xDirection * rayLength, Color.green);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (i == 0 && slopeAngle <= m_MaxSlopeAngle)
                {
                    float distanceToSlope = 0.0f;
                    if (slopeAngle != m_Collisions.PreviousSlopeAngle)
                    {
                        distanceToSlope = hit.distance - SkinWidth;
                        m_Displacement.x -= distanceToSlope * xDirection;
                    }
                    ClimbSlope(slopeAngle);
                    m_Displacement.x += distanceToSlope * xDirection;

                }

                if (!m_Collisions.ClimbingSlope || slopeAngle > m_MaxSlopeAngle)
                {
                    m_Displacement.x = (hit.distance - SkinWidth) * xDirection;
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
        float rayLength = Mathf.Abs(m_Displacement.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 origin = yDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.TopLeft;
            origin += Vector2.right * (VerticalRaySpacing * i + m_Displacement.x);

            Debug.DrawRay(origin, Vector2.up * yDirection * rayLength, Color.green);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * yDirection, rayLength, GetCollisionMask(yDirection));
            if (hit)
            {
                m_Displacement.y = (hit.distance - SkinWidth) * yDirection;
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

        if (m_Collisions.ClimbingSlope)
        {
            float xDirection = Mathf.Sign(m_Displacement.x);
            rayLength = Mathf.Abs(m_Displacement.x) + SkinWidth;
            Vector2 origin = (xDirection == -1) ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * m_Displacement.y;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, CollisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != m_Collisions.SlopeAngle)
                {
                    m_Displacement.x = (hit.distance - SkinWidth) * xDirection;
                    m_Collisions.SlopeAngle = slopeAngle;
                }
            }
        }

        if (m_IsPassingThrough)
        {
            Bounds bounds = m_Collider.bounds;
            bounds.Expand(-SkinWidth * 2.0f);
            Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0, PassThroughMask);
            if (overlappingColliders.Length == 0)
            {
                m_IsPassingThrough = false;
            }
        }
    }
    protected (bool left, bool right) IsFacingWall()
    {
        bool left = false, right = false;


        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomLeft + Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.left, SkinWidth * 1.1f, CollisionMask);
            if (hit && Vector2.Angle(hit.normal, Vector2.up) > m_MaxSlopeAngle)
            {
                left = true;
                break;
            }
        }

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomRight + Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, SkinWidth * 1.1f, CollisionMask);
            if (hit && Vector2.Angle(hit.normal, Vector2.up) > m_MaxSlopeAngle)
            {
                right = true;
                break;
            }
        }

        return (left, right);
    }

    private LayerMask GetCollisionMask(int yDirection)
    {
        if (yDirection == -1 && !m_IsPassingThrough)
        {
            return CollisionMask | PassThroughMask;
        }
        return CollisionMask;
    }

    protected bool CanPassThrough()
    {
        bool below = true;
        float rayLength = SkinWidth * 1.15f;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomLeft + Vector2.right * (VerticalRaySpacing * i + m_Displacement.x);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, PassThroughMask);
            Debug.DrawRay(origin, Vector2.down * rayLength, Color.blue);
            if (!hit)
            {
                below = false;
                break;
            }
        }
        return below;
    }

    protected bool Grounded()
    {
        float rayLength = 1.15f * SkinWidth + m_Displacement.y;
        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomLeft + Vector2.right * (VerticalRaySpacing * i + m_Displacement.x);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, GetCollisionMask(-1));
            Debug.DrawRay(origin, Vector2.down * rayLength, Color.yellow);
            if (hit)
            {
                return true;
            }
        }
        return false;
    }

    protected void ClimbSlope(float slopeAngle)
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

    protected void DescdendSlope()
    {
        int xDirection = (int)Mathf.Sign(m_Displacement.x);
        Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomRight : m_RaycastOrigins.BottomLeft;

        float maxRayLength = Mathf.Abs(m_Displacement.x) / Mathf.Cos(m_MaxDescendAngle * Mathf.Deg2Rad) + SkinWidth * 1.15f;

        var hit = Physics2D.Raycast(origin, Vector2.down, maxRayLength, CollisionMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0.0f && slopeAngle <= m_MaxDescendAngle)
            {
                if ((int)Mathf.Sign(hit.normal.x) == xDirection)
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