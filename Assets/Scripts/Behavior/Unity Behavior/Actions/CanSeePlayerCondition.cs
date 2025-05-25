using System;
using TheGame.EnemySensor;
using Unity.Behavior;
using UnityEngine;

namespace TheGame
{

    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "Can see Entity", story: "[Agent] can see [Target]", category: "Conditions", id: "ef6240ba72dbd5124ca03cc8a410f7ca")]
    public partial class CanSeeEntityCondition : Condition
    {

        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Target;

        protected VisualSensor Sensor;

        private bool m_Initialized = false;

        public override bool IsTrue()
        {
            return Sensor.Check(Target.Value);
        }

        public override void OnStart()
        {
            if (!m_Initialized && !Initialize())
            {
                return;
            }
        }

        public override void OnEnd()
        {

        }

        private bool Initialize()
        {
            Sensor = Agent.Value.GetComponent<VisualSensor>();
            if (Sensor == null)
            {
                Debug.LogError($"[CanSeePlayerCondition] {Agent.Value.name} does not have a VisualSensor component.");
                return false;
            }
            m_Initialized = true;
            return true;
        }
    }

}