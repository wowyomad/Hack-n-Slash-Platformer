using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "[Agent] patrols between [Waypoints] with [Proximity] until [Alerted]", category: "Flow", id: "f314202791b91a08bf381b35e3e83d49")]
public partial class PatrolSequence : Composite
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> Proximity = new BlackboardVariable<float>(0.01f);
    [SerializeReference] public BlackboardVariable<bool> Alerted;
    [SerializeReference] public Node Port1;
    [SerializeReference] public Node Port2;


    private NavAgent2D m_NavAgent;
    private int m_CurrentWaypointIndex = 0;
    protected override Status OnStart()
    {
        Initialize();

        if (m_NavAgent == null)
        {
            Debug.LogError("NavAgent is null");
            return Status.Failure;
        }

        if (Alerted.Value)
        {
            return Status.Failure;
        }

        m_NavAgent.SetDestination(Waypoints.Value[m_CurrentWaypointIndex].transform.position);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Alerted.Value)
        {
            return Status.Failure;
        }


        if (m_NavAgent.DistanceToTarget() < Proximity.Value)
        {
            MoveToNextWaypoint();
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_NavAgent.ResetPath();
        m_CurrentWaypointIndex = 0;
    }

    private bool m_Initialized = false;

    private void MoveToNextWaypoint()
    {
        if (m_CurrentWaypointIndex >= Waypoints.Value.Count)
        {
            Waypoints.Value.Reverse();
            m_CurrentWaypointIndex = 0;
        }

        m_NavAgent.SetDestination(Waypoints.Value[m_CurrentWaypointIndex].transform.position);
        m_CurrentWaypointIndex++;
    }
    private void Initialize()
    {
        if (m_Initialized)
            return;

        m_Initialized = true;
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();

        Debug.Log($"NavAgent: {Agent.Value.name}");
        foreach (var component in Agent.Value.GetComponents<MonoBehaviour>())
        {
            Debug.Log($"Component: {component.GetType().Name}");
        }

    }
}

