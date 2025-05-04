using System;
using System.Collections.Generic;
using Nav;
using Nav2D;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class NavAgent2D : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavData2D m_NavData;
    [SerializeField] private NavActorSO m_NavActor;


    [Header("Debug")]
    [SerializeField] private float m_DebugDrawDuration = 5.0f;


    private float m_StuckCheckTimer = 0.0f;
    private Vector3 m_LastPosition;
    [SerializeField] private float m_StuckCheckInterval = 0.5f;
    [SerializeField] private float m_StuckThreshold = 0.01f;


    public bool IsJumping => m_Jumping;
    public bool IsFollowing => m_Following;
    public bool IsPathReady => PathCalculated && m_Path != null && m_Path.Count > 0;
    public Vector3? CurrentPathTarget => (IsPathReady && m_CurrentPointIndex < m_Path.Count) ? m_Path[m_CurrentPointIndex].Position : (Vector3?)null;
    public bool PathCalculated { get; private set; } = false;


    private CharacterController2D m_Controller;
    [SerializeField] private bool m_Following = false;
    [SerializeField] private bool m_Jumping = false;
    [SerializeField] private int m_JumpDirection = 0;


    private List<NavPoint> m_Path = null;
    [SerializeField] private int m_CurrentPointIndex = 0;
    private Vector2 m_Target = Vector2.zero;

    private ActionTimer m_PathfindingTimer = new(true, false);

    static private readonly float REACH_THRESHOLD = 0.005f;

    private void Awake()
    {
        if (m_NavData == null)
        {
            throw new NullReferenceException("NavData2D is not assigned");
        }
        if (m_NavActor == null)
        {
            throw new NullReferenceException("NavActor is not assigned");
        }

        m_Controller = GetComponent<CharacterController2D>();
    }


    private void Start()
    {
        m_Controller.Gravity = m_NavActor.Gravity;
        m_Controller.MaxGravityVelocity = m_NavActor.MaxGravityVelocity;
        m_Controller.ApplyGravity = true;
    }

    private void Update()
    {
        if (!m_Following) return;

        if (m_Path == null || m_Path.Count == 0 || m_CurrentPointIndex >= m_Path.Count)
        {
            m_Following = false;
            return;
        }

        m_StuckCheckTimer += Time.deltaTime;
        if (m_StuckCheckTimer >= m_StuckCheckInterval)
        {
            // Check distance moved over the interval
            float distanceMoved = Vector3.Distance(transform.position, m_LastPosition);
            if (distanceMoved < m_StuckThreshold)
            {
                Debug.LogWarning("Agent is stuck! Recalculating path...");
                RecalculatePath();
            }

            // Reset the timer and update the last position
            m_StuckCheckTimer = 0.0f;
            m_LastPosition = transform.position;
        }

        if (m_Jumping)
        {
            float distance = Vector2.Distance(transform.position, m_Path[m_CurrentPointIndex].Position);
            float horizontalDistance = Mathf.Abs(m_Path[m_CurrentPointIndex].Position.x - transform.position.x);

            if (distance * distance < REACH_THRESHOLD && horizontalDistance < REACH_THRESHOLD)
            {
                m_Jumping = false;
                m_JumpDirection = 0;
                m_CurrentPointIndex++;
                if (m_CurrentPointIndex >= m_Path.Count)
                {
                    m_Following = false;
                    return;
                }
            }
            if (m_JumpDirection != 0)
            {
                if (horizontalDistance > REACH_THRESHOLD)
                {
                    Debug.Log("Horizontal distance: " + horizontalDistance);
                    Vector2 direction = m_JumpDirection > 0 ? Vector2.right : Vector2.left;
                    float displacement = Mathf.Min(m_NavActor.BaseSpeed * Time.deltaTime, horizontalDistance);
                    m_Controller.Move(direction * displacement);
                }
            }
            return;
        }

        if (m_Controller.IsGrounded && !m_Jumping)
        {
            Vector3 target = m_Path[m_CurrentPointIndex].Position;
            target.y = transform.position.y;
            Vector2 direction = (target - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, target);
            if (distance < REACH_THRESHOLD)
            {
                m_CurrentPointIndex++;
                if (m_CurrentPointIndex >= m_Path.Count)
                {
                    m_Following = false;
                    return;
                }
                else
                {
                    var previousPoint = m_Path[m_CurrentPointIndex - 1];
                    var currentPoint = m_Path[m_CurrentPointIndex];

                    var connection = previousPoint.Connections.Find(conn =>
                        conn.Point.CellPos == currentPoint.CellPos);

                    if (connection != null)
                    {
                        if (connection.Type >= ConnectionType.Jump && connection.Type <= ConnectionType.TransparentFall)
                        {
                            m_Jumping = true;
                            Debug.Log($"Jump found between {previousPoint.CellPos} and {currentPoint.CellPos} ({connection.Type})");
                            Jump(previousPoint, currentPoint, connection.Type);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No valid connection found between {previousPoint.CellPos} and {currentPoint.CellPos}");
                    }
                }
            }
            else
            {
                float displacement = Mathf.Min(m_NavActor.BaseSpeed * Time.deltaTime, distance);
                m_Controller.Move(direction * displacement);
            }
        }
    }

    private void Jump(NavPoint from, NavPoint to, ConnectionType connection)
    {
        float jumpVelocity = m_NavActor.JumpVelocity;
        float maxJumpHeight = m_NavActor.MaxJumpHeight;
        Vector2 direction = (to.Position - from.Position).normalized;

        m_JumpDirection = direction.x > 0 ? 1 : -1;

        if (ConnectionType.TransparentFall == connection)
        {
            m_Controller.PassThrough();
            return;
        }
        else if (ConnectionType.Fall == connection)
        {
            jumpVelocity /= 4.0f;
        }

        m_Controller.Velocity.y = jumpVelocity;
    }

    public NavPoint GetClosestNavpoint(Vector2 target)
    {
        return m_NavData.GetClosestNavPoint(target);
    }

    public void SetDestination(Vector2 target)
    {
        if (m_NavData == null)
        {
            throw new NullReferenceException("NavData2D is not assigned");
        }
        m_Path = m_NavData.GetPath(transform.position, target);
        m_Target = target;
        PathCalculated = m_Path != null && m_Path.Count > 0;
        m_CurrentPointIndex = 0;
        m_Following = true;
        m_Jumping = false;
    }

    private void OnDrawGizmos()
    {
        if (m_Path != null && m_Path.Count > 1)
        {
            Gizmos.color = Color.cyan;
            int startIndex = m_CurrentPointIndex;


            for (int i = startIndex; i < m_Path.Count - 1; i++)
            {
                GizmosEx.DrawArrow(m_Path[i].Position, m_Path[i + 1].Position, Color.green);
            }

            if (m_Path != null && m_CurrentPointIndex < m_Path.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(m_Path[m_CurrentPointIndex].Position, 0.3f);
            }
        }
    }
    private void RecalculatePath()
    {
        m_Path = m_NavData.GetPath(transform.position, m_Target);
        PathCalculated = m_Path != null && m_Path.Count > 0;
        m_CurrentPointIndex = 0;
        m_Jumping = false;
    }
}
