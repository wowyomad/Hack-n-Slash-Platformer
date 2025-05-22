using System;
using Unity.Behavior;
using UnityEngine;

namespace TheGame
{

    [Serializable, Unity.Properties.GeneratePropertyBag]
    [Condition(name: "CanSeePlayer", story: "[Agent] can see [Player] while [IsAlerted]", category: "Conditions", id: "ef6240ba72dbd5124ca03cc8a410f7ca")]
    public partial class CanSeePlayerCondition : Condition
    {

        [SerializeReference] public BlackboardVariable<GameObject> Agent;
        [SerializeReference] public BlackboardVariable<GameObject> Player;
        [SerializeReference] public BlackboardVariable<AlertedState> IsAlerted;

        private Enemy m_Self;
        private Player m_Player;

        private bool m_Initialized = false;

        public override bool IsTrue()
        {
            switch (IsAlerted.Value)
            {
                case AlertedState.Alerted:
                    return m_Self.CanSeeEntity(m_Player, true);
                case AlertedState.NotAlerted:
                    return m_Self.CanSeeEntity(m_Player, false);
                default:
                    Debug.LogError("Invalid operator in CanSeePlayerCondition");
                    return false;
            }
        }

        public override void OnStart()
        {
            if (!m_Initialized)
            {
                Initialize();
            }
        }

        public override void OnEnd()
        {
            
        }

        private void Initialize()
        {
            m_Self = Agent.Value.GetComponent<Enemy>();
            m_Player = Player.Value.GetComponent<Player>();
            m_Initialized = true;
        }
    }

}