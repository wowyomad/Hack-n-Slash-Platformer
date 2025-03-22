using System;
using System.Data;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController : RaycastController
{
    [SerializeField] protected CollisionInfo m_Collisions;
    public CollisionInfo Collisions { get { return m_Collisions; } }

    [SerializeField][Range(0.01f, 90.01f)] protected float m_MaxSlopeAngle = 80.0f;
    [SerializeField][Range(0.01f, 90.01f)] protected float m_MaxDescendAngle = 80.0f;

    protected Vector3 m_Displacement = Vector3.zero;

    [SerializeField] protected bool m_DisplayDebugRays;

    static protected readonly float Epsilon = 0.00001f;

    public void Move(Vector3 displacement)
    {
        m_Displacement += displacement;
    }
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();

        m_Collisions.Reset();

        if (m_Displacement.y <= -Epsilon && Mathf.Abs(m_Displacement.x) >= Epsilon)
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

        transform.position += m_Displacement;
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
        Debug.Log($"m_Displacement: {m_Displacement.ToString("F6")}");
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