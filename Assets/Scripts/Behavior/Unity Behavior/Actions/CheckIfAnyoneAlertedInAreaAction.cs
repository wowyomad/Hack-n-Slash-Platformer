using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;
using System.Linq;
using TheGame;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckIfAnyoneAlertedInArea", story: "Check rectangle area of [Size] near [Agent]", category: "Action/Blackboard", id: "bc8ccabb80a47e27ef1b4b1fc7479784")]
public partial class CheckIfAnyoneAlertedInAreaAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2> Size;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [SerializeReference] public BlackboardVariable<bool> AnyoneAlertedInArea;

    [SerializeReference] public BlackboardVariable<float> UpdateInterval = new BlackboardVariable<float>(0.5f);
    [SerializeReference] public BlackboardVariable<List<string>> Layers = new BlackboardVariable<List<string>>(new List<string> { "Enemy" });

    private float m_LastUpdateTime;
    private LayerMask m_Mask;

    private bool m_Initialized = false;
    
    protected override Status OnStart()
    {
        if (!Initialize())
        {
            Debug.LogError("CheckIfAnyoneAlertedInAreaAction: Initialization failed. Please check the layers and agent settings.");
            return Status.Failure;
        }

        if (Time.time - m_LastUpdateTime < UpdateInterval.Value)
        {
            return AnyoneAlertedInArea.Value ? Status.Success : Status.Failure;
        }
        m_LastUpdateTime = Time.time;
        AnyoneAlertedInArea.Value = false;

        List<Collider2D> witnesses = Physics2D.OverlapBoxAll(Agent.Value.transform.position, Size.Value, 0.0f, m_Mask).ToList();

        foreach (Collider2D witness in witnesses)
        {
            if (witness.gameObject == Agent.Value) continue;

            if (witness.TryGetComponent<Enemy>(out var friend) && friend.IsAlive && friend.IsAlerted)
            {
                AnyoneAlertedInArea.Value = true;
                return Status.Success;
            }
        }
        return Status.Failure;
    }

    private bool Initialize()
    {
        if (!m_Initialized)
        {
            m_Mask = 0;
            foreach (var layer in Layers.Value)
            {
                int layerIndex = LayerMask.NameToLayer(layer);
                if (layerIndex != -1)
                {
                    m_Mask |= 1 << layerIndex;
                }
                else
                {
                    Debug.LogWarning($"Layer '{layer}' not found. Skipping.");
                    break;
                }
            }
            m_Initialized = true;
        }
        return m_Initialized;
    }
}

