using Unity.VisualScripting;
using UnityEngine;
public class PlayerWalkState : PlayerBaseState
{    public PlayerWalkState(Player player) : base(player) { }

    public override void OnEnter(IState from)
    {      
        Debug.Log("Player entered Walk State");
        //Player.Animator.CrossFade(WalkAnimationHash, 0.0f);

        //if from.GetType() was idle, first play StartWalkAnimationHash
        if(from is PlayerIdleState)
        {
            Player.Animator.SetBool("IsWalking", true);
        }

    }

    public override void OnExit()
    {
        Player.Animator.SetBool("IsWalking", false);
    }

    public override void Update()
    {
        Player.Controller.Move(Player.Input.Movement * new Vector2(5.0f, 0.0f));
    }

}