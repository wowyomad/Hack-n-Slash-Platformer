using UnityEngine;

public abstract class PlayerBaseState : IState
{
    protected readonly Animator Animator;
    protected readonly PlayerController Controller;
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
}
