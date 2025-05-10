using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Alive", story: "Agent is [Alive]", category: "Conditions", id: "3f49317ab7c1a3ad5d084cff47d7c4c2")]
public partial class IsAliveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Alive;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
