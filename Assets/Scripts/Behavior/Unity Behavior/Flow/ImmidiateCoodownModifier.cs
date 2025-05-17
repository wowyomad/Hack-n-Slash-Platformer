using System;
using Unity.Behavior;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Immidiate Coodown", story: "Cooldown for [Duration] seconds", category: "Flow", id: "1b7a803e4ecf717b6383e018b1fe1236")]
public partial class ImmidiateCoodownModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<float> Duration;

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

