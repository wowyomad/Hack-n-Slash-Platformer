using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PatrolPointsExist", story: "[PatrolWaypoints] are not empty", category: "Conditions", id: "ea80ace43011f82fd56db0965a7557f6")]
public partial class PatrolPointsExistCondition : Condition
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> PatrolWaypoints;

    public override bool IsTrue()
    {
        var waypoints = PatrolWaypoints.Value;
        return waypoints != null && waypoints.Count >= 2 && waypoints.All(w => w != null);
    }
}
