using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "EntityDead", story: "[Entity] is dead", category: "Conditions", id: "68cc7b6118d15492b64fc40db180f478")]
public partial class EntityDeadCondition : Condition
{
    [SerializeReference] public BlackboardVariable<Entity> Entity;

    public override bool IsTrue()
    {
        return Entity.Value.IsDead;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
