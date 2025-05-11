using TheGame;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState, IPlayerVulnarableState
{
    public enum EntryMode
    {
        Start,
        Resume
    }

    protected float m_VelocitySmoothing = 0.0f;
    public float JumpVelocity => Player.Stats.JumpVelocity;
    public EntryMode Mode = EntryMode.Start;
    public PlayerJumpState(Player player) : base(player) { }
    
    public override void OnEnter()
    {
        if (Mode == EntryMode.Start)
        {
            Controller.Velocity.y = JumpVelocity;
        }
    }

    public override void OnExit()
    {
        Mode = EntryMode.Start;   
    }

    public override void OnUpdate()
    {
        float moveInput = Player.Input.Horizontal;
        
        Player.Flip(moveInput);
        

        float targetVelocityX = moveInput * Player.Stats.HorizontalSpeed;
        Controller.Velocity.x = Mathf.SmoothDamp(Controller.Velocity.x, targetVelocityX, ref m_VelocitySmoothing, Player.Stats.AccelerationTimeAirborne);

    }
}

