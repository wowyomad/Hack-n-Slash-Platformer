using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TargetDetected")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TargetDetected", message: "[Agent] detected [Target]", category: "Events", id: "cdc916981e8534c769d5e81a15fb73db")]
public partial class TargetDetected : EventChannelBase
{
    public delegate void TargetDetectedEventHandler(GameObject Agent, GameObject Target);
    public event TargetDetectedEventHandler Event; 

    public void SendEventMessage(GameObject Agent, GameObject Target)
    {
        Event?.Invoke(Agent, Target);
    }

    public override void SendEventMessage(BlackboardVariable[] messageData)
    {
        BlackboardVariable<GameObject> AgentBlackboardVariable = messageData[0] as BlackboardVariable<GameObject>;
        var Agent = AgentBlackboardVariable != null ? AgentBlackboardVariable.Value : default(GameObject);

        BlackboardVariable<GameObject> TargetBlackboardVariable = messageData[1] as BlackboardVariable<GameObject>;
        var Target = TargetBlackboardVariable != null ? TargetBlackboardVariable.Value : default(GameObject);

        Event?.Invoke(Agent, Target);
    }

    public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
    {
        TargetDetectedEventHandler del = (Agent, Target) =>
        {
            BlackboardVariable<GameObject> var0 = vars[0] as BlackboardVariable<GameObject>;
            if(var0 != null)
                var0.Value = Agent;

            BlackboardVariable<GameObject> var1 = vars[1] as BlackboardVariable<GameObject>;
            if(var1 != null)
                var1.Value = Target;

            callback();
        };
        return del;
    }

    public override void RegisterListener(Delegate del)
    {
        Event += del as TargetDetectedEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as TargetDetectedEventHandler;
    }
}

