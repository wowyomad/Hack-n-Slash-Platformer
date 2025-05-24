using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChangeColliderState", story: "Set [Agent] Collider2D [State]", category: "Action", id: "c1481d6758c7da9598e119d882f9ad24")]
public partial class ChangeColliderStateAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<ColliderState> State;
    public enum ColliderState
    {
        Enabled,
        Disabled
    }
    [SerializeReference] public BlackboardVariable<ColliderState> SwitchState = new BlackboardVariable<ColliderState>(ColliderState.Disabled);


    private bool m_Initialized = false;
    private Collider2D m_Collider;
    protected override Status OnStart()
    {
        if (!Initialize())
        {
            return Status.Failure;
        }

        if (m_Collider.enabled == true && SwitchState.Value == ColliderState.Disabled)
        {
            m_Collider.enabled = false;
        }
        else if (m_Collider.enabled == false && SwitchState.Value == ColliderState.Enabled)
        {
            m_Collider.enabled = true;
        }

        return Status.Success;

    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    private bool Initialize()
    {
        if (!m_Initialized)
        {
            if (Agent.Value.TryGetComponent<Collider2D>(out m_Collider))
            {
                m_Initialized = true;
            }
        }

        return m_Initialized;
    }
}

