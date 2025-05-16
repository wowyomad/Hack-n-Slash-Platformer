using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CanSeePlayer", story: "[Agent] can see [Player]", category: "Conditions", id: "ef6240ba72dbd5124ca03cc8a410f7ca")]
public partial class CanSeePlayerCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    private Enemy m_Self;
    private Player m_Player;

    private bool m_Initialized = false;

    public override bool IsTrue()
    {
        return m_Self.CanSeePlayer(m_Player);
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
