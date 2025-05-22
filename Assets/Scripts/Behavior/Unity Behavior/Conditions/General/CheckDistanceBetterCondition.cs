using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Check Distance Better", story: "Distance between [Origin] and [Target] [Comparison] than [Distance] on [Axis]", category: "Conditions", id: "e1d1f7497ed9731dc070a6001b655caf")]
public partial class CheckDistanceBetterCondition : Condition
{
    public enum EAxis
    {
        Both,
        Horizontal,
        Vertical,
    }
    [SerializeReference] public BlackboardVariable<Transform> Origin;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [Comparison(comparisonType: ComparisonType.All)]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Comparison;
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<EAxis> Axis;

    public override bool IsTrue()
    {
        if (Origin.Value == null || Target.Value == null)
        {
            return false;
        }

        switch (Axis.Value)
        {
            case EAxis.Both:
                return ConditionUtils.Evaluate(Vector2.Distance(Origin.Value.position, Target.Value.position), Comparison, Distance.Value);
            case EAxis.Horizontal:
                return ConditionUtils.Evaluate(Mathf.Abs(Origin.Value.position.x - Target.Value.position.x), Comparison, Distance.Value);
            case EAxis.Vertical:
                return ConditionUtils.Evaluate(Mathf.Abs(Origin.Value.position.y - Target.Value.position.y), Comparison, Distance.Value);
            default:
                return false;
        }
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
