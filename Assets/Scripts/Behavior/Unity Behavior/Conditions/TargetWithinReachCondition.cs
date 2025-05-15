using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "TargetWithinReach", story: "[Agent] is within [Reach] range to [Target]", category: "Conditions", id: "57c13c7654196138e493f0b856c448ff")]
public partial class TargetWithinReachCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Reach;
    [SerializeReference] public BlackboardVariable<GameObject> Target;


    private NavAgent2D m_NavAgent;

    public override bool IsTrue()
    {
        return Vector2.Distance(Agent.Value.transform.position, Target.Value.transform.position) <= Reach.Value;
    }

    public override void OnStart()
    {
        m_NavAgent = Agent.Value.GetComponent<NavAgent2D>();

    }

    public override void OnEnd()
    {
    }
}
