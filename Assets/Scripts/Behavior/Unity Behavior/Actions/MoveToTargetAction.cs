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
    [SerializeReference] public BlackboardVariable<bool> PredictTargetMovement = new BlackboardVariable<bool>(false);
    [SerializeReference] public BlackboardVariable<float> PredictionMultiplier = new BlackboardVariable<float>(2.0f); // New variable

    [SerializeReference] public BlackboardVariable<Vector2> TargetVelocity;
    [SerializeReference] public BlackboardVariable<bool> CantReachTarget;


    private NavAgent2D m_NavAgent;
    private float m_PathWaitTimer = 0.0f;
    private float m_PathUpdateTimer = 0.0f;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_LastAgentPosition;
    private bool m_Initialized = false;

    protected override Status OnStart()
    {
        if (!m_Initialized)
        {
            Intialize();
        }

        if (Target == null || Target.Value == null)
        {
            LogFailure("Target is null", true);
            return Status.Failure;
        }

        if (CantReachTarget == null)
        {
            LogFailure("CantReachTarget is null", true);
            return Status.Failure;
        }

        CantReachTarget.Value = false;

        if (m_LastTargetPosition == Target.Value.transform.position && m_LastAgentPosition == Agent.Value.transform.position)
        {
            return Status.Success;
        }

        float distance = Vector3.Distance(Target.Value.transform.position, Agent.Value.transform.position);
        if (distance < DistanceThreshold.Value)
        {
            return Status.Success;
        }

        m_LastTargetPosition = Target.Value.transform.position;
        m_LastAgentPosition = Agent.Value.transform.position;

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
                Vector3 currentActualTargetPosition = Target.Value.transform.position;
                Vector3 newPrimaryDestination = currentActualTargetPosition;
                bool pathNeedsRecalculation = false;

                if (PredictTargetMovement.Value)
                {
                    Vector3 predictionVelocity = TargetVelocity.Value;
                    predictionVelocity.y = 0.0f; 
                    
                    Vector3 predictedTargetPosition = currentActualTargetPosition + 
                                                      predictionVelocity * UpdatePathInterval.Value * PredictionMultiplier.Value;
                    newPrimaryDestination = predictedTargetPosition;

                    if (Mathf.Abs(m_LastTargetPosition.x - predictedTargetPosition.x) > 0.01f ||
                        Mathf.Abs(m_LastTargetPosition.y - predictedTargetPosition.y) > 0.01f)
                    {
                        m_NavAgent.SetDestination(predictedTargetPosition, currentActualTargetPosition);
                        pathNeedsRecalculation = true;
                    }
                }
                else
                {
                    if (Mathf.Abs(m_LastTargetPosition.x - currentActualTargetPosition.x) > 0.01f ||
                        Mathf.Abs(m_LastTargetPosition.y - currentActualTargetPosition.y) > 0.01f)
                    {
                        m_NavAgent.SetDestination(currentActualTargetPosition);
                        pathNeedsRecalculation = true;
                    }
                }

                if (pathNeedsRecalculation)
                {
                    m_LastTargetPosition = newPrimaryDestination;
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


        if (m_NavAgent.IsPathPending || m_NavAgent.InvalidPath)
        {
            m_PathWaitTimer += Time.deltaTime;
            if (m_PathWaitTimer >= MaxPathCalculationTime.Value)
            {
                if (m_NavAgent.InvalidPath)
                {
                    CantReachTarget.Value = true;
                    LogFailure("Can't reach target", true);
                }
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
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();
        m_Initialized = true;
    }

    protected override void OnEnd()
    {
        m_NavAgent.Stop();
        m_PathWaitTimer = 0.0f;
    }
}

