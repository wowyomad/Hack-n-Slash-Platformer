using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine m_StateMachine;
    protected Player m_Player;
    protected string m_AnimationBoolName;

    protected float m_XInput = 0.0f;

    public PlayerState(Player player, PlayerStateMachine stateMachine, string animationBoolName)
    {
        m_Player = player;
        m_StateMachine = stateMachine;
        m_AnimationBoolName = animationBoolName;

    }

    public virtual void Enter()
    {
        Debug.Log("Entering " + m_AnimationBoolName);
        m_Player.Animator.SetBool(m_AnimationBoolName, true);
    }

    public virtual void Update()
    {
        m_XInput = Input.GetAxisRaw("Horizontal");
    }

    public virtual void Exit()
    {
        Debug.Log("Exiting " + m_AnimationBoolName);
        m_Player.Animator.SetBool(m_AnimationBoolName, false);
    }

}
