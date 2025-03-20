using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{

    public float JumpVelocity;
    public float JumpDecceleration;

    public PlayerJumpState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {
        Debug.Log("Player entered Jump State");
    }

    public override void OnExit()
    {
       
    }

    public override void Update()
    {
       
    }
}

