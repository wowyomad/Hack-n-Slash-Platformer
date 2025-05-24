using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "EntityAlive", story: "[Entity] is [State]", category: "Conditions", id: "ed22ebf3d38257b2ef9b882854f48003")]
public partial class EntityAliveCondition : Condition
{

    public enum EntityState
    {
        Alive,
        Dead
    }
    [SerializeReference] public BlackboardVariable<Entity> Entity;

    [SerializeReference] public BlackboardVariable<EntityState> State;


    public override bool IsTrue()
    {
        if (Entity == null || Entity.Value == null || State == null)
        {
            Debug.LogError("Entity or State is not set in EntityAliveCondition");
            return false;
        }
        switch (State.Value)
        {
            case EntityState.Alive:
                return Entity.Value.IsAlive;
            case EntityState.Dead:
                return !Entity.Value.IsAlive;
            default:
                Debug.LogError("Invalid operator in EntityAliveCondition");
                return false;
        }
    }
}
