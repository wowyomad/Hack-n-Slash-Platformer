using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "FlipEntityInDirection", story: "Flip [Entity] in [Sign] [Direction]", category: "Action", id: "ccf5bdd6f10b87e14ae4b8e48ecb2e4d")]
public partial class FlipEntityInDirectionAction : Action
{
    public enum ESign
    {
        Direct,
        Inverted
    }
    [SerializeReference] public BlackboardVariable<Entity> Entity;
    [SerializeReference] public BlackboardVariable<Vector2> Direction;
    [SerializeReference] public BlackboardVariable<ESign> Sign;

    protected override Status OnStart()
    {
        Entity.Value.Flip(Sign.Value == ESign.Direct ? Direction.Value.x : -Direction.Value.x);
        return Status.Success;
    }
}

