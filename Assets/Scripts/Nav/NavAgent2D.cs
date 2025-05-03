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

    public bool IsJumping => m_Jumping;
    public bool IsFollowing => m_Following;
    public bool IsPathReady => PathCalculated && m_Path != null && m_Path.Count > 0;
    public Vector3? CurrentPathTarget => (IsPathReady && m_CurrentPointIndex < m_Path.Count) ? m_Path[m_CurrentPointIndex].Position : (Vector3?)null;
    public bool PathCalculated { get; private set; } = false;


    private CharacterController2D m_Controller;
    private bool m_Following = false;
    private bool m_Jumping = false;

    private List<NavPoint> m_Path = null;
    private int m_CurrentPointIndex = 0;
    private Vector2 m_Target = Vector2.zero;

    private ActionTimer m_PathfindingTimer = new(true, false);

    static private readonly float REACH_BIAS = 0.0001f;

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


    private void Update()
    {
        if (!m_Following) return;

        if (m_Path == null || m_Path.Count == 0 || m_CurrentPointIndex >= m_Path.Count)
        {
            m_Following = false;
            return;
        }

        if (m_Jumping)
        {
            Debug.LogWarning("Supposed to jump here but not implemented yet(((");
        }
        else if (m_Controller.IsGrounded)
        {
            Vector3 target = m_Path[m_CurrentPointIndex].Position;
            target.y = transform.position.y; // ignore y axis horizontal movement
            Vector2 direction = (target - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, target);
            if (distance < REACH_BIAS)
            {
                m_CurrentPointIndex++;
                if (m_CurrentPointIndex >= m_Path.Count)
                {
                    m_Following = false;
                    return;
                }
            }
            else
            {
                float displacement = Mathf.Min(m_NavActor.BaseSpeed * Time.deltaTime, distance);
                m_Controller.Move(direction * displacement);
            }
        }
        //wait until grounded?
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
        PathCalculated = m_Path != null && m_Path.Count > 0;
        m_CurrentPointIndex = 0;
        m_Following = true;
    }

    private void OnDrawGizmos()
    {
        if (m_Path != null && m_Path.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < m_Path.Count; i++)
            {
                GizmosEx.DrawArrow(m_Path[i - 1].Position, m_Path[i].Position, Color.green);
            }
        }
    }
}
