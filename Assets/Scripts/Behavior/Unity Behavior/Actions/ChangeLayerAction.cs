using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChangeObjectLayer", story: "Change [Agent] layer to [Layer]", category: "Action/GameObject", id: "9384529db6fe6e6241e3ab9ecab55612")]
public partial class ChangeLayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<string> Layer;

    protected override Status OnStart()
    {
        int layerIndex = LayerMask.NameToLayer(Layer.Value);
        if (layerIndex < 0)
        {
            LogFailure($"Layer '{Layer.Value}' does not exist or is invalid.");
            return Status.Failure;
        }

        Agent.Value.layer = layerIndex;
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

