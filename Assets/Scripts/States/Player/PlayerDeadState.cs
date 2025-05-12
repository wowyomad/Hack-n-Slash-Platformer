using UnityEngine;



public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(Player player) : base(player) { }

    public override void OnEnter()
    {
        EventBus<PlayerDeadEvent>.Raise(new PlayerDeadEvent());
        Debug.Log("Player is dead");
    }
}


public struct PlayerDeadEvent : IEvent
{
   
}