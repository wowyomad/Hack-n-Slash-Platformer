using Unity.VisualScripting;
using UnityEngine;
public class PlayerWalkState : PlayerBaseState
{    public PlayerWalkState(Player player) : base(player) { }

    public override void OnEnter(IState state)
    {
        Player.Animator.CrossFade(WalkAnimationHash, 0.0f);
    }
}