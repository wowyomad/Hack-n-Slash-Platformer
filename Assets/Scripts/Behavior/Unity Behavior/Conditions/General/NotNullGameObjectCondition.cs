using System;
using System.Diagnostics;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Is GameObject null", story: "[Target] [ToBeValue] null", category: "Conditions", id: "1353cbe9c7963fd262e3c060ec360e16")]
public partial class IsNullGameObjectCondition : Condition
{
    public enum ToBe
    {
        Is,
        IsNot
    }
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<ToBe> ToBeValue;

    public override bool IsTrue()
    {
        return ToBeValue.Value == ToBe.IsNot ? Target.Value != null : Target.Value == null;
    }
}
