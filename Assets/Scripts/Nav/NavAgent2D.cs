using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nav;
using Nav2D;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
[RequireComponent(typeof(Collider2D))]
public class NavAgent2D : MonoBehaviour
{
    public enum AgentState
    {
        None,
        Stopped,
        Moving,
        Jumping,
    }

    [Header("Components")]
    [SerializeField] private NavData2D m_NavData;
    [SerializeField] private NavActorSO m_NavActor;
    [SerializeField] private Animator m_Animator;
    private Collider2D m_Collider;

    [Header("Animation Triggers")]
    [SerializeField] private string m_AnimationTriggerIdle = "Idle";
    [SerializeField] private string m_AnimationTriggerWalk = "Walk";
    [SerializeField] private string m_AnimationTriggerRun = "Run";
    [SerializeField] private string m_AnimationTriggerJump = "Jump";


    [Header("Movement")]
    public bool OverrideSpeed = false;
    public float Speed = 0.0f;
    public float DeccelerationTime = 0.5f;
    public float AccelerationTime = 0.5f;
    public bool TurnAgentOnMove = true;
    public bool InvalidPath { get; private set; } = false;

    public bool IsPathPending => m_NewPathPending || m_NewPathReady;


    [Header("Pathfinding")]
    public bool IsAsync = false;
    public bool DontChangePathWhileSloping = true;
    private float m_StuckCheckTimer = 0.0f;
    private Vector3 m_LastPosition;
    [SerializeField] private float m_StuckCheckInterval = 0.5f;
    [SerializeField] private float m_StuckThreshold = 0.01f;

    private CharacterController2D m_Controller;
    [SerializeField] private int m_JumpDirection = 0;
    [SerializeField] private int m_CurrentPointIndex = 0;
    [SerializeField] private float m_JumpSpeedScale = 1.0f;


    [Header("Internal")]

    private Action<int> m_TurnCallback;

    private List<NavData2D.NavPoint> m_Path;
    [SerializeField] private Vector2 m_Target = Vector2.zero;
    private AgentState m_State;
    [SerializeField] private bool m_NewPathPending = false;


    private List<NavData2D.NavPoint> m_PathBuffer;
    [SerializeField] private bool m_NewPathReady = false;
    [SerializeField] private bool m_DismissPath = false;
    private object m_PathLock = new object();

    static private readonly float REACH_THRESHOLD = 0.005f;

    private bool m_ShortJump = false;
    private AgentState m_PreviousState = AgentState.None;
    [SerializeField] private bool m_PassingThrough;
    [SerializeField] private bool m_HasEnteredTransparentGround = false;
    [SerializeField] private bool m_HasJumped = false;

    public AgentState State => m_State;
    public bool IsJumping => m_State == AgentState.Jumping;
    public bool IsMoving => m_State == AgentState.Moving;
    public bool IsFollowing => m_State != AgentState.None && m_State != AgentState.Stopped;
    public bool IsPathReady => !m_NewPathPending;
    public Vector3? CurrentPathTarget => (IsPathReady && m_Path != null && m_CurrentPointIndex < m_Path.Count) ? m_Path[m_CurrentPointIndex].Position : (Vector3?)null;
    public Vector2 Velocity { get; private set; }

    private Vector3? m_OverrideCurrentTarget = null;


    [Header("Debug")]
    [SerializeField]
    private bool m_DrawPath = true;

    private void SetAgentState(AgentState newState)
    {
        if (m_State == newState)
            return;

        m_PreviousState = m_State;
        m_State = newState;

        return;
    }

    private void ResetTemps()
    {

        m_PassingThrough = false;
        m_HasEnteredTransparentGround = false;
        m_HasJumped = false;
    }

    public bool IsInsideTransparentGround()
    {
        if (m_NavData.GetCell(transform.position, out var cell, false))
        {
            return cell.Transparent != null;
        }
        return false;
    }

    public void SetSpeed(float speed)
    {
        OverrideSpeed = true;
        Speed = speed;
    }

    public void SetTurnCallback(Action<int> turnCallback)
    {
        m_TurnCallback = turnCallback;
    }

    public void SetAccelerationTime(float time)
    {
        AccelerationTime = time;
    }

    public void SetDeccelerationTime(float time)
    {
        DeccelerationTime = time;
    }

    public void SetDestination(Vector2 target, Vector2? fallback = null)
    {
        if (IsMoving && DontChangePathWhileSloping && m_Controller.Collisions.ClimbingSlope)
        {
            StartCoroutine(WaitForSlopeAndSetDestination(target, fallback));
            return;
        }
        else if (IsJumping)
        {
            StartCoroutine(WaitForJumpAndSetDestination(target, fallback));
            return;
        }

        InvalidPath = false;

        if (IsAsync)
        {
            SetDestinationAsyncInternal(target, fallback);
        }
        else
        {
            SetDestinationInternal(target);
        }
    }

    private void SetDestinationInternal(Vector2 target)
    {
        if (m_PassingThrough) return;

        if (m_NavData == null)
            throw new NullReferenceException("NavData2D is not assigned");

        m_NavData.GetPath(transform.position, target, m_Path);
        if (m_Path != null && m_Path.Count > 0)
        {
            bool correctPath = ApplyNewPath(m_Path);
            if (correctPath)
            {
                AdjustCurrentPathIndexForAgentPosition();
            }
            else
            {
                InvalidPath = true;
            }
        }
    }

    private void SetDestinationAsyncInternal(Vector2 target, Vector2? fallback)
    {
        if (m_PassingThrough) return;

        if (m_NavData == null)
            throw new NullReferenceException("NavData2D is not assigned");

        if (m_NewPathPending) return;

        m_NewPathPending = true;
        m_DismissPath = false;
        Vector3 colliderOffset = m_Collider.offset;
        bool isInsideTransparentGround = m_NavData.GetCellsInArea(transform.position + colliderOffset, m_Collider.bounds, out var cells) && cells.Any(cell => cell.Transparent != null);
        float verticalOffset = isInsideTransparentGround ? m_NavData.ActorSize.y * 0.5f : 0.0f;
        Vector2 currentPosition = transform.position - new Vector3(0, verticalOffset, 0);

        NavData2D.NavPoint startPoint = m_NavData.GetClosestNavPoint(currentPosition, isInsideTransparentGround || m_Controller.Collisions.ClimbingSlope || m_Controller.Collisions.DescendingSlope ? true : false);
        NavData2D.NavPoint endPoint = m_NavData.GetClosestNavPoint(target);
        m_Target = target;

        if (endPoint == null && fallback != null)
        {
            endPoint = m_NavData.GetClosestNavPoint(fallback.Value);
            m_Target = fallback.Value;
        }

        Task.Run(() =>
        {
            bool calculationDone = false;
            try
            {

                lock (m_PathLock)
                {

                    if (m_DismissPath)
                    {
                        m_NewPathPending = false;
                        m_DismissPath = false;
                        return;
                    }
                    m_PathBuffer.Clear();
                }

                m_NavData.GetPath_ThreadSafe(startPoint, endPoint, currentPosition, target, m_PathBuffer);
                calculationDone = true;

                lock (m_PathLock)
                {
                    if (!m_DismissPath)
                    {
                        m_NewPathReady = true;
                    }
                    else
                    {
                        m_PathBuffer.Clear();
                        m_NewPathReady = false;
                    }
                    m_NewPathPending = false;
                    m_DismissPath = false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error while calculating path: " + e.Message + "\n" + e.StackTrace);
                lock (m_PathLock)
                {

                    if (calculationDone)
                    {
                        m_PathBuffer.Clear();
                    }
                    m_NewPathPending = false;
                    m_NewPathReady = false;
                    m_DismissPath = false;
                }
            }
        });
    }

    private System.Collections.IEnumerator WaitForJumpAndSetDestination(Vector2 target, Vector2? fallback)
    {
        m_NewPathPending = true;
        while (IsJumping)
        {
            yield return null;
        }
        m_NewPathPending = false;
        SetDestination(target, fallback);
    }

    private System.Collections.IEnumerator WaitForSlopeAndSetDestination(Vector2 target, Vector2? fallback)
    {
        m_NewPathPending = true;
        while (m_Controller.Collisions.ClimbingSlope)
        {
            yield return null;
        }
        m_NewPathPending = false;
        SetDestination(target, fallback);
    }

    private void Awake()
    {
        if (m_Animator == null)
        {
            m_Animator = GetComponent<Animator>();
            if (m_Animator == null)
            {
                m_Animator = GetComponentInChildren<Animator>();
            }
        }

        if (!m_NavData)
        {
            m_NavData = FindFirstObjectByType<NavData2D>(FindObjectsInactive.Include);
            if (!m_NavData)
            {
                throw new NullReferenceException("NavData2D is missing");
            }
        }
        if (m_NavActor == null)
        {
            throw new NullReferenceException("NavActor is not assigned");
        }
        m_Collider = GetComponent<Collider2D>();
        m_Controller = GetComponent<CharacterController2D>();
        m_Path = new List<NavData2D.NavPoint>();
        m_PathBuffer = new List<NavData2D.NavPoint>();
    }

    private void Start()
    {
        m_Controller.Gravity = m_NavActor.Gravity;
        m_Controller.MaxGravityVelocity = m_NavActor.MaxGravityVelocity;
        m_Controller.ApplyGravity = true;
    }

    private void Update()
    {
        if (m_Controller.LastDisplacement.x > 0)
        {
            Velocity = new Vector2(OverrideSpeed ? Speed : m_NavActor.BaseSpeed, m_Controller.Velocity.y);
        }
        else if (m_Controller.LastDisplacement.x < 0)
        {
            Velocity = new Vector2(OverrideSpeed ? -Speed : -m_NavActor.BaseSpeed, m_Controller.Velocity.y);
        }
        else
        {
            Velocity = new Vector2(0, m_Controller.Velocity.y);
        }

        if (m_State == AgentState.Stopped)
        {
            m_Animator.SetInteger("Speed", 0);
        }
        else
        {
            int speed = OverrideSpeed ? (int)Speed : (int)m_NavActor.BaseSpeed;
            m_Animator.SetInteger("Speed", speed);
        }

        if (HandleAsyncPathResult())
        {
            return;
        }


        if (IsPathInvalid())
        {
            SetAgentState(AgentState.Stopped);
            m_PassingThrough = false;
            return;
        }

        if (!IsFollowing)
        {
            return;
        }

        if (IsStuck())
        {
            RecalculatePath();
            return;
        }

        HandlePassingThrough();


        if (IsJumping)
        {
            HandleJumping();
        }
        else if (m_Controller.IsGrounded)
        {
            HandleWalking();
        }



        if (TurnAgentOnMove && Velocity.x != 0.0f)
        {
            m_TurnCallback?.Invoke(Velocity.x > 0 ? 1 : -1);
        }


    }

    private void HandlePassingThrough()
    {
        if (m_PassingThrough)
        {
            if (m_Controller.CanPassTransparentGround)
            {
                m_Controller.ClimbDown();
            }
            if (m_NavData.GetCell(transform.position, out var cell, false))
            {
                if (!m_HasEnteredTransparentGround)
                {
                    if (cell.Transparent != null)
                    {
                        m_HasEnteredTransparentGround = true;
                    }
                }
                else if (cell.Transparent == null || m_Controller.IsGrounded)
                {
                    m_PassingThrough = false;
                }
            }
        }
    }

    private bool HandleAsyncPathResult()
    {
        if (m_DismissPath)
        {
            m_NewPathPending = false;
            m_NewPathReady = false;
            m_DismissPath = false;
            return false;
        }
        else
        {
            bool correctPath = false;
            if (m_NewPathReady)
            {
                lock (m_PathLock)
                {
                    if (!m_NewPathReady) return false;
                    m_NewPathReady = false;

                    List<NavData2D.NavPoint> temp = m_Path;
                    m_Path = m_PathBuffer;
                    m_PathBuffer = temp;

                    correctPath = ApplyNewPath(m_Path);
                    if (correctPath)
                    {
                        AdjustCurrentPathIndexForAgentPosition();
                    }
                    else
                    {
                        InvalidPath = true;
                    }
                }
            }
            return correctPath;
        }


    }

    private bool ApplyNewPath(List<NavData2D.NavPoint> newPath)
    {
        //TODO: set Jump state as well when appropriate
        //TODO: don't even set state here
        if (newPath == null || newPath.Count == 0)
        {
            SetAgentState(AgentState.Stopped);
            m_Path = newPath;
            m_CurrentPointIndex = 0;
            return false;
        }
        else
        {
            SetAgentState(AgentState.Moving);
            m_Path = newPath;
            m_Target = m_Path[m_Path.Count - 1].Position;
            m_CurrentPointIndex = 0;
            return true;
        }
    }

    private void AdjustCurrentPathIndexForAgentPosition()
    {
        if (m_Path == null || m_Path.Count <= 1 || !IsFollowing)
        {
            return;
        }

        while (m_CurrentPointIndex < m_Path.Count - 1)
        {
            Vector3 agentPos = transform.position;
            NavData2D.NavPoint currentNavPoint = m_Path[m_CurrentPointIndex];
            NavData2D.NavPoint nextNavPoint = m_Path[m_CurrentPointIndex + 1];
            Vector3 currentTargetPos = currentNavPoint.Position;
            Vector3 nextTargetPos = nextNavPoint.Position;

            float distanceToCurrent = Vector3.Distance(agentPos, currentTargetPos);

            NavData2D.ConnectionType connectionToNext = GetConnectionType(currentNavPoint, nextNavPoint);
            bool isNextSegmentAJump = connectionToNext >= NavData2D.ConnectionType.Jump && connectionToNext <= NavData2D.ConnectionType.TransparentFall;

            if (isNextSegmentAJump)
            {
                // Only advance if we're truly at the jump point
                if (distanceToCurrent < REACH_THRESHOLD)
                {
                    m_CurrentPointIndex++;
                }
                // Otherwise, stop here and let the jump logic handle it
                break;
            }

            if (distanceToCurrent < REACH_THRESHOLD)
            {
                m_CurrentPointIndex++;
                continue;
            }

            Vector3 segmentDirection = nextTargetPos - currentTargetPos;
            Vector3 agentRelativeToCurrent = agentPos - currentTargetPos;

            if (segmentDirection.sqrMagnitude <= float.Epsilon * float.Epsilon)
            {
                m_CurrentPointIndex++;
                continue;
            }

            float projection = Vector3.Dot(agentRelativeToCurrent, segmentDirection.normalized);
            if (projection > float.Epsilon)
            {
                m_CurrentPointIndex++;
                continue;
            }

            break;
        }

        if (m_Path != null && m_Path.Count > 0 && m_CurrentPointIndex >= m_Path.Count)
        {
            m_CurrentPointIndex = m_Path.Count - 1;
        }
    }


    private bool IsPathInvalid()
    {
        return m_Path == null || m_Path.Count == 0 || m_CurrentPointIndex >= m_Path.Count;
    }

    private void HandleJumping()
    {
        if (m_Path == null || m_CurrentPointIndex >= m_Path.Count)
            return;

        Vector2 targetPos = m_OverrideCurrentTarget ?? m_Path[m_CurrentPointIndex].Position;

        if (m_HasJumped)
        {
            if (m_Controller.IsGrounded)
            {
                GoNext();
                return;
            }
        }
        else
        {
            m_HasJumped = true;
        }

        float distance = Vector2.Distance(transform.position, targetPos);
        float horizontalDistance = Mathf.Abs(targetPos.x - transform.position.x);
        float verticalDistance = targetPos.y - transform.position.y;

        if (distance * distance < REACH_THRESHOLD && horizontalDistance < REACH_THRESHOLD)
        {
            m_JumpDirection = 0;
            m_ShortJump = false;
            m_OverrideCurrentTarget = null; // Clear after reaching
            GoNext();
        }
        else if (m_JumpDirection != 0)
        {
            // Only apply horizontal movement if we've reached or exceeded the target Y (for upward jumps)
            bool canMoveHorizontally = true;
            if (verticalDistance > 0) // Jumping up
            {
                canMoveHorizontally = (transform.position.y >= targetPos.y) && (m_Controller.Velocity.y <= 0);
            }
            // For downward or flat jumps, allow horizontal movement as before

            if (canMoveHorizontally)
            {
                if (!m_ShortJump || (m_ShortJump && m_Controller.Velocity.y <= 0))
                {
                    if (horizontalDistance > REACH_THRESHOLD)
                    {
                        Vector2 direction = m_JumpDirection > 0 ? Vector2.right : Vector2.left;
                        float displacement = Mathf.Min(m_JumpSpeedScale * (OverrideSpeed ? Speed : m_NavActor.BaseSpeed) * Time.deltaTime, horizontalDistance);
                        m_Controller.Move(direction * displacement);
                    }
                }
            }
        }
    }

    private void HandleWalking()
    {
        if (m_Path == null || m_CurrentPointIndex >= m_Path.Count) return;

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
            float displacement = Mathf.Min((OverrideSpeed ? Speed : m_NavActor.BaseSpeed) * Time.deltaTime, distance);
            m_Controller.Move(direction * displacement);
        }
    }

    private void GoNext()
    {
        ResetTemps();
        m_CurrentPointIndex++;
        if (m_Path == null || m_CurrentPointIndex >= m_Path.Count)
        {
            SetAgentState(AgentState.Stopped);
        }
        else
        {
            var previousPoint = m_Path[m_CurrentPointIndex - 1];
            var currentPoint = m_Path[m_CurrentPointIndex];
            var connection = GetConnectionType(previousPoint, currentPoint);
            if (connection >= NavData2D.ConnectionType.Jump && connection <= NavData2D.ConnectionType.TransparentFall)
            {
                Jump(previousPoint, currentPoint, connection);
                SetAgentState(AgentState.Jumping);
            }
            else
            {
                if (connection == NavData2D.ConnectionType.Slope)
                {
                    float dy = currentPoint.Position.y - previousPoint.Position.y;
                    if (dy < -0.001f)
                    {
                        m_PassingThrough = true;
                    }

                }
                SetAgentState(AgentState.Moving);
            }
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
                isStuck = true;
            }
            m_StuckCheckTimer = 0.0f;
            m_LastPosition = transform.position;
        }
        return isStuck;
    }

    private void Jump(NavData2D.NavPoint from, NavData2D.NavPoint to, NavData2D.ConnectionType connection)
    {
        if (connection < NavData2D.ConnectionType.Jump || connection > NavData2D.ConnectionType.TransparentFall)
            return;

        m_OverrideCurrentTarget = null; // Reset by default

        if (connection == NavData2D.ConnectionType.Fall || connection == NavData2D.ConnectionType.TransparentFall)
        {
            Vector2 direction = (to.Position - from.Position).normalized;
            m_JumpDirection = direction.x > 0 ? 1 : -1;
            m_JumpSpeedScale = 1.0f;

            float overshoot = 0.3f;
            Vector2 overshootTarget = to.Position + new Vector2(m_JumpDirection * overshoot, 0f);

            m_OverrideCurrentTarget = overshootTarget; // Use this for movement, do not modify NavPoint

            return;
        }

        // For upward/normal jumps, start the jump a bit before the actual jump point
        Vector2 jumpDir = (to.Position - from.Position).normalized;
        m_JumpDirection = jumpDir.x > 0 ? 1 : -1;
        m_JumpSpeedScale = 1.0f;

        float preJumpOffset = 0.5f;
        Vector2 preJumpTarget = from.Position - new Vector2(m_JumpDirection * preJumpOffset, 0f);

        m_OverrideCurrentTarget = preJumpTarget; // Use this for movement, do not modify NavPoint

        float jumpVelocity = m_NavActor.JumpVelocity;
        float jumpHorizontalDistance = Mathf.Abs(from.Position.x - to.Position.x);
        m_ShortJump = jumpHorizontalDistance <= 1.0f + float.Epsilon;
        bool flatJump = from.CellPos.y == to.CellPos.y;

        if (flatJump)
        {
            jumpVelocity *= 0.75f * jumpHorizontalDistance / m_NavActor.MaxJumpDistance;
        }
        else
        {
            float jumpHeight = Mathf.Abs(to.Position.y - from.Position.y);
            jumpVelocity *= Mathf.Sqrt((jumpHeight + 1.0f) / m_NavActor.MaxJumpHeight);
        }
        if (!m_ShortJump)
            m_JumpSpeedScale = m_NavActor.MaxJumpDistance / jumpHorizontalDistance;

        m_Controller.Velocity = new Vector2(m_Controller.Velocity.x, jumpVelocity);
    }

    private void RecalculatePath()
    {
        if (IsFollowing)
        {
            SetDestination(m_Target);
        }
    }

    private NavData2D.ConnectionType GetConnectionType(NavData2D.NavPoint a, NavData2D.NavPoint b)
    {
        if (a == null || b == null) return NavData2D.ConnectionType.None;
        var connection = a.Connections.Find(conn => conn.Point.CellPos == b.CellPos);
        if (connection != null)
        {
            return connection.Type;
        }
        return NavData2D.ConnectionType.None;
    }

    public void Stop()
    {
        if (m_NewPathPending || m_NewPathReady)
        {
            m_DismissPath = true;
        }
        if (m_State == AgentState.Stopped)
        {
            return;
        }

        SetAgentState(AgentState.Stopped);
        m_PassingThrough = false;

    }

    public float DistanceToTarget()
    {
        if (IsPathPending || m_Path.Count == 0)
        {
            return float.PositiveInfinity;
        }

        if (m_Path == null || m_CurrentPointIndex >= m_Path.Count)
        {
            return 0.0f;
        }

        float distance = Vector2.Distance(transform.position, m_Path[m_CurrentPointIndex].Position);
        for (int i = m_CurrentPointIndex + 1; i < m_Path.Count; i++)
        {
            distance += Vector2.Distance(m_Path[i - 1].Position, m_Path[i].Position);
        }
        return distance;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!m_DrawPath) return;

        if (m_Path != null && m_Path.Count > 1)
        {
            int startIndex = Mathf.Max(0, m_CurrentPointIndex);

            for (int i = startIndex; i < m_Path.Count - 1; i++)
            {
                if (m_Path[i] != null && m_Path[i + 1] != null)
                    GizmosEx.DrawArrow(m_Path[i].Position, m_Path[i + 1].Position, Color.cyan);
            }

            if (m_Path != null && m_CurrentPointIndex < m_Path.Count && m_Path[m_CurrentPointIndex] != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(m_Path[m_CurrentPointIndex].Position, 0.3f);
            }
        }
    }
#endif
}
