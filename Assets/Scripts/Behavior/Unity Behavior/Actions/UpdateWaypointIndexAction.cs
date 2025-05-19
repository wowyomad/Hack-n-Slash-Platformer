using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateWaypointIndex", story: "Increase [CurrentWaypointIndex] related to [Waypoints]", category: "Action", id: "c22ae01afd4657260b16b67fc02232cd")]
public partial class UpdateWaypointIndexAction : Action
{
    [SerializeReference] public BlackboardVariable<int> CurrentWaypointIndex;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    protected override Status OnStart()
    {
        if (Waypoints == null)
            return Status.Failure;

        CurrentWaypointIndex.Value = (CurrentWaypointIndex.Value + 1) % Waypoints.Value.Count;
        return Status.Success;
    }
}

