using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TestAction", story: "[Agnet] moves to [Target] with position update [Interval]", category: "Action", id: "bdca9ec90d26deca5769ed86ace47449")]
public partial class TestAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agnet;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    [SerializeReference] public BlackboardVariable<float> Interval = new BlackboardVariable<float>(0.5f);


    private NavAgent2D m_NavAgent;
    private ActionTimer m_UpdatePositionTimer = new ActionTimer();

    protected override Status OnStart()
    {
        m_NavAgent = Agnet.Value.GetComponent<NavAgent2D>();
        if(m_UpdatePositionTimer.IsRunning)
        {
            m_UpdatePositionTimer.Stop();
        } 
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_UpdatePositionTimer.Tick();
        return Status.Success;
    }

    protected override void OnEnd()
    {

    }

    private void UpdatePosition()
    {
        m_NavAgent.SetDestination(Target.Value.position);
    }
    
}
