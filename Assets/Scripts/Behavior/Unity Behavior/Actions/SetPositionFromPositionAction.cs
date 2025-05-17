using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Position From Position", story: "Set [T1] from [T2] (Position)", category: "Action", id: "d081ac9a1407df9b65902c0109bb55f0")]
public partial class SetPositionFromPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> T1;
    [SerializeReference] public BlackboardVariable<Transform> T2;
    protected override Status OnStart()
    {
        T1.Value.position = T2.Value.position;
        return Status.Success;
    }
}

