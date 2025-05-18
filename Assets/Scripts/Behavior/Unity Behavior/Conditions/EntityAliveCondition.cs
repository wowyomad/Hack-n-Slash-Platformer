using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "EntityAlive", story: "[Entity] is alive", category: "Conditions", id: "ed22ebf3d38257b2ef9b882854f48003")]
public partial class EntityAliveCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Entity;
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
        return m_Entity != null && m_Entity.IsAlive;
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
