using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using TheGame;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "WaitForValueChange_AlertedState", story: "Wait for [Value] to change", category: "Action/Delay", id: "f8b8813384ccf0f7e822e0b81860f3ca")]
public partial class WaitForValueChangeAction : Action
{
    [SerializeReference] public BlackboardVariable<AlertedState> Value;

    private bool m_ValueChanged;

    protected override Status OnStart()
    {
        Value.OnValueChanged += OnValueChanged;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_ValueChanged)
        {
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        Value.OnValueChanged -= OnValueChanged;
        m_ValueChanged = false;
    }

    private void OnValueChanged()
    {
        m_ValueChanged = true;
    }


}

