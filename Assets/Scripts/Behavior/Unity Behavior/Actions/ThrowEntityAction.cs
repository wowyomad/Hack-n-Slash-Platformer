using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ThrowEntity", story: "Throw [Entity] in [Direction] with [Impulse]", category: "Action", id: "b961ece27f9be40216824b5c976d10bc")]
public partial class ThrowEntityAction : Action
{
    [SerializeReference] public BlackboardVariable<Entity> Entity;
    [SerializeReference] public BlackboardVariable<Vector2> Direction;
    [SerializeReference] public BlackboardVariable<float> Impulse;

    [SerializeReference] public BlackboardVariable<float> Duration = new BlackboardVariable<float>(0.15f);
    private CharacterController2D m_CharacterController;

    private bool m_Initialized = false;
    private float m_Timer;
    protected override Status OnStart()
    {
        if (!Initialize())
        {
            return Status.Failure;
        }

        m_Timer = 0.0f;

        float xVelocity = Direction.Value.x * Impulse.Value;
        float yVelocity = Direction.Value.y * Impulse.Value;

        m_CharacterController.Velocity.x = xVelocity;
        m_CharacterController.Velocity.y = yVelocity;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (m_Timer < Duration.Value)
        {
            m_Timer += Time.deltaTime;
            return Status.Running;
        }
        else
        {
            m_CharacterController.Velocity.x = 0.0f;
            return Status.Success;
        }
    }


    private bool Initialize()
    {
        if (!m_Initialized)
        {
            m_CharacterController = Entity.Value.GetComponent<CharacterController2D>();
            m_Initialized = true;
        }

        return m_Initialized;
    }
}

