using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "EntityAlive", story: "[Entity] is [State]", category: "Conditions", id: "ed22ebf3d38257b2ef9b882854f48003")]
public partial class EntityAliveCondition : Condition
{

    public enum EntityState
    {
        Alive,
        Dead
    }
    [SerializeReference] public BlackboardVariable<Entity> Entity;

    [SerializeReference] public BlackboardVariable<EntityState> State;
    private Entity m_Entity;

    private bool m_Initialized = false;
    private void Initialize()
    {
        m_Entity = Entity.Value.GetComponent<Entity>();
        if (m_Entity == null)
        {
            Debug.LogError("No Entity Component in EntityAliveCondition");
            return;
        }

        m_Initialized = true;
    }
    public override bool IsTrue()
    {
        switch (State.Value)
        {
            case EntityState.Alive:
                return m_Entity.IsAlive;
            case EntityState.Dead:
                return !m_Entity.IsAlive;
            default:
                Debug.LogError("Invalid operator in EntityAliveCondition");
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
}
