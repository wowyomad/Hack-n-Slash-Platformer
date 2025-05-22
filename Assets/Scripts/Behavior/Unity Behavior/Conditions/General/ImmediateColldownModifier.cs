using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Modifier = Unity.Behavior.Modifier;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Immediate Cooldown",
                 description: "Imposes a mandatory wait time before each execution, including the first, using standard serialization.",
                 story: "Cooldown for [Duration] seconds before each execution",
                 category: "Flow",
                 id: "e9443708468f9b3f185cfb54ead68651")]
public partial class ImmediateCooldownModifier : Modifier
{
    [SerializeReference] public BlackboardVariable<float> Duration;

    [SerializeField] private float m_CooldownRemainingTimeOnSave;
    [SerializeField] private bool m_InitialCooldownAppliedOnSave;

    private float m_CooldownEndTime;
    private bool m_RuntimeInitialized = false;

    private bool m_RestoreCooldownFromSavedState = false;


    protected override Status OnStart()
    {
        if (!m_InitialCooldownAppliedOnSave && !m_RuntimeInitialized)
        {
            m_CooldownEndTime = Time.time + Duration.Value;
            m_RuntimeInitialized = true;
            m_InitialCooldownAppliedOnSave = true;
            return Status.Failure;
        }

        if (m_RestoreCooldownFromSavedState)
        {
            if (m_CooldownRemainingTimeOnSave > 0)
            {
                m_CooldownEndTime = Time.time + m_CooldownRemainingTimeOnSave;
                Debug.Log($"ImmediateCooldownModifier: Restoring active cooldown from save: {m_CooldownRemainingTimeOnSave}s remaining.");
            }
            else
            {
                m_CooldownEndTime = 0f;
                Debug.Log("ImmediateCooldownModifier: Cooldown was over when saved.");
            }
            m_RestoreCooldownFromSavedState = false;
            if (!m_RuntimeInitialized) m_RuntimeInitialized = true;
        }


        if (Time.time < m_CooldownEndTime)
        {
            return Status.Failure;
        }

        m_CooldownEndTime = Time.time + Duration.Value;

        if (Child == null)
        {
            return Status.Success;
        }

        var status = StartNode(Child);
        if (status == Status.Running)
        {
            return Status.Waiting;
        }
        return status;
    }

    protected override Status OnUpdate()
    {
        if (Child == null)
        {
            return Status.Success;
        }

        var status = Child.CurrentStatus;
        if (status == Status.Running)
        {
            return Status.Waiting;
        }

        return status;
    }

    protected override void OnEnd()
    {

    }

    protected override void OnSerialize()
    {
        m_CooldownRemainingTimeOnSave = Mathf.Max(0f, m_CooldownEndTime - Time.time);

        m_InitialCooldownAppliedOnSave = m_RuntimeInitialized;
    }

    protected override void OnDeserialize()
    {
        m_RuntimeInitialized = m_InitialCooldownAppliedOnSave;

        m_RestoreCooldownFromSavedState = m_InitialCooldownAppliedOnSave;
    }
}