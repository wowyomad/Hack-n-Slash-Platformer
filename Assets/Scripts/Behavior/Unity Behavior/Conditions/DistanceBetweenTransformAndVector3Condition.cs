using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Distance Between TransformAndVector3", story: "Distance between [Value1] and [Value2] (Vector3) < [Distance]", category: "Conditions", id: "9298fd679a62ce4e29fe4654818aa553")]
public partial class DistanceBetweenTransformAndVector3Condition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> Value1;
    [SerializeReference] public BlackboardVariable<Vector3> Value2;
    [SerializeReference] public BlackboardVariable<float> Distance;

    public override bool IsTrue()
    {
        if (Value1 == null || Value2 == null || Distance == null)
        {
            Debug.LogError("Invalid values for distance comparison.");
            return false;
        }
        return Vector3.Distance(Value1.Value.position, Value2.Value) < Distance.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
