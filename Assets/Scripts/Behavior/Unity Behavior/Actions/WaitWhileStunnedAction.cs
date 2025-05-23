using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitWhileStunned", story: "Stunned for [StunDuration]", category: "Action", id: "7407061065da6958f6459d07619f66c2")]
public partial class WaitWhileStunnedAction : Action
{
    [SerializeReference] public BlackboardVariable<float> StunDuration;
    [SerializeReference] public BlackboardVariable<float> StunTimeLeft;

    protected override Status OnStart()
    {
        if (StunDuration == null)
        {
            LogFailure("StunDuration reference must be set!");
            return Status.Failure;
        }
        if (StunTimeLeft == null)
        {
            LogFailure("StunTimeLeft reference must be set!");
            return Status.Failure;
        }
        StunTimeLeft.Value = StunDuration.Value;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (StunTimeLeft.Value > 0)
        {
            StunTimeLeft.Value -= Time.deltaTime;
            return Status.Running;
        }
        else
        {
            return Status.Success;
        }
    }

    protected override void OnEnd()
    {
        StunTimeLeft.Value = 0;
    }
}

