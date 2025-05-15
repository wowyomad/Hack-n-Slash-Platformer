using System;
using TheGame;
using UnityEngine;

[RequireComponent(typeof(WeaponAnimation))]
[RequireComponent(typeof(Collider2D))]
public class Weapon : MonoBehaviour
{
    private IWeaponWielder m_Wielder;

    private Collider2D m_Collider;

    public event Action OnAttacked;

    private void Awake()
    {
        m_Wielder = GetComponentInParent<IWeaponWielder>();
        
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
        if (collision.TryGetComponent<IHittable>(out var target))
        {
            m_Wielder.TryHitTarget(target);
        }
    }
}
