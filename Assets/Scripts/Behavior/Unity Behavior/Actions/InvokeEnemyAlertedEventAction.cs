using System;
using TheGame;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Invoke EnemyAlerted Event", story: "Invoke EnemyAlerted for [Agent] with [AlertedState]", category: "Action/Scene", id: "3dcb477fbac1815f4c4ddc4fee0ca566")]
public partial class InvokeEnemyAlertedEventAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<AlertedState> AlertedState;

    protected override Status OnStart()
    {
        EventBus<EnemyAlertedEvent>.Raise(new EnemyAlertedEvent
        {
            EnemyGameObject = Agent.Value,
            Alerted = AlertedState.Value == TheGame.AlertedState.Alerted
        });
        return Status.Success;
    }
    
}

