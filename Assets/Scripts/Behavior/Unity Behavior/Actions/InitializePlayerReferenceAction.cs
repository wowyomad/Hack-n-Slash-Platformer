using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "InitializePlayerReference", story: "Sets player reference to [Player] by [Tag]", category: "Action", id: "62639ad165d327d32dc5ef86f2047e03")]
public partial class InitializePlayerReferenceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<string> Tag = new BlackboardVariable<string>("Player");

    protected override Status OnStart()
    {
        if (Player == null)
        {
            LogFailure("Player reference is not passed as a BlackboardVariable.", true);
            return Status.Failure;
        }

        if (Player.Value != null)
        {
            return Status.Success;
        }

        var playerObject = GameObject.FindWithTag(Tag.Value);
        if (playerObject == null)
        {
            LogFailure($"No GameObject found with tag '{Tag.Value}'.", true);
            return Status.Failure;
        }
        Player.Value = playerObject;
        return Status.Success;
    }


}

