using System;
using UnityEngine;

namespace TheGame
{
    [RequireComponent(typeof(Collider2D))]
    public class MeleeWeapon : MonoBehaviour
    {
        private Collider2D m_Collider;
        private MeleeCombat m_CombatController;


        public event Action OnAttacked;

        public void SetCombatController(MeleeCombat combatController)
        {
            m_CombatController = combatController;
        }

        private void Awake()
        {
            m_CombatController = GetComponentInParent<MeleeCombat>();
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
            transform.localRotation = Quaternion.Euler(0, 0, angle);

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent<IHittable>(out var target) && target as MonoBehaviour != transform.parent.gameObject)
            {
                m_CombatController.HandleWeaponTrigger(target);
            }
        }
    }
}