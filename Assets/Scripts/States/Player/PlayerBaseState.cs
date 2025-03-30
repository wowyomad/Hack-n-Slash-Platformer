using UnityEngine;

public abstract class PlayerBaseState : IPlayerState
{
    protected CharacterController2D Controller => Player.Controller;
    protected InputReader Input => Player.Input;
    protected void ChangeState(IPlayerState state) => Player.ChangeState(state);

    public Player Player { get; private set; }

    protected static readonly int IdleAnimationHash = Animator.StringToHash("Idle");
    protected static readonly int JumpAnimationHash = Animator.StringToHash("Jump");
    protected static readonly int WalkAnimationHash = Animator.StringToHash("Walk");
    protected static readonly int RunAnimationHash = Animator.StringToHash("Run");
    protected static readonly int AirAnimationHash = Animator.StringToHash("Air");
    protected static readonly int AttackMeleeAnimationHash = Animator.StringToHash("AttackMelee");

    public PlayerBaseState(Player player)
    {
        Player = player;
    }

    public virtual void Enter(IState from) { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }

}
