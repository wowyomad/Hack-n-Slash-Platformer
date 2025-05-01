using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nav2D;
using NUnit.Framework.Constraints;
using UnityEngine;

[RequireComponent(typeof(CharacterController2D))]
public class NavAgent2D : MonoBehaviour
{
    [SerializeField] private NavData2D m_NavData;
    CharacterController2D m_Controller;

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

    private ActionTimer m_PathfindingTimer = new(true, false);

    private void Awake()
    {
        if (m_NavData == null)
        {
            DebugEx.LogAndThrow("NavWeights is not assigned", message => new NullReferenceException(message));
        }
        m_Controller = GetComponent<CharacterController2D>();

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        m_PathfindingTimer.SetCallback(() =>
        {
            var start = Time.realtimeSinceStartup;
            m_CurrentPath = m_NavData.GetPath(transform.position, playerTransform.position);
            var finished = Time.realtimeSinceStartup;
            Debug.Log($"Pathfinding took {finished - start} seconds");
        });

        m_PathfindingTimer.Start(0.1f);
    }


    private void Update()
    {
        m_PathfindingTimer.Tick();
    }
    
    public NavPoint GetClosestNavpoint(Vector2 target)
    {
        return m_NavData.GetClosestNavPoint(target);
    }


    private void OnDrawGizmos()
    {
        if (m_CurrentPath != null && m_CurrentPath.Count > 1)
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < m_CurrentPath.Count; i++)
            {
                GizmosEx.DrawArrow(m_CurrentPath[i - 1].Position, m_CurrentPath[i].Position, Color.green);
            }
        }
    }
}
