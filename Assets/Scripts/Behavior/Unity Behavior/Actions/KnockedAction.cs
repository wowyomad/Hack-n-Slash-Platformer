using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Knocked", story: "Knocked for [Duration]", category: "Action", id: "90370c5b4418b64bfe3a745774cdf28d")]
public partial class KnockedAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Duration;
    

    private float m_ElapsedTime;

    protected override Status OnStart()
    {
        m_ElapsedTime = 0f;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_ElapsedTime += Time.deltaTime;

        if (m_ElapsedTime >= Duration.Value)
        {
            return Status.Success;
        }

        return Status.Running;
    }
}

