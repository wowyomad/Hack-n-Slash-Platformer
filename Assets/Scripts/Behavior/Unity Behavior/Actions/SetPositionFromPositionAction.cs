using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Position From Position", story: "Set [Target] position from [Other]", category: "Action/Blackboard", id: "d081ac9a1407df9b65902c0109bb55f0")]
public partial class SetPositionFromPositionAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Other;

    protected override Status OnStart()
    {
        Target.Value.position = Other.Value.transform.position;
        return Status.Success;
    }
}

