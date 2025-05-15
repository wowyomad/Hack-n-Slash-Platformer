using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToTarget", story: "[Agent] moves to [Target] until within [Reach]", category: "Action", id: "a3cb355433384adbe043ccab43b6866a")]
public partial class MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Reach;

    [SerializeReference] public BlackboardVariable<float> UpdateDestinationInterval;


    private NavAgent2D m_NavAgent;
    private float m_UpdateDestinationTimer;
    protected override Status OnStart()
    {
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();
        m_UpdateDestinationTimer = 0f;

        m_NavAgent.SetDestination(Target.Value.transform.position);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_UpdateDestinationTimer += Time.deltaTime;

        if (m_UpdateDestinationTimer >= UpdateDestinationInterval.Value)
        {
            m_NavAgent.SetDestination(Target.Value.transform.position);
            m_UpdateDestinationTimer = 0f;
        }
        else if (Vector2.Distance(Agent.Value.transform.position, Target.Value.transform.position) <= Reach.Value)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        m_NavAgent.Stop();
    }
}

