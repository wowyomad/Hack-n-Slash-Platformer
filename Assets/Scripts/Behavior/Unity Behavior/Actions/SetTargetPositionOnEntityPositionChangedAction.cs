using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Reflection;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Target Position On Entity Position Changed", story: "Update [Transform] position if [MovingTarget] position changed within [Duration] seconds relative to [Agent]", category: "Action", id: "a2c73533260f2aeb7dc665e0e7884297")]
public partial class SetTargetPositionOnEntityPositionChangedAction : Action
{
    [SerializeReference] public BlackboardVariable<Transform> Transform;
    [SerializeReference] public BlackboardVariable<Transform> MovingTarget;
    [SerializeReference] public BlackboardVariable<Transform> Agent;
    [SerializeReference] public BlackboardVariable<float> Duration = new BlackboardVariable<float>(0.0f);
    [SerializeReference] public BlackboardVariable<float> MaximumDistance = new BlackboardVariable<float>(20.0f);
    [SerializeReference] public BlackboardVariable<float> PositionChangedThreshold = new BlackboardVariable<float>(0.1f);


    private Vector3 m_LastPosition;
    private float m_InitialDistance;
    private bool m_HasMoved = false;
    private float m_Timer = 0.0f;
    protected override Status OnStart()
    {
        float distance = Vector3.Distance(Agent.Value.position, MovingTarget.Value.position);

        if (distance > MaximumDistance.Value)
        {
            return Status.Failure;
        }

        m_LastPosition = MovingTarget.Value.position;

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float distance = Vector3.Distance(m_LastPosition, MovingTarget.Value.position);
        m_Timer += Time.deltaTime;
        if (m_Timer >= Duration.Value || distance > MaximumDistance.Value)
        {
            if (m_HasMoved)
            {
                return Status.Success;
            }
            else
            {
                return Status.Failure;
            }
        }
        else if (distance > PositionChangedThreshold.Value && distance <= MaximumDistance.Value)
        {
            m_HasMoved = true;
            m_LastPosition = MovingTarget.Value.position;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (m_HasMoved && MovingTarget.Value != null)
        {
            Assign(MovingTarget.Value.position);
        }

        m_Timer = 0.0f;
        m_LastPosition = Vector3.zero;
        m_InitialDistance = 0.0f;
        m_HasMoved = false;
    }

    private void Assign(Vector3 position)
    {
        Transform.Value.position = position;

        Type type = Transform.GetType();
        MethodInfo invokeMethod = type.GetMethod("InvokeValueChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        invokeMethod.Invoke(Transform, null);
    }
}

