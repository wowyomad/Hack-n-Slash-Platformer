using System;
using TheGame;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[RequireComponent(typeof(WeaponAnimation))]
[RequireComponent(typeof(Collider2D))]
public class Weapon : MonoBehaviour
{
    private Player m_Player;

    private Collider2D m_Collider;

    public event Action<Vector3, Vector3> OnAttackedDirected;
    public event Action OnAttacked;

    private void Awake()
    {
        m_Player = GetComponentInParent<Player>();
        
        m_Collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        m_Collider.enabled = false;
    }


    public void EnableCollider()
    {
        m_Collider.enabled = true;
    }

    public void DisableCollider()
    {
        m_Collider.enabled = false;
    }

    public void Attack(Vector3 direction)
    {
        RotateInDirection(direction);
        EnableCollider();
        
        OnAttacked?.Invoke();
        OnAttackedDirected?.Invoke(m_Player.transform.position, direction);
    }

    public void Stop()
    {
        DisableCollider();
    }

    private void RotateInDirection(Vector3 direction)
    {
        if (direction == Vector3.zero) return;
        float angle = Mathf.Atan2(direction.y, Mathf.Abs(direction.x)) * Mathf.Rad2Deg;
        transform.parent.localRotation = Quaternion.Euler(0, 0, angle);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Hit {collision.tag}");
        if (collision.TryGetComponent<IHittable>(out var target))
        {
            m_Player.TryHitTarget(target);
        }
    }
}
