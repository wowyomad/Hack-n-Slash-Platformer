using UnityEngine;
using UnityEngine.XR;

public abstract class PlayerBaseState : IState
{
    protected CharacterController2D Controller => Player.Controller;
    protected InputReader Input => Player.Input;
    protected void ChangeState(IState state) => Player.ChangeState(state); 

    public Player Player { get; private set; }

    protected static readonly int IdleAnimationHash = Animator.StringToHash("Idle");
    protected static readonly int JumpAnimationHash = Animator.StringToHash("Jump");
    protected static readonly int WalkAnimationHash = Animator.StringToHash("Walk");
    protected static readonly int RunAnimationHash = Animator.StringToHash("Run");
    protected static readonly int AirAnimationHash = Animator.StringToHash("Air");

    public PlayerBaseState(Player player)
    {
        Player = player;
    }

    public virtual void OnEnter(IState from) { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}
