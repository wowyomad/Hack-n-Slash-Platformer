using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {

    }

    public override void Update()
    {
        base.Update();

        m_Player.SetVelocity(m_XInput, m_Player.Rigidbody.linearVelocityY);

        if (m_XInput == 0)
        {
            m_StateMachine.SwitchState(m_Player.IdleState);
        }
    }
}
