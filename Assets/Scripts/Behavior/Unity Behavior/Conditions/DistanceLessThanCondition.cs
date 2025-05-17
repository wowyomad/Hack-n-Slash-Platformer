using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "DistanceLessThan", story: "Distance between [Value1] and [Value2] < [Distance]", category: "Variable Conditions", id: "70a18c922551babe4ac243f07e8ec503")]
public partial class DistanceLessThanCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> Value1;
    [SerializeReference] public BlackboardVariable<Transform> Value2;
    [SerializeReference] public BlackboardVariable<float> Distance;

    public override bool IsTrue()
    {
        if (Value1.Value == null || Value2.Value == null || Distance.Value <= 0)
        {
            Debug.LogError("Invalid values for distance comparison.");
            return false;
        }
        return Vector3.Distance(Value1.Value.position, Value2.Value.position) < Distance.Value;
    }
}
