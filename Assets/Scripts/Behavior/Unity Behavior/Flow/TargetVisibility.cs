using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

#if UNITY_EDITOR
[CreateAssetMenu(menuName = "Behavior/Event Channels/TargetVisibility")]
#endif
[Serializable, GeneratePropertyBag]
[EventChannelDescription(name: "TargetVisibility", message: "[Agent] has spotted [Target]", category: "Events", id: "19aabf99ea04e7f7e9cac72dc608241a")]
public partial class TargetVisibility : EventChannelBase
{
    public delegate void TargetVisibilityEventHandler(GameObject Agent, GameObject Target);
    public event TargetVisibilityEventHandler Event; 

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
        TargetVisibilityEventHandler del = (Agent, Target) =>
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
        Event += del as TargetVisibilityEventHandler;
    }

    public override void UnregisterListener(Delegate del)
    {
        Event -= del as TargetVisibilityEventHandler;
    }
}

