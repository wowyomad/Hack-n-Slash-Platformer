using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CharacterController2D : RaycastController
{
    [SerializeField] protected CollisionInfo m_Collisions;
    [SerializeField] protected bool m_Grounded;
    [SerializeField] protected bool m_IsFacingWallLeft;
    [SerializeField] protected bool m_IsFacingWallRight;
    [SerializeField] protected bool m_IsPassingThrough;
    [SerializeField] protected bool m_CanPasstTransparentGround;

    private bool m_StartedPassingThrough;

    public bool IsFacingWallLeft => m_IsFacingWallLeft;
    public bool IsFacingWallRight => m_IsFacingWallRight;
    public bool IsGrounded => m_Grounded;//Hack to make 'Descending' state count as Grounded. TODO: Be better.
    public bool CanPassTransparentGround => m_CanPasstTransparentGround;
    public CollisionInfo Collisions => m_Collisions;

    public bool ApplyGravity = false;
    public float Gravity = 0.0f;
    public float MaxGravityVelocity = 0.0f;

    public Vector3 LastDisplacement { get; private set; }

    [SerializeField][Range(0.01f, 90.01f)] protected float m_MaxSlopeAngle = 80.0f;
    [SerializeField][Range(0.01f, 90.01f)] protected float m_MaxDescendAngle = 80.0f;

    protected Vector3 Displacement = Vector3.zero;
    public Vector3 Velocity = Vector3.zero;

    [SerializeField] protected bool DisplayDebugRays;

    static protected readonly float EPSILON = 0.00001f;
    static protected readonly float MAGIC_RAY_MULTIPLIER = 7.5f;

    public virtual void Move(Vector3 displacement)
    {
        Displacement += displacement;
    }


    public void SetVelocity(Vector3 velocity)
    {
        Velocity = velocity;
    }

    public void PassThrough()
    {
        if (CanPassTransparentGround)
        {
            m_IsPassingThrough = true;
            m_StartedPassingThrough = false;
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

        if (ApplyGravity)
        {
            Velocity.y += Gravity * Time.deltaTime;
            Velocity.y = Mathf.Max(Velocity.y, MaxGravityVelocity);
        }

        Displacement += Velocity * Time.deltaTime;

        if (Mathf.Abs(Displacement.x) >= EPSILON)
        {
            DescdendSlope();
        }

        if (Mathf.Abs(Displacement.x) >= EPSILON)
        {
            HorizontalCollisions();
        }
        if (Mathf.Abs(Displacement.y) >= EPSILON)
        {
            VerticalCollisions();
        }

        m_Grounded = Grounded();
        m_CanPasstTransparentGround = CanPassThrough();
        (m_IsFacingWallLeft, m_IsFacingWallRight) = IsFacingWall();

        transform.position += Displacement;

        LastDisplacement = Displacement;
        Displacement = Vector3.zero;

        if ((IsGrounded && Velocity.y < 0.0f) || (m_Collisions.Above && Velocity.y > 0.0f))
        {
            Velocity.y = 0.0f;
        }

        if (m_Collisions.Left || m_Collisions.Right)
        {
            Velocity.x = 0.0f;
        }

    }
    protected void HorizontalCollisions()
    {
        int xDirection = (int)Mathf.Sign(Displacement.x);
        float rayLength = Mathf.Abs(Displacement.x) + SkinWidth;

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * (HorizontalRaySpacing * i);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, GroundMask);
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
                        Displacement.x -= distanceToSlope * xDirection;
                    }
                    ClimbSlope(slopeAngle);
                    Displacement.x += distanceToSlope * xDirection;

                }

                if (!m_Collisions.ClimbingSlope || slopeAngle > m_MaxSlopeAngle)
                {
                    Displacement.x = (hit.distance - SkinWidth) * xDirection;
                    rayLength = hit.distance;


                    if (m_Collisions.ClimbingSlope)
                    {
                        Displacement.y = Mathf.Tan(m_Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(Displacement.x);
                    }

                    m_Collisions.Left = xDirection == -1;
                    m_Collisions.Right = xDirection == 1;
                }

            }

            if (Mathf.Abs(Displacement.x) <= EPSILON)
            {
                Displacement.x = 0.0f;
                break;
            }
        }
    }

    protected void VerticalCollisions()
    {
        int yDirection = (int)Mathf.Sign(Displacement.y);
        float rayLength = Mathf.Abs(Displacement.y) + SkinWidth;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 origin = yDirection == -1 ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.TopLeft;
            origin += Vector2.right * (VerticalRaySpacing * i + Displacement.x);

            Debug.DrawRay(origin, Vector2.up * yDirection * rayLength, Color.green);

            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up * yDirection, rayLength, GetVerticalCollisionMask(yDirection));
            if (hit)
            {
                Displacement.y = (hit.distance - SkinWidth) * yDirection;
                rayLength = hit.distance;

                m_Collisions.Below = yDirection == -1;
                m_Collisions.Above = yDirection == 1;

                if (m_Collisions.ClimbingSlope && m_Collisions.Above)
                {
                    Displacement.x = Displacement.y / Mathf.Tan(m_Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(Displacement.x);
                }

                if (Mathf.Abs(Displacement.y) <= EPSILON)
                {
                    Displacement.y = 0.0f;
                    break;
                }
            }
        }

        if (m_Collisions.ClimbingSlope)
        {
            float xDirection = Mathf.Sign(Displacement.x);
            rayLength = Mathf.Abs(Displacement.x) + SkinWidth;
            Vector2 origin = (xDirection == -1) ? m_RaycastOrigins.BottomLeft : m_RaycastOrigins.BottomRight;
            origin += Vector2.up * Displacement.y;
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right * xDirection, rayLength, GroundMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != m_Collisions.SlopeAngle)
                {
                    Displacement.x = (hit.distance - SkinWidth) * xDirection;
                    m_Collisions.SlopeAngle = slopeAngle;
                }
            }
        }

        if (m_Collisions.ClimbingSlope)
        {
            m_IsPassingThrough = true;
        }

        if (m_IsPassingThrough)
        {
            Bounds bounds = m_Collider.bounds;
            bounds.Expand(-SkinWidth * 2.0f);
            Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(bounds.center, bounds.size * 0.95f, 0, TransparentGroundMask);
            if (m_StartedPassingThrough)
            {
                if (overlappingColliders.Length == 0)
                {
                    m_IsPassingThrough = false;
                }
            }
            else if (overlappingColliders.Length > 0)
            {
                m_StartedPassingThrough = true;
            }
        }
    }
    protected (bool left, bool right) IsFacingWall()
    {
        bool left = false, right = false;


        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomLeft + Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.left, SkinWidth * MAGIC_RAY_MULTIPLIER, GroundMask);
            if (hit && Vector2.Angle(hit.normal, Vector2.up) > m_MaxSlopeAngle)
            {
                left = true;
                break;
            }
        }

        for (int i = 0; i < HorizontalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomRight + Vector2.up * (HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.right, SkinWidth * MAGIC_RAY_MULTIPLIER, GroundMask);
            if (hit && Vector2.Angle(hit.normal, Vector2.up) > m_MaxSlopeAngle)
            {
                right = true;
                break;
            }
        }

        return (left, right);
    }

    private LayerMask GetVerticalCollisionMask(int yDirection)
    {
        if (yDirection == -1 && !m_IsPassingThrough)
        {
            return GroundMask | TransparentGroundMask;
        }
        return GroundMask;
    }

    protected bool CanPassThrough()
    {
        bool below = true;
        float rayLength = SkinWidth * MAGIC_RAY_MULTIPLIER;

        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomLeft + Vector2.right * (VerticalRaySpacing * i + Displacement.x);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, TransparentGroundMask);
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
        if (m_Collisions.Below) return true;

        float rayLength = MAGIC_RAY_MULTIPLIER * SkinWidth;
        for (int i = 0; i < VerticalRayCount; i++)
        {
            Vector2 origin = m_RaycastOrigins.BottomLeft + Vector2.right * (VerticalRaySpacing * i + Displacement.x);
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, rayLength, GetVerticalCollisionMask(-1));
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
        float distance = Mathf.Abs(Displacement.x);
        float yClimbDistance = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distance;
        if (Displacement.y <= yClimbDistance)
        {
            Displacement.y = yClimbDistance;
            Displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(Displacement.x);
            m_Collisions.Below = true;
            m_Collisions.ClimbingSlope = true;
            m_Collisions.SlopeAngle = slopeAngle;
        }
    }

    protected void DescdendSlope()
    {
        int xDirection = (int)Mathf.Sign(Displacement.x);
        Vector2 origin = xDirection == -1 ? m_RaycastOrigins.BottomRight : m_RaycastOrigins.BottomLeft;

        float maxRayLength = Mathf.Abs(Displacement.x) / Mathf.Cos(m_MaxDescendAngle * Mathf.Deg2Rad) + SkinWidth * MAGIC_RAY_MULTIPLIER;

        var hit = Physics2D.Raycast(origin, Vector2.down, maxRayLength, GroundMask);
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0.0f && slopeAngle <= m_MaxDescendAngle)
            {
                if ((int)Mathf.Sign(hit.normal.x) == xDirection)
                {
                    float distance = Mathf.Abs(Displacement.x);
                    float yDescendDistance = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * distance;
                    Displacement.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * distance * Mathf.Sign(Displacement.x);
                    Displacement.y -= yDescendDistance;
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