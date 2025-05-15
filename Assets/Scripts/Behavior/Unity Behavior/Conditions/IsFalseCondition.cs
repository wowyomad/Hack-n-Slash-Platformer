using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Condition Is True", story: "[Condition]", category: "Conditions", id: "d57aacbe7ce8275d586c46722ed32281")]
public partial class IsTrueCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Condition;

    public override bool IsTrue()
    {
        return Condition.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
