using UnityEngine;
using TheGame;
using System.Collections.Generic;
public abstract class PlayerBaseState : IPlayerState
{
    protected CharacterController2D Controller => Player.Controller;
    protected void Trigger(Player.Trigger trigger) => Player.StateMachine.Fire(trigger);

    public Player Player { get; private set; }

    public PlayerBaseState(Player player)
    {
        Player = player;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate() { }
    public virtual void FixedUpdate() { }

}
