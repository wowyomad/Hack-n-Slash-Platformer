using TheGame;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerWeapon : MonoBehaviour
{
    private Player m_Player;

    private Collider2D m_Collider;
    private PlayerAnimationEvents m_AnimationEvents;

    private void Awake()
    {
        m_Player = GetComponentInParent<Player>();
        m_Collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        m_Collider.enabled = false;
        m_AnimationEvents = m_Player.AnimationEvents;
        if (m_AnimationEvents == null)
        {
            Debug.LogError("PlayerAnimationEvents not found on player :/");
        }
    }

    public void SubscribeToAttackAnimationEvents()
    {
        if (m_AnimationEvents == null) return;

        m_AnimationEvents.AttackMeleeEntered += OnAttackMeleeEntered;
        m_AnimationEvents.AttackMeleeFinished += OnAttackMeleeFinished;
    }

    public void UnsubscribeFromAttackAnimationEvents()
    {
        if (m_AnimationEvents == null) return;

        m_AnimationEvents.AttackMeleeEntered -= OnAttackMeleeEntered;
        m_AnimationEvents.AttackMeleeFinished -= OnAttackMeleeFinished;
    }

    private void OnAttackMeleeEntered()
    {
        EnableCollider();
    }
    private void OnAttackMeleeFinished()
    {
        DisableCollider();
    }

    public void EnableCollider()
    {
        m_Collider.enabled = true;
    }

    public void DisableCollider()
    {
        m_Collider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Hit {collision.tag}");
        if (collision.TryGetComponent<IHittable>(out var target))
        {
            target.TakeHit();
            //Player.Attack(target);
        }
    }
}
