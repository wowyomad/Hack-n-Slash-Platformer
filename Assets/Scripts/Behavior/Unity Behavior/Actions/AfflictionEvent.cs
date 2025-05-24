using System;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;


namespace TheGame
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Behavior/Event Channels/AfflictionEvent")]
#endif
    [Serializable, GeneratePropertyBag]
    [EventChannelDescription(name: "AfflictionEvent", message: "[Agent] got aflicted by [Afliction]", category: "Events", id: "0f8437cbea803a1dfc827273790ff028")]
    public partial class AfflictionEvent : EventChannelBase
    {
        public delegate void AfflictionEventEventHandler(GameObject Agent, Affliction Afliction);
        public event AfflictionEventEventHandler Event;

        public void SendEventMessage(GameObject Agent, Affliction Afliction)
        {
            Event?.Invoke(Agent, Afliction);
        }

        public override void SendEventMessage(BlackboardVariable[] messageData)
        {
            BlackboardVariable<GameObject> AgentBlackboardVariable = messageData[0] as BlackboardVariable<GameObject>;
            var Agent = AgentBlackboardVariable != null ? AgentBlackboardVariable.Value : default(GameObject);

            BlackboardVariable<Affliction> AflictionBlackboardVariable = messageData[1] as BlackboardVariable<Affliction>;
            var Afliction = AflictionBlackboardVariable != null ? AflictionBlackboardVariable.Value : default(Affliction);

            Event?.Invoke(Agent, Afliction);
        }

        public override Delegate CreateEventHandler(BlackboardVariable[] vars, System.Action callback)
        {
            AfflictionEventEventHandler del = (Agent, Afliction) =>
            {
                BlackboardVariable<GameObject> var0 = vars[0] as BlackboardVariable<GameObject>;
                if (var0 != null)
                    var0.Value = Agent;

                BlackboardVariable<Affliction> var1 = vars[1] as BlackboardVariable<Affliction>;
                if (var1 != null)
                    var1.Value = Afliction;

                callback();
            };
            return del;
        }

        public override void RegisterListener(Delegate del)
        {
            Event += del as AfflictionEventEventHandler;
        }

        public override void UnregisterListener(Delegate del)
        {
            Event -= del as AfflictionEventEventHandler;
        }
    }


}

