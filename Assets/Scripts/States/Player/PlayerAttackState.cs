using TheGame;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IPlayerVulnarableState
{
    private float m_FallbackDuration = (1.0f / 32.0f) * 7.0f;
    private float m_ClipDuration = 0.0f;
    private float m_AttackTimer = 0.0f;
    public bool AttackFinished => m_AttackTimer >= m_ClipDuration;
    private Weapon m_Weapon;

    public PlayerAttackState(Player player) : base(player)
    {
        m_Weapon = player.GetComponentInChildren<Weapon>();
        if (m_Weapon == null)
        {
            Debug.LogError("Weapon component not found on Player or its children for PlayerAttackState.", player);
        }
        if (AnimationDurations.ContainsKey(AttackMeleeAnimationHash))
        {
            m_ClipDuration = AnimationDurations[AttackMeleeAnimationHash];
        }
        else
        {
            m_ClipDuration = m_FallbackDuration;
        }
    }

    public override void Enter(IState from)
    {
        m_AttackTimer = 0.0f;

        Player.TurnToCursor();

        Player.Animator.CrossFade(AttackMeleeAnimationHash, 0.0f);
        Player.Animation.OnAttackMeleeFinished.AddListener(OnAnimationFinished);
        Player.Animation.OnAttackMeleeEntered.AddListener(OnAnimationEntered);
    }

    public override void Exit()
    {
        Player.Animation.OnAttackMeleeFinished.RemoveListener(OnAnimationFinished);
        Player.Animation.OnAttackMeleeEntered.RemoveListener(OnAnimationEntered);
        m_Weapon?.DisableCollider();
    }

    public override void Update()
    {
        m_AttackTimer += Time.deltaTime;
    }

    protected void OnAnimationEntered()
    {
        m_Weapon?.EnableCollider();
    }

    protected void OnAnimationFinished()
    {
        m_Weapon?.DisableCollider();
    }
}