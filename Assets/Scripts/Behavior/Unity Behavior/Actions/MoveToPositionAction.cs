using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToPosition", story: "[Agent] moves to [Position] (Vector3)", category: "Action", id: "28b213d1d3b400f9ad37dc5d30551cda")]
public partial class MoveToPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Vector3> Position;

    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(8.0f);
    [SerializeReference] public BlackboardVariable<float> DeccelerationTime = new BlackboardVariable<float>(0.25f);
    [SerializeReference] public BlackboardVariable<float> AccelerationTime = new BlackboardVariable<float>(0.35f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.01f);
    [SerializeReference] public BlackboardVariable<float> UpdateDestinationInterval = new BlackboardVariable<float>(0.5f);
    [SerializeReference] public BlackboardVariable<float> MaxPathCalculationTime = new BlackboardVariable<float>(0.5f);


    private NavAgent2D m_NavAgent;
    private float m_PathWaitTimer = 0.0f;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Position.Value == null)
        {
            LogFailure("Agent or Position is null");
            return Status.Failure;
        }
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();
        m_NavAgent.SetDestination(Position.Value);

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
}

