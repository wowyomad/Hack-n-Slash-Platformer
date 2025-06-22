using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "IncrementInteger", story: "Increment [Value] by [Delta]", category: "Action/Blackboard", id: "f3d9f7d1d9a06f7088acf1f1cbe917fd")]
public partial class IncrementIntegerAction : Action
{
    [SerializeReference] public BlackboardVariable<int> Value;
    [SerializeReference] public BlackboardVariable<int> Delta;

    protected override Status OnStart()
    {
        if (Value == null)
        {
            LogFailure("Value reference must be set!");
            return Status.Failure;
        }
        if (Delta == null)
        {
            LogFailure("Delta reference must be set!");
            return Status.Failure;
        }
        Value.Value += Delta.Value;
        return Status.Success;
    }
}

