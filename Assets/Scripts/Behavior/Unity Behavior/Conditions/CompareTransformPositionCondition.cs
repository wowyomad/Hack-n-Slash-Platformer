using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CompareTransformPosition", story: "[T1] is [Operator] [T2] (Position)", category: "Variable Conditions", id: "43978a7136824d126532a4f27aacd43b")]
public partial class CompareTransformPositionCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Transform> T1;
    [SerializeReference] public BlackboardVariable<Transform> T2;
    [Comparison(comparisonType: ComparisonType.BlackboardVariables, variable: "T1", comparisonValue: "T2")]
    [SerializeReference] public BlackboardVariable<ConditionOperator> Operator;

    public override bool IsTrue()
    {
        if (ConditionOperator.Equal == Operator.Value)
        {
            if (T1.Value == null && T2.Value == null)
            {
                return true;
            }
            else if (T1.Value == null || T2.Value == null)
            {
                return false;
            }
            else
            {
                return T1.Value.position == T2.Value.position;
            }
        }
        else if (ConditionOperator.NotEqual == Operator.Value)
        {
            if (T1.Value == null && T2.Value == null)
            {
                return false;
            }
            else if (T1.Value == null || T2.Value == null)
            {
                return true;
            }
            else
            {
                return T1.Value.position != T2.Value.position;
            }
        }

        Debug.LogError($"Condition operator {Operator.Value} is not supported for CompareTransformPositionCondition");
        return false;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
