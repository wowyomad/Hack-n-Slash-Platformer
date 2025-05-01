using System;
using System.Collections.Generic;
using Nav2D;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class NavAgent2D : MonoBehaviour
{
    [SerializeField] private NavData2D m_NavData;
    CharacterController2D m_Controller;
    private NavPoint m_LastNavPoint = null;

    private bool m_IsFollowing = false;
    public bool IsFollowing => m_IsFollowing;
    public bool IsPathReady => PathCalculated && m_CurrentPath != null && m_CurrentPath.Count > 0;
    public Vector3? CurrentPathTarget => (IsPathReady && m_CurrentPathIndex < m_CurrentPath.Count) ? m_CurrentPath[m_CurrentPathIndex].Position : (Vector3?)null;

    [SerializeField] private LayerMask m_Mask => m_NavData.CollisionMask;
    [SerializeField] private float m_DebugDrawDuration = 5.0f;

    [SerializeField] private float m_MoveSpeed = 5f;
    [SerializeField] private float m_JumpHeight = 3f;
    [SerializeField] private float m_JumpDuration = 0.5f;

    private List<NavPoint> m_CurrentPath = null;
    private int m_CurrentPathIndex = 0;
    public bool PathCalculated { get; private set; } = false;
    private bool m_PathfindingInProgress = false;

    private void Awake()
    {
        if (m_NavData == null)
        {
            DebugEx.LogAndThrow("NavWeights is not assigned", message => new NullReferenceException(message));
        }
        m_Controller = GetComponent<CharacterController2D>();
    }

    private void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            NavPoint targetNavPoint = GetClosestNavpoint(mousePosition);
            if (targetNavPoint != null)
            {
                DebugEx.DrawArrow(mousePosition, targetNavPoint.Position, Color.green, m_DebugDrawDuration);

                Debug.Log($"NavPoint {targetNavPoint.Position} has {targetNavPoint.Connections.Count} connections.");
                foreach (var connection in targetNavPoint.Connections)
                {
                    DebugEx.DrawArrow(targetNavPoint.Position, connection.Point.Position, Color.blue, m_DebugDrawDuration);
                }
            }
        }
    }

    public NavPoint GetClosestNavpoint(Vector2 target)
    {
        return m_NavData.GetClosestNavPoint(target);
    }


    private void OnDrawGizmos()
    {
        if (m_LastNavPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, m_LastNavPoint.Position);
        }
        // Draw path
        if (m_CurrentPath != null && m_CurrentPath.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < m_CurrentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(m_CurrentPath[i].Position, m_CurrentPath[i + 1].Position);
            }
        }
    }
}
