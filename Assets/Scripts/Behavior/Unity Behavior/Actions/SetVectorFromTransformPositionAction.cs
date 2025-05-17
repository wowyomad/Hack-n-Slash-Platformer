using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetVectorFromTransformPosition", story: "Set Vector [Value] from [Transform] (position)", category: "Action", id: "ecbaf9e45eb352d5df86dc1601afef12")]
public partial class SetVectorFromTransformPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector3> Value;
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    protected override Status OnStart()
    {
        Value.Value = Transform.Value.position;
        return Status.Success;
    }

    
}

