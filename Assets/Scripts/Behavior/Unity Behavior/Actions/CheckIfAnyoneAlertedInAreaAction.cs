using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.VisualScripting;
using System.Linq;
using TheGame;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CheckIfAnyoneAlertedInArea", story: "Check rectangle area of [Size] near [Agent]", category: "Action/Blackboard", id: "bc8ccabb80a47e27ef1b4b1fc7479784")]
public partial class CheckIfAnyoneAlertedInAreaAction : Action
{
    [SerializeReference] public BlackboardVariable<Vector2> Size;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [SerializeReference] public BlackboardVariable<bool> AnyoneAlertedInArea;

    [SerializeReference] public BlackboardVariable<float> UpdateInterval = new BlackboardVariable<float>(0.5f);

    private float m_LastUpdateTime;

    protected override Status OnStart()
    {
        if (Time.time - m_LastUpdateTime < UpdateInterval.Value)
        {
            return AnyoneAlertedInArea.Value ? Status.Success : Status.Failure;
        }
        m_LastUpdateTime = Time.time;
        AnyoneAlertedInArea.Value = false;

        var friends = Physics2D.OverlapBoxAll(Agent.Value.transform.position, Size.Value, 0.0f, Agent.Value.layer).ToList();

        foreach (var friend in friends)
        {
            if (friend == Agent.Value) continue;
            
            if (friend.TryGetComponent<Enemy>(out var f) && f.IsAlive && f.IsAlerted)
            {
                AnyoneAlertedInArea.Value = true;
                return Status.Success;
            }
        }
        return Status.Failure;
    }
}

