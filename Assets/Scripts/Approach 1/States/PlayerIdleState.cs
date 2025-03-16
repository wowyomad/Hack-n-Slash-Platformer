using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {

    }

    public override void Update()
    {
        base.Update();

        if(m_XInput != 0)
        {
            m_StateMachine.SwitchState(m_Player.MoveState);
        }
    }
}
