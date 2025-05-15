using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UpdateAlertedState", story: "Update [Alerted] state from [Agent] and [Target]", category: "Action", id: "88479ed90ba1fd8670874c258f14dda8")]
public partial class UpdateAlertedStateAction : Action
{
    [SerializeReference] public BlackboardVariable<bool> Alerted;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<float> UpdatePeriod = new BlackboardVariable<float>(0.5f);


    private float m_UpdateTimer;

    public Enemy Self;

    protected override Status OnStart()
    {
        Self = Agent.Value.GetComponent<Enemy>();
        m_UpdateTimer = 0f;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        m_UpdateTimer += Time.deltaTime;
        if (m_UpdateTimer < UpdatePeriod.Value)
        {
            if (Target.Value.TryGetComponent<Player>(out var player))
            {
                if (player.CurrentState is PlayerDeadState)
                {
                    Alerted.Value = false;
                }
                else
                {
                    Alerted.Value = Self.CanSeePlayer(Target.Value.transform.position);
                }
                return Status.Success;
            }
            return Status.Running;
        }
        return Status.Failure;

    }
}

