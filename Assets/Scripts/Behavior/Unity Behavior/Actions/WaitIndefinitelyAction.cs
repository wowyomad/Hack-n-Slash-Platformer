using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wait indefinitely", story: "Wait indefinitely", category: "Action", id: "27e530a334ed72524eb499b72dc40d15")]
public partial class WaitIndefinitelyAction : Action
{

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Running;
    }
}

