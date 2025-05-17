using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "EntityAlive", story: "[Entity] is alive", category: "Conditions", id: "ed22ebf3d38257b2ef9b882854f48003")]
public partial class EntityAliveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Entity;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
