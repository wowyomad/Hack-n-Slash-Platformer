using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol2D", story: "[Agent] patrols [PatrolWaypoints] until sees [Target]", category: "Action", id: "185a75690c390c236f543af8d58b9217")]
public partial class Patrol2DAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolWaypoints;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> WaitTime = new BlackboardVariable<float>(2.0f);
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(8.0f);
    [SerializeReference] public BlackboardVariable<float> DeccelerationTime = new BlackboardVariable<float>(0.25f);
    [SerializeReference] public BlackboardVariable<float> AccelerationTime = new BlackboardVariable<float>(0.35f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.01f);


    private NavAgent2D m_NavAgent;
    private int m_CurrentWaypointIndex = 0;

    private float m_WaitTime = 0.0f;
    private bool m_Waiting = false;

    private Enemy Self;
    private Player Player;

    protected override Status OnStart()
    {
        if (PatrolWaypoints.Value == null || PatrolWaypoints.Value.Count < 2)
        {
            return Status.Failure;
        }
        Player = Target.Value.GetComponent<Player>();
        Self = Agent.Value.GetComponent<Enemy>();

        m_CurrentWaypointIndex = 0;
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();

        m_NavAgent.SetSpeed(Speed.Value);
        m_NavAgent.SetAccelerationTime(AccelerationTime.Value);
        m_NavAgent.SetDeccelerationTime(DeccelerationTime.Value);

        m_NavAgent.SetDestination(PatrolWaypoints.Value[m_CurrentWaypointIndex].transform.position);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        if (Self.CanSeePlayer(Player))
        {
            return Status.Failure;
        }

        if (m_Waiting)
        {
            m_WaitTime += Time.deltaTime;
            if (m_WaitTime >= WaitTime.Value)
            {
                m_Waiting = false;
                m_WaitTime = 0.0f;

                if (m_CurrentWaypointIndex >= PatrolWaypoints.Value.Count)
                {
                    return Status.Success;
                }

                m_NavAgent.SetDestination(PatrolWaypoints.Value[m_CurrentWaypointIndex].transform.position);
            }
        }
        else if (m_NavAgent.DistanceToTarget() < DistanceThreshold.Value)
        {
            m_CurrentWaypointIndex++;
            m_Waiting = true;
        }

        return Status.Running;
    }
}

