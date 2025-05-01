using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TestAction", story: "[Agnet] moves to [Target]", category: "Action", id: "bdca9ec90d26deca5769ed86ace47449")]
public partial class TestAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agnet;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    
}
