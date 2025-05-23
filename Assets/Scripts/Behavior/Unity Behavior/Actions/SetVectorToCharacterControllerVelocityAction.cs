using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Vector to CharacterController velocity", story: "Set [Vector] to [Target] CharacterController.Velocity", category: "Action/Blackboard", id: "bc4a563d92b5d17d1d263e28bf11de2b")]
public partial class SetVectorToCharacterControllerVelocityAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2> Vector;
    [SerializeReference] public BlackboardVariable<CharacterController2D> Target;
    protected override Status OnStart()
    {
        if (Target.Value == null)
        {
            LogFailure("CharacterController is null", true);
            return Status.Failure;
        }
        if (Vector.Value == null)
        {
            LogFailure("Vector is null", true);
            return Status.Failure;
        }
        Vector.Value = Target.Value.Velocity;
        return Status.Success;
    }
}

