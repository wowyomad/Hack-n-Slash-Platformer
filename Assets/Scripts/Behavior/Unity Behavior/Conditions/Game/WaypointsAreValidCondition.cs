using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Waypoints are valid", story: "[Waypoints] are valid", category: "Variable Conditions", id: "cfe4c662aeb314b91a54d674dd147e67")]
public partial class WaypointsAreValidCondition : Condition
{
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;

    public override bool IsTrue()
    {
        return Waypoints.Value.Count >= 2 && Waypoints.Value.TrueForAll(waypoint => waypoint != null);
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
