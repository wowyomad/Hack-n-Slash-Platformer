using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TheGame;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToTarget", story: "[Agent] moves to [Target]", category: "Action", id: "a3cb355433384adbe043ccab43b6866a")]
public partial class MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(8.0f);
    [SerializeReference] public BlackboardVariable<float> DeccelerationTime = new BlackboardVariable<float>(0.25f);
    [SerializeReference] public BlackboardVariable<float> AccelerationTime = new BlackboardVariable<float>(0.35f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.01f);
    [SerializeReference] public BlackboardVariable<float> MaxPathCalculationTime = new BlackboardVariable<float>(0.5f);

    [SerializeReference] public BlackboardVariable<bool> UpdatePath = new BlackboardVariable<bool>(false);
    [SerializeReference] public BlackboardVariable<float> UpdatePathInterval = new BlackboardVariable<float>(0.25f);

    [SerializeReference] public BlackboardVariable<bool> CantReachTarget;


    private NavAgent2D m_NavAgent;
    private Enemy m_Self;
    private Entity m_TargetEntity;
    private float m_PathWaitTimer = 0.0f;
    private float m_PathUpdateTimer = 0.0f;

    private bool m_Initialized = false;

    protected override Status OnStart()
    {
        if (!m_Initialized)
        {
            Intialize();
        }

        if (CantReachTarget == null)
        {
            LogFailure("CantReachTarget is null", true);
            return Status.Failure;
        }

        CantReachTarget.Value = false;

        m_NavAgent.SetDestination(Target.Value.transform.position);
        m_NavAgent.SetSpeed(Speed.Value);
        m_NavAgent.SetAccelerationTime(AccelerationTime.Value);
        m_NavAgent.SetDeccelerationTime(DeccelerationTime.Value);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (UpdatePath.Value)
        {
            if (m_PathUpdateTimer >= UpdatePathInterval.Value)
            {
                var position = Target.Value.transform.position;
                if (m_Self.CanSeeEntity(m_TargetEntity, true))
                {
                    m_NavAgent.SetDestination(position);
                    m_PathUpdateTimer = 0.0f;
                }
            }
            else
            {
                m_PathUpdateTimer += Time.deltaTime;
            }
        }

        if (m_NavAgent.DistanceToTarget() < DistanceThreshold.Value)
        {
            return Status.Success;
        }

        if (!m_NavAgent.IsFollowing && !m_NavAgent.IsPathPending)
        {
            if (m_NavAgent.InvalidPath)
            {
                CantReachTarget.Value = true;
            }
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
        else
        {
            m_PathWaitTimer = 0.0f;
        }

        return Status.Running;
    }

    private void Intialize()
    {
        if (Target.Value.TryGetComponent<Entity>(out var targetEntity))
        {
            m_TargetEntity = targetEntity;
        }


        m_Self = Agent.Value.GetComponent<Enemy>();
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();

        m_Initialized = true;
    }

    protected override void OnEnd()
    {
        m_NavAgent.Stop();
        m_PathWaitTimer = 0.0f;
    }
}

