using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateStunnedState", story: "Update [Stunned] state from [Agent]", category: "Action", id: "70af6c0f5ff6fdfa6e63770c58c5e536")]
public partial class UpdateStunnedStateAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Stunned;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    protected Enemy Self;
    protected override Status OnStart()
    {
        Self = Agent.Value.GetComponent<Enemy>();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Stunned.Value = Self.IsStunned;
        return Status.Success;
    }

}

