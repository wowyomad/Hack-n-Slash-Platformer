using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetAnimationTriggerBetter", story: "Trigger [Trigger] in [Agent] animator", category: "Action", id: "edb4c3333d00e2a2d3a2ff178efc5bb1")]
public partial class SetAnimationTriggerBetterAction : Action
{
    [SerializeReference] public BlackboardVariable<string> Trigger;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private int m_TriggerHash;
    private Animator m_Animator;
    private bool m_Initialized = false;

    protected override Status OnStart()
    {
        if (!Initialize())
        {
            return Status.Failure;
        }

        m_Animator.SetTrigger(m_TriggerHash);
        return Status.Running;
    }

    private bool Initialize()
    {
        if (!m_Initialized)
        {
            m_Animator = Agent.Value.GetComponentInChildren<Animator>();
            m_TriggerHash = Animator.StringToHash(Trigger.Value);
            m_Initialized = true;
        }

        return m_Initialized;
    }
}

