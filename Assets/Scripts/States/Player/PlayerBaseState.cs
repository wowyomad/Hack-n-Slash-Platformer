using UnityEngine;
using TheGame;
using System.Collections.Generic;
public abstract class PlayerBaseState : IPlayerState
{
    protected CharacterController2D Controller => Player.Controller;
    protected void Trigger(Player.Trigger trigger) => Player.StateMachine.Fire(trigger);

    public Player Player { get; private set; }

    protected static readonly int IdleAnimationHash = Animator.StringToHash("Idle");
    protected static readonly int JumpAnimationHash = Animator.StringToHash("Jump");
    protected static readonly int WalkAnimationHash = Animator.StringToHash("Walk");
    protected static readonly int RunAnimationHash = Animator.StringToHash("Run");
    protected static readonly int AirAnimationHash = Animator.StringToHash("Air");
    protected static readonly int AttackMeleeAnimationHash = Animator.StringToHash("AttackMelee");

    protected static Dictionary<int, float> AnimationDurations = null;

    public PlayerBaseState(Player player)
    {
        Player = player;

        if (AnimationDurations == null)
        {
            AnimationDurations = new Dictionary<int, float>();
            var clips = Player.Animator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == "Idle")
                    AnimationDurations[IdleAnimationHash] = clip.length;
                else if (clip.name == "Fall")
                    AnimationDurations[JumpAnimationHash] = clip.length;
                else if (clip.name == "Jump")
                    AnimationDurations[JumpAnimationHash] = clip.length;
                else if (clip.name == "Walk")
                    AnimationDurations[WalkAnimationHash] = clip.length;
                else if (clip.name == "Run")
                    AnimationDurations[RunAnimationHash] = clip.length;
                else if (clip.name == "Air")
                    AnimationDurations[AirAnimationHash] = clip.length;
                else if (clip.name == "AttackMelee")
                    AnimationDurations[AttackMeleeAnimationHash] = clip.length;
            }
        }
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate() { }
    public virtual void FixedUpdate() { }

}
