using UnityEngine;

public class PlayerStunnedState : PlayerBaseState
{
    private float m_StunDuration;
    public bool IsStunned => m_Stunned;
    private bool m_Stunned = false;
    private float m_StunTimer = 0.0f;

    public PlayerStunnedState(Player player, float stunDuration) : base(player)
    {
        m_StunDuration = stunDuration;
    }

    public override void OnEnter()
    {
        m_StunTimer = 0.0f;
        m_Stunned = true;
    }

    public override void OnUpdate()
    {
        if (m_StunTimer >= m_StunDuration)
        {
            m_Stunned = false;
        }
        else
        {
            m_StunTimer += Time.deltaTime;
        }

    }

    public override void OnExit()
    {
        m_Stunned = false;
    }
}

public class PlayerDeadState : PlayerBaseState
{
    public PlayerDeadState(Player player) : base(player) { }

    public override void OnEnter()
    {
        //Die animation, show death screen.
    }
}
