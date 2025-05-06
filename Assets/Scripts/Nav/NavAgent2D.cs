using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Nav;
using Nav2D;
using UnityEditor.MemoryProfiler;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class NavAgent2D : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavData2D m_NavData;
    [SerializeField] private NavActorSO m_NavActor;


    private float m_StuckCheckTimer = 0.0f;
    private Vector3 m_LastPosition;
    [SerializeField] private float m_StuckCheckInterval = 0.5f;
    [SerializeField] private float m_StuckThreshold = 0.01f;

    public AgentState State => m_State;
    public bool IsJumping => m_State == AgentState.Jumping;
    public bool IsFollowing => m_State >= AgentState.Moving;
    public bool IsPathReady => !m_PathPending;
    public Vector3? CurrentPathTarget => (IsPathReady && m_CurrentPointIndex < m_Path.Count) ? m_Path[m_CurrentPointIndex].Position : (Vector3?)null;

    private CharacterController2D m_Controller;
    [SerializeField] private int m_JumpDirection = 0;
    [SerializeField] private int m_CurrentPointIndex = 0;
    [SerializeField] private float m_JumpSpeedScale = 1.0f;


    private List<NavPoint> m_Path = null;
    private Vector2 m_Target = Vector2.zero;
    private AgentState m_State;
    private bool m_PathPending = false;

    private List<NavPoint> m_PathBuffer = new List<NavPoint>();
    private bool m_NewPathReady = false;
    private object m_PathLock = new object();

    static private readonly float REACH_THRESHOLD = 0.005f;

    private bool m_ShortJump = false;

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
        HandleAsyncPathResult();

        if (!IsFollowing) return;

        if (IsPathInvalid())
        {
            m_State = AgentState.Stopped;
            return;
        }

        if (IsStuck())
        {
            RecalculatePath();
            return;
        }

        if (IsJumping)
        {
            HandleJumping();
        }
        else if (m_Controller.IsGrounded)
        {
            HandleWalking();
        }
    }

    private void HandleAsyncPathResult()
    {
        if (m_NewPathReady)
        {
            lock (m_PathLock)
            {
                m_NewPathReady = false;
                HandlePathResult(m_PathBuffer);
            }
        }
    }

    private bool IsPathInvalid()
    {
        return m_Path == null || m_Path.Count == 0 || m_CurrentPointIndex >= m_Path.Count;
    }

    private void HandleJumping()
    {
        float distance = Vector2.Distance(transform.position, m_Path[m_CurrentPointIndex].Position);
        float horizontalDistance = Mathf.Abs(m_Path[m_CurrentPointIndex].Position.x - transform.position.x);

        //Костыль distance^2 вместо distance потому что потому. 
        if (distance * distance < REACH_THRESHOLD && horizontalDistance < REACH_THRESHOLD)
        {
            m_State = AgentState.Moving;
            m_JumpDirection = 0;
            m_ShortJump = false; // Reset flag
            GoNext();
        }
        else if (m_JumpDirection != 0)
        {
            // For short jump, only move horizontally after reaching peak (falling)
            if (!m_ShortJump || (m_ShortJump && m_Controller.Velocity.y <= 0))
            {
                if (horizontalDistance > REACH_THRESHOLD)
                {
                    Vector2 direction = m_JumpDirection > 0 ? Vector2.right : Vector2.left;
                    float displacement = Mathf.Min(m_JumpSpeedScale * m_NavActor.BaseSpeed * Time.deltaTime, horizontalDistance);
                    m_Controller.Move(direction * displacement);
                }
            }
        }
    }

    private void HandleWalking()
    {
        Vector3 target = m_Path[m_CurrentPointIndex].Position;
        target.y = transform.position.y;
        float distance = Vector2.Distance(transform.position, target);
        if (distance < REACH_THRESHOLD)
        {
            GoNext();
        }
        else
        {
            Vector2 direction = (target - transform.position).normalized;
            float displacement = Mathf.Min(m_NavActor.BaseSpeed * Time.deltaTime, distance);
            m_Controller.Move(direction * displacement);
        }
    }

    private void GoNext()
    {
        m_CurrentPointIndex++;
        if (m_CurrentPointIndex >= m_Path.Count)
        {
            m_State = AgentState.Stopped;
            return;
        }
        var previousPoint = m_Path[m_CurrentPointIndex - 1];
        var currentPoint = m_Path[m_CurrentPointIndex];
        var connection = GetConnectionType(previousPoint, currentPoint);
        if (connection >= ConnectionType.Jump && connection <= ConnectionType.TransparentFall)
        {
            Jump(previousPoint, currentPoint, connection);
            m_State = AgentState.Jumping;
        }
        else
        {
            m_State = AgentState.Moving;
        }
    }

    private bool IsStuck()
    {
        bool isStuck = false;
        m_StuckCheckTimer += Time.deltaTime;
        if (m_StuckCheckTimer >= m_StuckCheckInterval)
        {
            float distanceMoved = Vector3.Distance(transform.position, m_LastPosition);
            if (distanceMoved < m_StuckThreshold)
            {
                Debug.LogWarning("Agent is stuck! Recalculating path...");
                isStuck = true;
            }

            m_StuckCheckTimer = 0.0f;
            m_LastPosition = transform.position;
        }
        return isStuck;
    }

    private void Jump(NavPoint from, NavPoint to, ConnectionType connection)
    {
        if (connection < ConnectionType.Jump || connection > ConnectionType.TransparentFall)
            return;

        float jumpVelocity = m_NavActor.JumpVelocity;
        Vector2 direction = (to.Position - from.Position).normalized;

        m_JumpDirection = direction.x > 0 ? 1 : -1;
        m_JumpSpeedScale = 1.0f;

        // Detect short jump
        float jumpHorizontalDistance = Mathf.Abs(from.Position.x - to.Position.x);
        m_ShortJump = jumpHorizontalDistance <= 1.0f + float.Epsilon;
        bool flatJump = from.CellPos.y == to.CellPos.y;

        if (ConnectionType.TransparentFall == connection)
        {
            m_Controller.PassThrough();
            return;
        }
        else if (ConnectionType.Fall == connection)
        {
            jumpVelocity /= 4.0f;
        }
        else
        {
            if (flatJump)
            {
                jumpVelocity *= jumpHorizontalDistance  / m_NavActor.MaxJumpDistance;
            }
            else
            {
                float jumpHeight = Mathf.Abs(to.Position.y - from.Position.y);
                jumpVelocity *= Mathf.Sqrt((jumpHeight + 1.0f) / m_NavActor.MaxJumpHeight);
            }

            m_JumpSpeedScale = m_NavActor.MaxJumpDistance / jumpHorizontalDistance;
        }

        m_Controller.Velocity.y = jumpVelocity;
    }

    public void SetDestination(Vector2 target)
    {
        if (m_NavData == null)
            throw new NullReferenceException("NavData2D is not assigned");

        m_Target = target;
        m_Path = m_Path ?? new List<NavPoint>();
        var path = m_NavData.GetPath(transform.position, target, m_Path);
        HandlePathResult(path);
    }

    public void SetDestinationAsync(Vector2 target)
    {
        if (m_NavData == null)
            throw new NullReferenceException("NavData2D is not assigned");

        if (m_PathPending) return;

        m_PathPending = true;
        m_Target = target;
        Vector2 position = transform.position;

        NavPoint startPoint = m_NavData.GetClosestNavPoint(position);
        NavPoint endPoint = m_NavData.GetClosestNavPoint(target);

        Task.Run(() =>
        {
            try
            {
                // Only lock when accessing the buffer, not during pathfinding!
                List<NavPoint> localBuffer;
                lock (m_PathLock)
                {
                    m_PathBuffer.Clear();
                    localBuffer = m_PathBuffer;
                }

                // Do pathfinding OUTSIDE the lock
                m_NavData.GetPath_ThreadSafe(startPoint, endPoint, position, target, localBuffer);

                // Now lock again to set flags and signal result
                lock (m_PathLock)
                {
                    m_NewPathReady = true;
                    m_PathPending = false;
                }
            }
            catch (Exception e)
            {
                lock (m_PathLock)
                {
                    m_PathPending = false;
                }
                Debug.LogError("Error while calculating path: " + e.Message);
            }
        });
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
        SetDestination(m_Target);
    }

    private ConnectionType GetConnectionType(NavPoint a, NavPoint b)
    {
        var connection = a.Connections.Find(conn =>
            conn.Point.CellPos == b.CellPos);
        if (connection != null)
        {
            return connection.Type;
        }
        return ConnectionType.None;
    }

    private void HandlePathResult(List<NavPoint> path)
    {
        if (path == null || path.Count == 0)
        {
            m_State = AgentState.Stopped;
            m_Path = path;
        }
        else
        {
            m_State = AgentState.Moving;
            m_Path = path;
            m_CurrentPointIndex = 0;
        }
    }


    public enum AgentState
    {
        None,
        Stopped,
        Moving,
        Jumping,
    }
}
