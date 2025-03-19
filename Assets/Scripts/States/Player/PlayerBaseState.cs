using UnityEngine;

public abstract class PlayerBaseState : IState
{
    public Player Player { get; private set; }

    protected static readonly int IdleAnimationHash = Animator.StringToHash("Player_Idle");
    protected static readonly int JumpAnimationHash = Animator.StringToHash("Player_Jump");
    protected static readonly int WalkAnimationHash = Animator.StringToHash("Player_Walk");
    protected static readonly int RunAnimationHash = Animator.StringToHash("Player_Run");
    protected static readonly int AirAnimationHash = Animator.StringToHash("Player_Air");

    public PlayerBaseState(Player player)
    {
        Player = player;
    }

    public virtual void OnEnter(IState from) { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
