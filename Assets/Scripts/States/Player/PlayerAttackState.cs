using TheGame;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState, IPlayerVulnarableState
{
    private float m_StateDuration;
    private float m_AttackTimer = 0.0f;
    public bool AttackFinished => m_AttackTimer >= m_StateDuration;
    private PlayerWeapon m_Weapon;

    public PlayerAttackState(Player player, float attackDuration = 0.5f) : base(player)
    {
        m_StateDuration = attackDuration;
        m_Weapon = player.GetComponentInChildren<PlayerWeapon>(); 
        if (m_Weapon == null)
        {
            Debug.LogError("Weapon component not found on Player or its children for PlayerAttackState.", player);
        }
    }

    public override void OnEnter()
    {
        m_AttackTimer = 0.0f;
        Player.TurnToCursor();

        m_Weapon?.SubscribeToAttackAnimationEvents();
    }

    public override void OnExit()
    {
        // Tell the weapon to stop listening and ensure collider is off
        m_Weapon?.UnsubscribeFromAttackAnimationEvents();
        m_Weapon?.DisableCollider(); // Failsafe
    }

    public override void OnUpdate()
    {
        m_AttackTimer += Time.deltaTime;
    }
}