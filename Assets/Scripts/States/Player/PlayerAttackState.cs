

using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IPlayerVulnarableState
{
    private IPlayerState m_PreviousState;
    private float m_FallbackDuration = 1.0f / 32 * 7; //(Second/SampleRate) * Frames 
    private float m_Timer = 0.0f;
    private Weapon m_Weapon;
    public PlayerAttackState(Player player) : base(player)
    {
        m_Weapon = player.GetComponentInChildren<Weapon>();
    }

    public override void Enter(IState from)
    {
        m_PreviousState = (IPlayerState)from;
        m_Timer = 0.0f;

        Player.TurnToCursor();

        Player.Animator.CrossFade(AttackMeleeAnimationHash, 0.0f);
        Player.Animation.OnAttackMeleeFinished.AddListener(OnAnimationFinished);
        Player.Animation.OnAttackMeleeEntered.AddListener(OnAnimationEntered);
    }

    public override void Exit()
    {
        Player.Animation.OnAttackMeleeFinished.RemoveListener(OnAnimationFinished);
        Player.Animation.OnAttackMeleeEntered.RemoveListener(OnAnimationEntered);
    }

    public override void Update()
    {
        if (m_Timer >= m_FallbackDuration)
        {
            ChangeState(m_PreviousState);
            return;
        }
        m_Timer += Time.deltaTime;
        Player.ApplyGravity();
    }
    protected void OnAnimationEntered()
    {
        m_Weapon.EnableCollider();
    }
    protected void OnAnimationFinished()
    {
        m_Weapon.DisableCollider();
    }
}