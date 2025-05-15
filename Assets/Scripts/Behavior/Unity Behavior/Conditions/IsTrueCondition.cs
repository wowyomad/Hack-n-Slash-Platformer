using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is Condition False", story: "Not [Condition]", category: "Conditions", id: "7a06d9d6f8d1bf6a9346839c1fd4e6c9")]
public partial class IsFalseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<bool> Condition;

    public override bool IsTrue()
    {
        return !Condition.Value;  
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
