

using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private IState m_PreviousState;
    private float m_FallbackDuration = 0.4375f;
    private float m_Timer = 0.0f;
    private Weapon m_Weapon;
    public PlayerAttackState(Player player) : base(player)
    {
        m_Weapon = player.GetComponentInChildren<Weapon>();
    }

    public override void Enter(IState from)
    {
        m_PreviousState = from;
        m_Timer = 0.0f;

        Player.TurnToCursor();

        Player.Animator.CrossFade(AttackMeleeAnimationHash, 0.0f);
        Player.Animation.OnAttackMeleeFinished.AddListener(OnAnimationFinished);
    }

    public override void Exit()
    {
        Player.Animation.OnAttackMeleeFinished.RemoveListener(OnAnimationFinished);
    }

    public override void Update()
    {
        if (m_Timer >= m_FallbackDuration)
        {
            ChangeState(Player.IdleState);
            return;
        }
        m_Timer += Time.deltaTime;
        Player.ApplyGravity();
    }

    protected void OnAnimationFinished()
    {
        ChangeState(Player.IdleState);
    }
}