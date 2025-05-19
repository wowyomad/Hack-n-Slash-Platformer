using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToWayopoint", story: "[Agent] moves to waypoint [Index] from [Waypoints]", category: "Action", id: "436ac4c1018fc34f9541a4d164eae38a")]
public partial class MoveToWayopointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<int> Index;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(8.0f);
    [SerializeReference] public BlackboardVariable<float> DeccelerationTime = new BlackboardVariable<float>(0.25f);
    [SerializeReference] public BlackboardVariable<float> AccelerationTime = new BlackboardVariable<float>(0.35f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.01f);

    [SerializeReference] public BlackboardVariable<float> MaxPathCalculationTime = new BlackboardVariable<float>(0.5f);

    private float m_PathWaitTimer = 0.0f;
    private NavAgent2D m_NavAgent;

    private bool m_Initialized = false;

    protected override Status OnStart()
    {
        if (!m_Initialized)
        {
            Initialize();
        }

        m_NavAgent.SetDestination(Waypoints.Value[Index.Value].transform.position);

        m_NavAgent.SetSpeed(Speed.Value);
        m_NavAgent.SetAccelerationTime(AccelerationTime.Value);
        m_NavAgent.SetDeccelerationTime(DeccelerationTime.Value);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_NavAgent.DistanceToTarget() < DistanceThreshold.Value)
        {
            return Status.Success;
        }

        if (!m_NavAgent.IsFollowing && !m_NavAgent.IsPathPending)
        {
            return Status.Failure;
        }

        if (m_NavAgent.IsPathPending)
        {
            m_PathWaitTimer += Time.deltaTime;
            if (m_PathWaitTimer >= MaxPathCalculationTime.Value)
            {
                LogFailure("Path calculation timed out");
                return Status.Failure;
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_NavAgent.Stop();
        m_PathWaitTimer = 0.0f;
    }

    private void Initialize()
    {
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();
        if (m_NavAgent == null)
        {
            Debug.LogError("NavAgent2D component not found on Agent GameObject.");
            return;
        }
        m_Initialized = true;
    }
}

